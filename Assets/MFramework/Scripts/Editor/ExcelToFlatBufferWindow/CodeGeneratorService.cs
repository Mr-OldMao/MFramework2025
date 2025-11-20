using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class CodeGeneratorService
{
    /// <summary>
    /// 数组分割符
    /// </summary>
    private char[] m_ArrSplits = { ',', '，', ':' };

    public string GenerateFbsContent(ExcelTableData tableData)
    {
        var sb = new StringBuilder();

        string tableName = $"FB_{tableData.tableName}_{tableData.workbookName}";
        bool isExistType_Prop = false;

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
                if (fieldType == "Prop" || fieldType == "[Prop]")
                {
                    isExistType_Prop = true;
                }
            }
        }
        sb.AppendLine("}");

        if (isExistType_Prop)
        {
            sb.AppendLine();
            sb.AppendLine($"struct Prop");
            sb.AppendLine("{");
            sb.AppendLine($"\tid:int;");
            sb.AppendLine($"\tvalue:int;");
            sb.AppendLine("}");
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

            sb.Append("\t\t{");
            bool firstField = true;

            for (int j = 0; j < tableData.fieldNames.Length && j < tableData.dataRows[i].Length; j++)
            {
                if (string.IsNullOrEmpty(tableData.fieldNames[j])) continue;

                if (!firstField) sb.Append(", ");
                firstField = false;

                string fieldName = tableData.fieldNames[j].Trim();
                string fieldType = j < tableData.fieldTypes.Length ? tableData.fieldTypes[j].Trim() : "string";
                string value = tableData.dataRows[i][j].Trim();

                sb.Append($"\"{fieldName}\": {FormatValueForJson(value, fieldType, fieldName, tableData)}");
            }

            sb.Append(i < tableData.dataRows.Length - 1 ? "}," : "}");
            sb.AppendLine();
        }

        sb.AppendLine("\t]");
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
            case "prop":
                return "Prop";
            default:
                Debug.Log($"检测到未定义类型 fieldType：{type}");
                return "string";
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
            case "prop"://自定义数据类型
                return ParseCustomProp(value);
            default:
                //TODO 处理其他数据类型或自定义类型
                Debug.Log($"检测到未定义类型 fieldType：{fieldType}，fieldName：{fieldName}，tableName：{excelTableData.tableName}_{excelTableData.workbookName} , value: {value}");
                return $"\"{value}\"";
        }
    }

    private string ParseCustomProp(string prop)
    {
        StringBuilder res = new StringBuilder();
        var propStrArr = prop.Split(',');
        for (int i = 0; i < propStrArr.Length; i++)
        {
            string propStr = propStrArr[i].Trim();
            if (propStr.StartsWith("{"))
            {
                string[] propArr = propStr.Split("{");
                res.Append("{");
                res.Append($"\"id\":{propArr[1]}");
                if (propStrArr.Length > 1)
                {
                    res.Append(",");
                }
            }
            else if (propStr.EndsWith("}"))
            {
                string[] propArr = propStr.Split("}");

                res.Append($"\"value\":{propArr[0]}");
                res.Append("}");
            }
        }
        return res.ToString();
    }

    /// <summary>
    /// 解析常规类型数组，整型，浮点型，布尔型，字符串型
    /// </summary>
    /// <param name="value"></param>
    /// <param name="fieldType"></param>
    /// <returns></returns>
    private string FormatArrayValue(string value, string fieldType, string fieldName, ExcelTableData excelTableData)
    {
        if (string.IsNullOrEmpty(value)) return "[]";

        string innerType = fieldType.Substring(1, fieldType.Length - 2);
        string[] items = items = value.Split(m_ArrSplits);
        var sb = new StringBuilder();
        sb.Append("[");

        for (int i = 0; i < items.Length; i++)
        {
            string itemValue = FormatValueForJson(items[i].Trim(), innerType, fieldName, excelTableData);
            if (itemValue.StartsWith("\"") && itemValue.EndsWith("\""))
                itemValue = itemValue.Substring(1, itemValue.Length - 2);

            if (innerType == "string")
                sb.Append($"\"{itemValue}\"");
            else
                sb.Append(itemValue);

            if (i < items.Length - 1)
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

}