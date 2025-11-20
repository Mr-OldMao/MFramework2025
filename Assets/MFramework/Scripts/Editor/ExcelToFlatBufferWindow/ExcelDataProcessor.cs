using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ExcelDataProcessor
{
    private GenerationConfig _config;

    public ExcelDataProcessor(GenerationConfig config)
    {
        _config = config;
    }

    public List<string> GetExcelFiles()
    {
        var files = new List<string>();

        if (!Directory.Exists(_config.excelFolderPath))
        {
            Debug.LogWarning($"Excel文件夹不存在: {_config.excelFolderPath}");
            return files;
        }

        string[] allFiles = Directory.GetFiles(_config.excelFolderPath, "*.xlsx");
        foreach (string file in allFiles)
        {
            if (!file.Contains("~$"))
            {
                files.Add(file);
            }
        }

        return files;
    }

    public List<ExcelTableData> ParseExcelFile(string excelPath)
    {
        if (!File.Exists(excelPath))
            throw new System.Exception("Excel文件不存在");

        List<ExcelTableData> tableDataArr = new List<ExcelTableData>();
        FileInfo fileInfo = new FileInfo(excelPath);
        using (ExcelPackage package = new ExcelPackage(fileInfo))
        {
            if (package.Workbook.Worksheets.Count == 0)
                throw new System.Exception("Excel文件中没有工作表");

            for (int n = 1; n <= package.Workbook.Worksheets.Count; n++)
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[n];
                Debug.Log($"tableName:{Path.GetFileNameWithoutExtension(excelPath)} , sheetName:{worksheet.Name}");
                if (worksheet.Name == "#end")
                {
                    break;
                }

                if (worksheet.Dimension == null || worksheet.Dimension.Rows < _config.dataStartIndex + 1)
                    throw new System.Exception("Excel文件数据不足");

                string tableName = Path.GetFileNameWithoutExtension(excelPath);
                int columnCount = worksheet.Dimension.Columns;
                int rowCount = worksheet.Dimension.Rows;
                //校准有效列数量
                int realColumnCount = _config.dataStartIndex;
                for (int i = 1; i <= columnCount; i++)
                {
                    if (!string.IsNullOrEmpty(worksheet.Cells[_config.fieldNameIndex + 1, i].Text.Trim()))
                    {
                        realColumnCount++;
                    }
                    else
                    {
                        break;
                    }
                }
                columnCount = realColumnCount;
                //校准有效行数
                int realRowCount = _config.dataStartIndex;
                for (int i = realRowCount + 1; i <= rowCount; i++)
                {
                    if (!string.IsNullOrEmpty(worksheet.Cells[i, 1].Text.Trim()))
                    {
                        realRowCount++;
                    }
                    else
                    {
                        break;
                    }
                }
                rowCount = realRowCount;
                worksheet.DeleteColumn(columnCount + 1, worksheet.Dimension.Columns);
                worksheet.DeleteRow(rowCount + 1, worksheet.Dimension.Rows);

                Debug.Log($"table:{tableName}，列数：{columnCount}，行数：{rowCount}");

                var tableData = new ExcelTableData
                {
                    tableName = tableName,
                    workbookName = worksheet.Name,
                    fieldComments = new string[columnCount],
                    fieldNames = new string[columnCount],
                    fieldTypes = new string[columnCount],
                    dataRows = new string[rowCount - _config.dataStartIndex][]
                };
                tableDataArr.Add(tableData);

                // 读取字段注释、名称、类型
                for (int col = 1; col <= columnCount; col++)
                {
                    tableData.fieldComments[col - 1] = worksheet.Cells[_config.fieldCommentIndex + 1, col].Text?.Trim() ?? "";
                    tableData.fieldNames[col - 1] = worksheet.Cells[_config.fieldNameIndex + 1, col].Text?.Trim() ?? $"Field{col}";
                    tableData.fieldTypes[col - 1] = worksheet.Cells[_config.fieldTypeIndex + 1, col].Text?.Trim() ?? "string";
                }

                // 容错处理
                for (int i = 0; i < tableData.fieldTypes.Length; i++)
                {
                    if (string.IsNullOrEmpty(tableData.fieldTypes[i]))
                    {
                        tableData.fieldTypes[i] = "string";
                    }
                }

                // 读取数据行
                int dataRowIndex = 0;
                for (int row = _config.dataStartIndex + 1; row <= rowCount; row++)
                {
                    List<string> values = new List<string>();
                    for (int col = 1; col <= columnCount; col++)
                    {
                        var cell = worksheet.Cells[row, col];
                        string value = cell.Text?.Trim() ?? "";
                        values.Add(value);
                    }

                    if (values.Any(v => !string.IsNullOrEmpty(v)))
                    {
                        tableData.dataRows[dataRowIndex] = values.ToArray();
                        dataRowIndex++;
                    }
                }
            }

            return tableDataArr;
        }
    }

    #region 生成Excel模板文件
    public void GenerateSampleExcel()
    {
        if (!Directory.Exists(_config.excelFolderPath))
            Directory.CreateDirectory(_config.excelFolderPath);

        string excelPath = Path.Combine(_config.excelFolderPath, "TestTemplate.xlsx");

        FileInfo newFile = new FileInfo(excelPath);
        using (ExcelPackage package = new ExcelPackage(newFile))
        {
            GenerateTable(package, "Tab1");

            GenerateTable(package, "Tab2");

            ExcelWorksheet worksheet3 = package.Workbook.Worksheets.Add("#end");
            worksheet3.Cells[1, 1].Value = "#end关键字约定为结束符，此表及以后所有表会被忽略掉，不会打成其他二进制文件";
            worksheet3.Cells.AutoFitColumns();

            package.Save();
            AssetDatabase.Refresh();
            Debug.Log($"已生成Excek模板示例文件: {excelPath}");
        }
    }

    private ExcelWorksheet GenerateTable(ExcelPackage package, string name)
    {
        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(name);
        // 设置表头和数据
        int column = 1;
        worksheet.Cells[_config.fieldCommentIndex + 1, column++].Value = "id";
        worksheet.Cells[_config.fieldCommentIndex + 1, column++].Value = "攻击力";
        worksheet.Cells[_config.fieldCommentIndex + 1, column++].Value = "技能";
        worksheet.Cells[_config.fieldCommentIndex + 1, column++].Value = "技能CD";
        worksheet.Cells[_config.fieldCommentIndex + 1, column++].Value = "技能描述";
        worksheet.Cells[_config.fieldCommentIndex + 1, column++].Value = "升级所消耗的材料";
        worksheet.Cells[_config.fieldCommentIndex + 1, column++].Value = "升星所消耗的材料";

        column = 1;
        worksheet.Cells[_config.fieldNameIndex + 1, column++].Value = "id";
        worksheet.Cells[_config.fieldNameIndex + 1, column++].Value = "attack";
        worksheet.Cells[_config.fieldNameIndex + 1, column++].Value = "skills";
        worksheet.Cells[_config.fieldNameIndex + 1, column++].Value = "skillsCD";
        worksheet.Cells[_config.fieldNameIndex + 1, column++].Value = "skillsDes";
        worksheet.Cells[_config.fieldNameIndex + 1, column++].Value = "CostLevel";
        worksheet.Cells[_config.fieldNameIndex + 1, column++].Value = "CostStart";

        column = 1;
        worksheet.Cells[_config.fieldTypeIndex + 1, column++].Value = "int";
        worksheet.Cells[_config.fieldTypeIndex + 1, column++].Value = "int";
        worksheet.Cells[_config.fieldTypeIndex + 1, column++].Value = "[int]";
        worksheet.Cells[_config.fieldTypeIndex + 1, column++].Value = "[int]";
        worksheet.Cells[_config.fieldTypeIndex + 1, column++].Value = "[string]";
        worksheet.Cells[_config.fieldTypeIndex + 1, column++].Value = "Prop";
        worksheet.Cells[_config.fieldTypeIndex + 1, column++].Value = "[Prop]";

        for (int i = _config.dataStartIndex + 1; i <= _config.dataStartIndex + 10; i++)
        {
            column = 1;
            worksheet.Cells[i, column++].Value = 10001000 + i;
            worksheet.Cells[i, column++].Value = Random.Range(10, 1000);
            worksheet.Cells[i, column++].Value = GenerateRandomIntData(4, ",");
            worksheet.Cells[i, column++].Value = GenerateRandomIntData(4, "，");
            worksheet.Cells[i, column++].Value = GenerateRandomStringData(4, ":");
            worksheet.Cells[i, column++].Value = $"{{{Random.Range(10, 100)},{Random.Range(10, 100)}}}";
            worksheet.Cells[i, column++].Value = GenerateRandomPropData(Random.Range(2, 4));
        }
        worksheet.Cells.AutoFitColumns();
        return worksheet;
    }

    private string GenerateRandomPropData(int count)
    {
        string res = string.Empty;
        for (int i = 0; i < count; i++)
        {
            res += $"{{{Random.Range(10, 100)},{Random.Range(10, 100)}}}";
            if (i != count - 1)
            {
                res += ",";
            }
        }
        Debug.Log($"GenerateRandomPropData: {res}");
        return res;
    }
    private string GenerateRandomIntData(int count, string Split)
    {
        string res = string.Empty;
        for (int i = 0; i < count; i++)
        {
            res += $"{Random.Range(10, 100)}";
            if (i != count - 1)
            {
                res += Split;
            }
        }
        return res;
    }
    private string GenerateRandomStringData(int count, string Split)
    {
        string res = string.Empty;
        for (int i = 0; i < count; i++)
        {
            res += $"des{Random.Range(10, 100)}";
            if (i != count - 1)
            {
                res += Split;
            }
        }
        return res;
    }
    #endregion
}