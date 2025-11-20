using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MFramework.Editor
{
    public class ExcelCharacterExtractor : EditorWindow
    {
        // 使用EditorPrefs保存的键名
        private const string EXCEL_FOLDER_PREF_KEY = "ExcelCharacterExtractor_ExcelFolderPath";
        private const string OUTPUT_FILE_PREF_KEY = "ExcelCharacterExtractor_OutputFilePath";

        private string excelFolderPath = "Assets/ExcelFiles/";
        private string outputFilePath = "Assets/art/font/Art_CN.txt";
        private string fileFilter = "*.xlsx";
        private bool includeAllSheets = true;
        private bool removeDuplicates = true;
        private Vector2 scrollPosition;

        // 文件选择相关
        private List<ExcelFileInfo> excelFiles = new List<ExcelFileInfo>();
        private bool selectAll = true;

        // 输出文件状态
        private bool outputFileExists = false;
        private int existingCharCount = 0;

        [Serializable]
        private class ExcelFileInfo
        {
            public string path;
            public string name;
            public bool selected;
            public long fileSize;
            public DateTime lastModified;

            public ExcelFileInfo(string filePath)
            {
                path = filePath;
                name = Path.GetFileName(filePath);
                selected = true;

                FileInfo fileInfo = new FileInfo(filePath);
                fileSize = fileInfo.Length;
                lastModified = fileInfo.LastWriteTime;
            }
        }

        [MenuItem("Tools/字库工具/Excel字符提取工具")]
        public static void ShowWindow()
        {
            GetWindow<ExcelCharacterExtractor>("Excel字符提取工具");
        }

        void OnEnable()
        {
            // 从EditorPrefs加载保存的路径
            LoadSavedPaths();
            RefreshFileList();
            CheckOutputFileStatus();
        }

        void OnDisable()
        {
            // 窗口关闭时保存当前路径
            SaveCurrentPaths();
        }

        void OnDestroy()
        {
            // 窗口销毁时保存当前路径
            SaveCurrentPaths();
        }

        private void LoadSavedPaths()
        {
            // 加载Excel文件夹路径
            string savedExcelFolder = EditorPrefs.GetString(EXCEL_FOLDER_PREF_KEY, "");
            if (!string.IsNullOrEmpty(savedExcelFolder) && Directory.Exists(savedExcelFolder))
            {
                excelFolderPath = savedExcelFolder;
            }

            // 加载输出文件路径
            string savedOutputFile = EditorPrefs.GetString(OUTPUT_FILE_PREF_KEY, "");
            if (!string.IsNullOrEmpty(savedOutputFile))
            {
                outputFilePath = savedOutputFile;
            }
        }

        private void SaveCurrentPaths()
        {
            // 保存Excel文件夹路径
            if (!string.IsNullOrEmpty(excelFolderPath))
            {
                EditorPrefs.SetString(EXCEL_FOLDER_PREF_KEY, excelFolderPath);
            }

            // 保存输出文件路径
            if (!string.IsNullOrEmpty(outputFilePath))
            {
                EditorPrefs.SetString(OUTPUT_FILE_PREF_KEY, outputFilePath);
            }
        }

        private void CheckOutputFileStatus()
        {
            outputFileExists = File.Exists(outputFilePath);
            existingCharCount = 0;

            if (outputFileExists)
            {
                try
                {
                    string content = File.ReadAllText(outputFilePath, Encoding.UTF8);
                    existingCharCount = content.Length;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"无法读取输出文件: {e.Message}");
                }
            }
        }

        void OnGUI()
        {
            GUILayout.Label("Excel字符提取工具", EditorStyles.boldLabel);

            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Excel文件夹路径
            EditorGUILayout.BeginHorizontal();
            string newExcelFolderPath = EditorGUILayout.TextField("Excel文件夹:", excelFolderPath);
            if (newExcelFolderPath != excelFolderPath)
            {
                excelFolderPath = newExcelFolderPath;
                // 路径改变时自动刷新文件列表
                if (Directory.Exists(excelFolderPath))
                {
                    RefreshFileList();
                }
            }

            if (GUILayout.Button("选择", GUILayout.Width(60)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("选择Excel文件夹", excelFolderPath, "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    excelFolderPath = selectedPath;
                    RefreshFileList();
                    SaveCurrentPaths(); // 立即保存新路径
                }
            }
            EditorGUILayout.EndHorizontal();

            // 显示路径状态
            if (!Directory.Exists(excelFolderPath))
            {
                EditorGUILayout.HelpBox("文件夹不存在", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox($"找到 {excelFiles.Count} 个Excel文件", MessageType.Info);
            }

            // 输出文件路径
            EditorGUILayout.BeginHorizontal();
            string newOutputFilePath = EditorGUILayout.TextField("输出文件:", outputFilePath);
            if (newOutputFilePath != outputFilePath)
            {
                outputFilePath = newOutputFilePath;
                SaveCurrentPaths(); // 立即保存新路径
                CheckOutputFileStatus(); // 检查输出文件状态
            }

            if (GUILayout.Button("选择", GUILayout.Width(60)))
            {
                string defaultName = "Art_CN.txt";
                if (!string.IsNullOrEmpty(outputFilePath))
                {
                    defaultName = Path.GetFileName(outputFilePath);
                }

                string selectedPath = EditorUtility.SaveFilePanel("保存字库文件",
                    Path.GetDirectoryName(outputFilePath), defaultName, "txt");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    outputFilePath = selectedPath;
                    SaveCurrentPaths(); // 立即保存新路径
                    CheckOutputFileStatus(); // 检查输出文件状态
                }
            }
            EditorGUILayout.EndHorizontal();

            // 显示输出文件状态
            if (outputFileExists)
            {
                EditorGUILayout.HelpBox($"输出文件已存在，包含 {existingCharCount} 个字符", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox("输出文件不存在，将创建新文件", MessageType.Info);
            }

            // 文件过滤
            EditorGUILayout.BeginHorizontal();
            fileFilter = EditorGUILayout.TextField("文件过滤:", fileFilter);
            if (GUILayout.Button("刷新", GUILayout.Width(60)))
            {
                RefreshFileList();
            }
            EditorGUILayout.EndHorizontal();

            includeAllSheets = EditorGUILayout.Toggle("读取所有工作表", includeAllSheets);
            removeDuplicates = EditorGUILayout.Toggle("自动去重", removeDuplicates);

            EditorGUILayout.Space();

            // 文件选择区域
            GUILayout.Label("选择要提取的Excel文件:", EditorStyles.boldLabel);

            // 全选/取消全选
            EditorGUILayout.BeginHorizontal();
            bool newSelectAll = EditorGUILayout.Toggle("全选", selectAll);
            if (newSelectAll != selectAll)
            {
                selectAll = newSelectAll;
                foreach (var file in excelFiles)
                {
                    file.selected = selectAll;
                }
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("反选", GUILayout.Width(60)))
            {
                foreach (var file in excelFiles)
                {
                    file.selected = !file.selected;
                }
                selectAll = excelFiles.All(f => f.selected);
            }

            if (GUILayout.Button("刷新列表", GUILayout.Width(80)))
            {
                RefreshFileList();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // 显示Excel文件列表（带勾选框）
            if (excelFiles.Count > 0)
            {
                GUILayout.Label($"找到 {excelFiles.Count} 个Excel文件:");

                foreach (var fileInfo in excelFiles)
                {
                    EditorGUILayout.BeginHorizontal();

                    fileInfo.selected = EditorGUILayout.Toggle(fileInfo.selected, GUILayout.Width(20));

                    EditorGUILayout.LabelField(fileInfo.name, GUILayout.MinWidth(200));

                    // 显示文件信息
                    string sizeText = FormatFileSize(fileInfo.fileSize);
                    EditorGUILayout.LabelField(sizeText, GUILayout.Width(80));

                    EditorGUILayout.LabelField(fileInfo.lastModified.ToString("MM/dd HH:mm"), GUILayout.Width(100));

                    // 预览按钮
                    if (GUILayout.Button("预览", GUILayout.Width(50)))
                    {
                        PreviewExcelFile(fileInfo.path);
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }
            else if (Directory.Exists(excelFolderPath))
            {
                EditorGUILayout.HelpBox("未找到匹配的Excel文件", MessageType.Warning);
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            // 操作按钮 - 根据输出文件是否存在显示不同的按钮
            if (outputFileExists)
            {
                GUILayout.Label("输出文件已存在，请选择操作方式:", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("叠加提取", GUILayout.Height(40)))
                {
                    ExtractSelectedCharacters(ExtractMode.Append);
                }

                if (GUILayout.Button("覆盖提取", GUILayout.Height(40)))
                {
                    if (EditorUtility.DisplayDialog("确认覆盖",
                        $"将覆盖现有文件的所有内容\n原文件有 {existingCharCount} 个字符\n确定要覆盖吗？", "是", "否"))
                    {
                        ExtractSelectedCharacters(ExtractMode.Overwrite);
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.HelpBox("覆盖: 完全替换文件内容\n叠加: 在现有内容后添加新字符并去重", MessageType.Info);
            }
            else
            {
                if (GUILayout.Button("提取选中字符", GUILayout.Height(40)))
                {
                    ExtractSelectedCharacters(ExtractMode.Overwrite);
                }
            }

            EditorGUILayout.Space();

            // 其他功能按钮
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("测试选中文件", GUILayout.Height(25)))
            {
                TestSelectedFiles();
            }

            if (GUILayout.Button("清空字库文件", GUILayout.Height(25)))
            {
                ClearOutputFile();
            }

            if (GUILayout.Button("重置路径", GUILayout.Height(25)))
            {
                ResetPaths();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("功能说明:\n1. 勾选需要提取的Excel文件\n2. 提取选中文件的文本内容\n3. 自动去重并保持字符顺序\n4. 生成统一的字库文件", MessageType.Info);
        }

        private enum ExtractMode
        {
            Overwrite,
            Append
        }

        private void RefreshFileList()
        {
            excelFiles.Clear();

            if (!Directory.Exists(excelFolderPath))
                return;

            string[] files = Directory.GetFiles(excelFolderPath, fileFilter);
            foreach (string file in files)
            {
                // 跳过临时Excel文件（以~$开头的文件）
                if (Path.GetFileName(file).StartsWith("~$"))
                    continue;

                excelFiles.Add(new ExcelFileInfo(file));
            }

            // 按文件名排序
            excelFiles = excelFiles.OrderBy(f => f.name).ToList();

            // 更新全选状态
            selectAll = excelFiles.Count > 0 && excelFiles.All(f => f.selected);
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double len = bytes;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return string.Format("{0:0.##} {1}", len, sizes[order]);
        }

        private void PreviewExcelFile(string filePath)
        {
            try
            {
                //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var workbook = package.Workbook;
                    string previewInfo = $"文件: {Path.GetFileName(filePath)}\n";
                    previewInfo += $"工作表数量: {workbook.Worksheets.Count}\n\n";

                    foreach (var worksheet in workbook.Worksheets)
                    {
                        int rowCount = worksheet.Dimension?.Rows ?? 0;
                        int colCount = worksheet.Dimension?.Columns ?? 0;
                        previewInfo += $"{worksheet.Name}: {rowCount}行 × {colCount}列\n";
                    }

                    EditorUtility.DisplayDialog("文件预览", previewInfo, "确定");
                }
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("预览错误", $"无法读取文件: {e.Message}", "确定");
            }
        }

        private void ExtractSelectedCharacters(ExtractMode mode)
        {
            var selectedFiles = excelFiles.Where(f => f.selected).ToList();
            if (selectedFiles.Count == 0)
            {
                EditorUtility.DisplayDialog("错误", "请至少选择一个Excel文件", "确定");
                return;
            }

            try
            {
                //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                HashSet<char> allCharacters = new HashSet<char>();
                int totalSheets = 0;
                int totalCells = 0;
                int processedFiles = 0;

                // 如果是叠加模式，先读取现有文件中的字符
                if (mode == ExtractMode.Append && outputFileExists)
                {
                    try
                    {
                        string existingContent = File.ReadAllText(outputFilePath, Encoding.UTF8);
                        foreach (char c in existingContent)
                        {
                            allCharacters.Add(c);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"无法读取现有文件内容: {e.Message}");
                    }
                }

                foreach (var fileInfo in selectedFiles)
                {
                    processedFiles++;
                    EditorUtility.DisplayProgressBar("提取字符",
                        $"处理文件: {Path.GetFileName(fileInfo.path)} ({processedFiles}/{selectedFiles.Count})",
                        (float)processedFiles / selectedFiles.Count);

                    using (var package = new ExcelPackage(new FileInfo(fileInfo.path)))
                    {
                        var workbook = package.Workbook;

                        foreach (var worksheet in workbook.Worksheets)
                        {
                            if (!includeAllSheets && worksheet.Hidden != eWorkSheetHidden.Visible)
                                continue;

                            totalSheets++;
                            int rowCount = worksheet.Dimension?.Rows ?? 0;
                            int colCount = worksheet.Dimension?.Columns ?? 0;

                            for (int row = 1; row <= rowCount; row++)
                            {
                                for (int col = 1; col <= colCount; col++)
                                {
                                    var cell = worksheet.Cells[row, col];
                                    if (cell.Value != null)
                                    {
                                        string cellText = cell.Value.ToString();
                                        ExtractCharactersFromText(cellText, allCharacters);
                                        totalCells++;
                                    }
                                }
                            }
                        }
                    }
                }

                EditorUtility.ClearProgressBar();

                // 转换为列表并排序（可选）
                List<char> characterList = allCharacters.ToList();
                characterList.Sort();

                // 确保输出目录存在
                string outputDir = Path.GetDirectoryName(outputFilePath);
                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                // 写入文件
                File.WriteAllText(outputFilePath, new string(characterList.ToArray()), Encoding.UTF8);

                // 刷新AssetDatabase
                AssetDatabase.Refresh();

                // 更新文件状态
                CheckOutputFileStatus();

                string modeText = mode == ExtractMode.Overwrite ? "覆盖" : "叠加";
                EditorUtility.DisplayDialog("提取完成",
                    $"{modeText}模式处理完成\n" +
                    $"处理了 {selectedFiles.Count} 个Excel文件\n" +
                    $"读取了 {totalSheets} 个工作表\n" +
                    $"扫描了 {totalCells} 个单元格\n" +
                    $"提取到 {characterList.Count} 个唯一字符\n" +
                    $"已保存到: {outputFilePath}", "确定");

                Debug.Log($"Excel字符提取完成({modeText}): {characterList.Count}个字符已保存到 {outputFilePath}");

                // 重新绘制UI以更新状态
                Repaint();
            }
            catch (System.Exception e)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("错误", $"提取字符时发生错误: {e.Message}", "确定");
                Debug.LogError($"Excel字符提取错误: {e}");
            }
        }

        private void TestSelectedFiles()
        {
            var selectedFiles = excelFiles.Where(f => f.selected).ToList();
            if (selectedFiles.Count == 0)
            {
                EditorUtility.DisplayDialog("错误", "请至少选择一个Excel文件", "确定");
                return;
            }

            StringBuilder testResult = new StringBuilder();
            testResult.AppendLine("文件测试结果:");
            testResult.AppendLine();

            int successCount = 0;
            int failCount = 0;

            foreach (var fileInfo in selectedFiles)
            {
                try
                {
                    //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                    using (var package = new ExcelPackage(new FileInfo(fileInfo.path)))
                    {
                        var workbook = package.Workbook;
                        testResult.AppendLine($"✓ {fileInfo.name}");
                        testResult.AppendLine($"  工作表: {workbook.Worksheets.Count}个");

                        foreach (var worksheet in workbook.Worksheets)
                        {
                            int rowCount = worksheet.Dimension?.Rows ?? 0;
                            int colCount = worksheet.Dimension?.Columns ?? 0;
                            testResult.AppendLine($"  - {worksheet.Name}: {rowCount}行 {colCount}列");
                        }

                        successCount++;
                    }
                }
                catch (Exception e)
                {
                    testResult.AppendLine($"✗ {fileInfo.name}");
                    testResult.AppendLine($"  错误: {e.Message}");
                    failCount++;
                }

                testResult.AppendLine();
            }

            testResult.AppendLine($"总计: {successCount}个成功, {failCount}个失败");

            EditorUtility.DisplayDialog("文件测试", testResult.ToString(), "确定");
        }

        private void ExtractCharactersFromText(string text, HashSet<char> characterSet)
        {
            if (string.IsNullOrEmpty(text))
                return;

            foreach (char c in text)
            {
                // 过滤控制字符和空白字符（根据需要调整）
                if (!char.IsControl(c) && c != '\r' && c != '\n' && c != '\t')
                {
                    characterSet.Add(c);
                }
            }
        }

        private void ClearOutputFile()
        {
            if (File.Exists(outputFilePath))
            {
                if (EditorUtility.DisplayDialog("确认", "确定要清空字库文件吗？", "是", "否"))
                {
                    File.WriteAllText(outputFilePath, "", Encoding.UTF8);
                    AssetDatabase.Refresh();
                    CheckOutputFileStatus(); // 更新文件状态
                    EditorUtility.DisplayDialog("完成", "字库文件已清空", "确定");
                    Repaint();
                }
            }
            else
            {
                EditorUtility.DisplayDialog("提示", "字库文件不存在", "确定");
            }
        }

        private void ResetPaths()
        {
            if (EditorUtility.DisplayDialog("确认", "确定要重置所有路径为默认值吗？", "是", "否"))
            {
                // 清除保存的路径
                EditorPrefs.DeleteKey(EXCEL_FOLDER_PREF_KEY);
                EditorPrefs.DeleteKey(OUTPUT_FILE_PREF_KEY);

                // 重置为默认值
                excelFolderPath = "Assets/ExcelFiles/";
                outputFilePath = "Assets/art/font/Art_CN.txt";

                RefreshFileList();
                CheckOutputFileStatus(); // 更新文件状态
                Repaint();
            }
        }

        // 右键菜单：快速处理选中的Excel文件
        [MenuItem("Assets/字库工具/提取Excel字符到字库", false, 31)]
        static void ExtractFromSelectedExcel()
        {
            Object[] selectedObjects = Selection.objects;
            if (selectedObjects != null && selectedObjects.Length > 0)
            {
                List<string> excelPaths = new List<string>();

                foreach (Object obj in selectedObjects)
                {
                    string path = AssetDatabase.GetAssetPath(obj);
                    if (path.EndsWith(".xlsx") || path.EndsWith(".xls"))
                    {
                        excelPaths.Add(path);
                    }
                }

                if (excelPaths.Count > 0)
                {
                    ExcelCharacterExtractor window = GetWindow<ExcelCharacterExtractor>();

                    // 如果当前窗口的文件夹不是选中文件所在的文件夹，则更新路径
                    string firstFileDir = Path.GetDirectoryName(excelPaths[0]);
                    if (window.excelFolderPath != firstFileDir)
                    {
                        window.excelFolderPath = firstFileDir;
                        window.RefreshFileList();
                        window.SaveCurrentPaths(); // 保存新路径
                    }

                    // 只选择当前选中的文件
                    foreach (var fileInfo in window.excelFiles)
                    {
                        fileInfo.selected = excelPaths.Contains(fileInfo.path);
                    }

                    // 检查输出文件状态并决定使用哪种模式
                    window.CheckOutputFileStatus();
                    if (window.outputFileExists)
                    {
                        // 如果输出文件存在，询问用户使用哪种模式
                        if (EditorUtility.DisplayDialog("选择提取模式",
                            "输出文件已存在，请选择提取模式:", "覆盖", "叠加"))
                        {
                            window.ExtractSelectedCharacters(ExtractMode.Overwrite);
                        }
                        else
                        {
                            window.ExtractSelectedCharacters(ExtractMode.Append);
                        }
                    }
                    else
                    {
                        window.ExtractSelectedCharacters(ExtractMode.Overwrite);
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("错误", "请选择Excel文件", "确定");
                }
            }
        }

        [MenuItem("Assets/字库工具/提取Excel字符到字库", true)]
        static bool ValidateExtractFromSelectedExcel()
        {
            Object[] selectedObjects = Selection.objects;
            if (selectedObjects == null) return false;

            foreach (Object obj in selectedObjects)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (path.EndsWith(".xlsx") || path.EndsWith(".xls"))
                {
                    return true;
                }
            }
            return false;
        }
    }
}