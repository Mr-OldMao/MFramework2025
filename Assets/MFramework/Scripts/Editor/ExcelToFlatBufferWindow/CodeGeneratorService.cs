using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class CodeGeneratorService
{
    /// <summary>
    /// 基本数据类型数组分割符
    /// </summary>
    private char[] m_ArrSplits = { ',', '，', ':' };

    private GenerationConfig _config;
    private Dictionary<string, CustomDataType> m_DicCustomDataType;

    public class CustomDataType
    {
        public string typeName;
        //key-name value-type
        public Dictionary<string, string> fields;
        public bool isStruet;
    }

    public CodeGeneratorService(GenerationConfig config)
    {
        _config = config;
    }

    public string GenerateFbsContent(ExcelTableData tableData)
    {
        var sb = new StringBuilder();

        string tableName = $"FB_{tableData.tableName}_{tableData.workbookName}";
        List<string> customDataTypeList = new List<string>();
        sb.AppendLine("namespace GameMain.Generate.FlatBuffers;");
        sb.AppendLine();
        sb.AppendLine($"table {tableName}");
        sb.AppendLine("{");

        for (int i = 0; i < tableData.fieldNames.Length; i++)
        {
            if (!string.IsNullOrEmpty(tableData.fieldNames[i]) && !string.IsNullOrEmpty(tableData.fieldTypes[i]))
            {
                string fieldName = tableData.fieldNames[i].Trim();
                string fieldType = ConvertToFlatBufferType(tableData.fieldTypes[i].Trim());
                sb.AppendLine($"\t{fieldName}:{fieldType};");
                fieldType = fieldType.Replace("[", "").Replace("]", "");
                if (IsCustomDataType(fieldType) && !customDataTypeList.Contains(fieldType))
                {
                    customDataTypeList.Add(fieldType);
                }
            }
        }
        sb.AppendLine("}");

        if (customDataTypeList?.Count > 0)
        {
            sb.AppendLine(GetCustomDataType(customDataTypeList.ToArray()));
        }

        sb.AppendLine();
        sb.AppendLine($"table {tableName}_Array");
        sb.AppendLine("{");
        sb.AppendLine($"\tdatas:[{tableName}];");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine($"root_type {tableName}_Array;");

        return sb.ToString();
    }

    public string GenerateJsonContent(ExcelTableData tableData)
    {
        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine("\t\"datas\": [");

        for (int i = 0; i < tableData.dataRows.Length; i++)
        {
            if (tableData.dataRows[i] == null) continue;

            sb.AppendLine("\t{");
            int minCount = Math.Min(tableData.fieldNames.Length, tableData.dataRows[i].Length);
            for (int j = 0; j < minCount; j++)
            {
                if (string.IsNullOrEmpty(tableData.fieldNames[j])) continue;

                string fieldName = tableData.fieldNames[j].Trim();
                string fieldType = j < tableData.fieldTypes.Length ? tableData.fieldTypes[j].Trim() : "string";
                string value = tableData.dataRows[i][j].Trim();

                sb.Append($"\t\t\"{fieldName}\": {FormatValueForJson(value, fieldType, fieldName, tableData)}");
                sb.AppendLine(j < minCount-1 ? "," : "");
            }
            if (i < tableData.dataRows.Length - 1)
            {
                sb.AppendLine("\t},");
            }
            else
            {
                sb.Append("\t}");
            }
        }
        sb.AppendLine("]");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private string ConvertToFlatBufferType(string type)
    {
        if (type.StartsWith("[") && type.EndsWith("]"))
        {
            string innerType = type.Substring(1, type.Length - 2);
            return $"[{ConvertToFlatBufferType(innerType)}]";
        }

        switch (type.ToLower())
        {
            case "int": return "int";
            case "float": return "float";
            case "bool": return "bool";
            case "string":
            case "repeated string":
                return "string";
            case "long": return "long";
            case "double": return "double";
            case "byte": return "byte";
            case "ubyte": return "ubyte";
            case "short": return "short";
            //case "prop":
            //    return "Prop";
            default:

                var customStruct = m_DicCustomDataType.Where(p => p.Key.ToLower() == type.ToLower());
                if (customStruct.Count() > 0)
                {
                    return customStruct.FirstOrDefault().Key;
                }
                else
                {
                    Debug.Log($"检测到未定义类型 fieldType：{type}");
                    return "string";
                }
        }
    }

    private string FormatValueForJson(string value, string fieldType, string fieldName, ExcelTableData excelTableData)
    {
        if (string.IsNullOrEmpty(value))
            return GetDefaultValue(fieldType);

        if (fieldType.StartsWith("[") && fieldType.EndsWith("]"))
        {
            return FormatArrayValue(value, fieldType, fieldName, excelTableData);
        }

        switch (fieldType.ToLower())
        {
            case "string":
            case "repeated string":
                return $"\"{value}\"";
            case "bool":
                return value.ToLower();
            case "int":
            case "long":
            case "short":
            case "byte":
            case "ubyte":
            case "ushort":
            case "uint":
            case "ulong":
                return int.TryParse(value, out _) ? value : "0";
            case "float":
            case "double":
                return float.TryParse(value, out _) ? value : "0.0";
            default:
                //TODO 处理其他数据类型或自定义类型
                string customValue = ParseCustomDataTypeToJson(value, fieldType);
                if (!string.IsNullOrEmpty(customValue))
                {
                    return customValue;
                }
                else
                {
                    Debug.Log($"检测到未定义类型 fieldType：{fieldType}，fieldName：{fieldName}，tableName：{excelTableData.tableName}_{excelTableData.workbookName} , value: {value}");
                    return $"\"{value}\"";
                }
        }
    }

    /// <summary>
    /// 解析常规类型数组，整型，浮点型，布尔型，字符串型
    /// </summary>
    /// <param name="value"></param>
    /// <param name="fieldsType"></param>
    /// <returns></returns>
    private string FormatArrayValue(string value, string fieldsType, string fieldName, ExcelTableData excelTableData)
    {
        if (string.IsNullOrEmpty(value)) return "[]";

        var sb = new StringBuilder();

        string fieldTypeName = fieldsType.Substring(1, fieldsType.Length - 2);
        List<string> items = new List<string>();
        bool isCustomStructType = IsCustomDataType(fieldTypeName);
        if (isCustomStructType)
        {
            string fieldPattern = @"\{[^}]+\}";
            var fieldMatches = Regex.Matches(value, fieldPattern);

            foreach (Match fieldMatch in fieldMatches)
            {
                items.Add(fieldMatch.Groups[0].Value);
            }
            sb.AppendLine();
            sb.Append("\t\t\t[");
        }
        else
        {
            items = value.Split(m_ArrSplits).ToList();
            sb.Append("[");
        }


        for (int i = 0; i < items.Count; i++)
        {
            string itemValue = FormatValueForJson(items[i].Trim(), fieldTypeName, fieldName, excelTableData);
            if (itemValue.StartsWith("\"") && itemValue.EndsWith("\""))
                itemValue = itemValue.Substring(1, itemValue.Length - 2);

            if (fieldTypeName == "string")
                sb.Append($"\"{itemValue}\"");
            else
                sb.Append(itemValue);

            if (i < items.Count - 1)
                sb.Append(", ");
        }

        sb.Append("]");
        return sb.ToString();
    }

    private string GetDefaultValue(string fieldType)
    {
        switch (fieldType.ToLower())
        {
            case "int":
            case "long":
            case "short":
            case "byte": return "0";
            case "float":
            case "double": return "0.0";
            case "bool": return "false";
            case "string": return "\"\"";
            default:
                if (fieldType.StartsWith("[") && fieldType.EndsWith("]"))
                    return "[]";
                return "\"\"";
        }
    }

    #region 自定义类型

    public void InitCustomDataType()
    {
        m_DicCustomDataType = new Dictionary<string, CustomDataType>();
        string customDataTypePath = _config.CustomDataTypeFilePath;
        if (File.Exists(customDataTypePath))
        {
            string content = File.ReadAllText(customDataTypePath);
            content = content.Replace("\r", "").Replace("\n", "").Replace("\t", "");
            Debug.Log("-------" + content);

            string structPattern = @"struct\s+(\w+)\s*\{([^}]*)\}";
            MatchCollection structMatches = Regex.Matches(content, structPattern, RegexOptions.Singleline);
            InitCustomDataType(structMatches, true);

            string tablePattern = @"table\s+(\w+)\s*\{([^}]*)\}";
            MatchCollection tableMatches = Regex.Matches(content, tablePattern, RegexOptions.Singleline);
            InitCustomDataType(tableMatches, false);
            Debug.Log("InitCustomDataType CustomDataTypeCount:" + m_DicCustomDataType.Count);
        }
        else
        {
            Debug.LogError($"customDataTypePath not exist , {customDataTypePath}");
        }
    }

    private void InitCustomDataType(MatchCollection matchCollection, bool isStruct)
    {
        foreach (Match structMatch in matchCollection)
        {
            string structName = structMatch.Groups[1].Value;
            string structBody = structMatch.Groups[2].Value;

            if (!m_DicCustomDataType.ContainsKey(structName))
            {
                m_DicCustomDataType.Add(structName, new CustomDataType()
                {
                    typeName = structName,
                    fields = new Dictionary<string, string>(),
                    isStruet = isStruct
                });
            }
            else
            {
                Debug.LogError($"结构体名称重复,structName:{structName},customDataTypePath:{matchCollection}");
                continue;
            }
            string fieldPattern = @"(\w+)\s*:\s*(\w+);";
            var fieldMatches = Regex.Matches(structBody, fieldPattern);

            foreach (Match fieldMatch in fieldMatches)
            {
                string fieldName = fieldMatch.Groups[1].Value;
                string fieldType = fieldMatch.Groups[2].Value;
                //Debug.Log("-------" + fieldName + "-------" + fieldType);
                m_DicCustomDataType[structName].fields.Add(fieldName, fieldType);
            }
        }
    }

    private bool IsCustomDataType(string typeName)
    {
        typeName = typeName.Replace("[", "").Replace("]", "");
        return m_DicCustomDataType.ContainsKey(typeName);
    }

    private string GetCustomDataType(params string[] customDataTypeName)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < customDataTypeName?.Length; i++)
        {
            string structName = customDataTypeName[i].Replace("[", "").Replace("]", "");
            var structData = m_DicCustomDataType[structName];
            sb.AppendLine();

            sb.AppendLine(GetCustomDataType(structData.typeName).isStruet ?
                $"struct {structData.typeName}" : $"table {structData.typeName}");
            sb.AppendLine("{");
            foreach (var field in structData.fields)
            {
                sb.AppendLine($"\t{field.Key}:{field.Value};");
            }
            sb.AppendLine("}");
        }
        return sb.ToString();
    }

    private string ParseCustomDataTypeToJson(string allFieldValue, string typeName)
    {
        StringBuilder sb = new StringBuilder();
        CustomDataType customStructType = GetCustomDataType(typeName);
        if (customStructType != null)
        {
            List<string> fieldValues = new List<string>();
            var allFieldValueSplit = allFieldValue.Split('{', '}', ',');
            foreach (var fieldValue in allFieldValueSplit)
            {
                if (!string.IsNullOrEmpty(fieldValue))
                {
                    fieldValues.Add(fieldValue);
                }
            }

            sb.Append("{");
            for (int i = 0; i < fieldValues.Count; i++)
            {
                string subFieldName = customStructType.fields.Keys.ToArray()[i];
                string subFieldType = customStructType.fields.Values.ToArray()[i];

                //string subFieldValue = FormatValueForJson(fieldValues[i], subFieldType, fieldName, excelTableData);
                string subFieldValue = fieldValues[i];
                sb.Append($"\"{subFieldName}\":{subFieldValue}");
                //if (i != fieldValues.Count - 1)
                //{
                //    sb.AppendLine(",");
                //}
                sb.Append(i != fieldValues.Count - 1? ",":"");
            }
            sb.Append("}");
        }
        return sb.ToString();
    }

    private CustomDataType GetCustomDataType(string dataTypeName)
    {
        dataTypeName = dataTypeName.Replace("[", "").Replace("]", "");
        if (m_DicCustomDataType.ContainsKey(dataTypeName))
        {
            return m_DicCustomDataType[dataTypeName];
        }
        else
        {
            throw new Exception($"CustomDataType is null ,DataTypeName:{dataTypeName}");
        }
    }
    #endregion
}