using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Game.Tool
{
    public class ExcelToFlatBufferWindow : EditorWindow
    {
        private GenerationConfig _config = new GenerationConfig();
        private ExcelDataProcessor _dataProcessor;
        private CodeGeneratorService _codeGenerator;
        private FlatcCompilerService _compilerService;

        private Vector2 _scrollPosition;
        private List<string> _excelFiles = new List<string>();
        private string _selectedExcel = "";
        private StringBuilder _consoleOutput = new StringBuilder();

        /// <summary>
        /// 打表工具(依赖于C#FlatBuffers文件库、flatc.exe编译器、EPPlus.dll)
        /// </summary>
        [MenuItem("Tools/打表工具")]
        public static void ShowWindow()
        {
            GetWindow<ExcelToFlatBufferWindow>("Excel转FlatBuffer");
        }

        private void OnEnable()
        {
            _dataProcessor = new ExcelDataProcessor(_config);
            _codeGenerator = new CodeGeneratorService(_config);
            _compilerService = new FlatcCompilerService();

            _config.flatcAvailable = _compilerService.IsCompilerAvailable(_config.FlatcPath);

            RefreshExcelFiles();
        }

        private void OnGUI()
        {
            GUILayout.Label("Excel转FlatBuffer工具", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawPathSettings();
            DrawFileSelection();
            DrawActionButtons();
            DrawConsoleOutput();

            if (!_config.flatcAvailable)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(
                    "需要安装flatc编译器。\n" +
                    "下载地址: https://github.com/google/flatbuffers/releases\n" +
                    "下载后请选择flatc可执行文件路径",
                    MessageType.Info);
            }
        }


        private void DrawPathSettings()
        {
            // Excel文件夹路径
            EditorGUILayout.BeginHorizontal();
            string newExcelFolderPath = EditorGUILayout.TextField("Excel文件夹:", _config.ExcelFolderPath);
            if (newExcelFolderPath != _config.ExcelFolderPath)
            {
                _config.ExcelFolderPath = newExcelFolderPath;
                RefreshExcelFiles();
            }
            if (GUILayout.Button("浏览", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFolderPanel("选择Excel文件夹", Application.dataPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    _config.ExcelFolderPath = path;
                    RefreshExcelFiles();
                }
            }
            EditorGUILayout.EndHorizontal();

            // Excel自定义数据类型路径
            EditorGUILayout.BeginHorizontal();
            _config.CustomDataTypeFilePath  = EditorGUILayout.TextField("Excel自定义数据类型路径:", _config.CustomDataTypeFilePath);
            if (GUILayout.Button("浏览", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFilePanel("选择Excel自定义数据类型路径", Application.dataPath, "fbs");
                if (!string.IsNullOrEmpty(path)) 
                    _config.CustomDataTypeFilePath = path;
            }
            EditorGUILayout.EndHorizontal();

            // FlatBuffer输出路径
            EditorGUILayout.BeginHorizontal();
            _config.OutputPath = EditorGUILayout.TextField("FlatBuffer输出路径:", _config.OutputPath);
            if (GUILayout.Button("浏览", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFolderPanel("选择输出文件夹", Application.dataPath, "");
                if (!string.IsNullOrEmpty(path)) _config.OutputPath = path;
            }
            EditorGUILayout.EndHorizontal();

            // 数据文件输出路径
            EditorGUILayout.BeginHorizontal();
            _config.DataPathOutputPath = EditorGUILayout.TextField("数据文件输出路径:", _config.DataPathOutputPath);
            if (GUILayout.Button("浏览", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFolderPanel("选择输出文件夹", Application.dataPath, "");
                if (!string.IsNullOrEmpty(path)) _config.DataPathOutputPath = path;
            }
            EditorGUILayout.EndHorizontal();

            // C#代码输出路径
            EditorGUILayout.BeginHorizontal();
            _config.CsOutputPath = EditorGUILayout.TextField("C#代码输出路径:", _config.CsOutputPath);
            if (GUILayout.Button("浏览", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFolderPanel("选择C#代码输出文件夹", Application.dataPath, "");
                if (!string.IsNullOrEmpty(path)) _config.CsOutputPath = path;
            }
            EditorGUILayout.EndHorizontal();

            // .Bytes文件拷贝路径
            EditorGUILayout.BeginHorizontal();
            _config.CopyBytesPath = EditorGUILayout.TextField(".Bytes文件拷贝路径:", _config.CopyBytesPath);
            if (GUILayout.Button("浏览", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFolderPanel("选择输出文件夹", Application.dataPath, "");
                if (!string.IsNullOrEmpty(path)) _config.CopyBytesPath = path;
            }
            EditorGUILayout.EndHorizontal();

            // flatc路径
            EditorGUILayout.BeginHorizontal();
            string newFlatcPath = EditorGUILayout.TextField("flatc路径:", _config.FlatcPath);
            if (newFlatcPath != _config.FlatcPath)
            {
                _config.FlatcPath = newFlatcPath;
                _config.flatcAvailable = _compilerService.IsCompilerAvailable(_config.FlatcPath);
            }

            if (GUILayout.Button("浏览", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFilePanel("选择flatc编译器", "", "exe");
                if (!string.IsNullOrEmpty(path))
                {
                    _config.FlatcPath = path;
                    _config.flatcAvailable = _compilerService.IsCompilerAvailable(_config.FlatcPath);
                }
            }
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();

            // 自动查找flatc
            if (GUILayout.Button("自动查找flatc", GUILayout.Width(150)))
            {
                string foundPath = _compilerService.FindFlatcCompiler(_config.FlatcPath);
                if (!string.IsNullOrEmpty(foundPath))
                {
                    _config.FlatcPath = foundPath;
                    _config.flatcAvailable = true;
                    AddConsoleOutput($"已找到flatc: {foundPath}");
                }
                else
                {
                    EditorUtility.DisplayDialog("提示", "未找到flatc编译器，请手动选择或下载安装", "确定");
                }
            }

            // 验证flatc状态
            GUILayout.FlexibleSpace();
            GUILayout.Label("状态:", GUILayout.Width(50));
            _config.flatcAvailable = _compilerService.IsCompilerAvailable(_config.FlatcPath);
            GUILayout.Label(_config.flatcAvailable ? "✓ 可用" : "✗ 不可用",
                           _config.flatcAvailable ? EditorStyles.boldLabel : EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();

            // 索引配置
            EditorGUILayout.Space(10);
            GUILayout.Label("Excel解析配置", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            _config.FieldCommentIndex = EditorGUILayout.IntField("注释行索引:", _config.FieldCommentIndex);
            _config.FieldNameIndex = EditorGUILayout.IntField("字段名索引:", _config.FieldNameIndex);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            _config.FieldTypeIndex = EditorGUILayout.IntField("字段类型索引:", _config.FieldTypeIndex);
            _config.FieldTagIndex = EditorGUILayout.IntField("字段标识索引:", _config.FieldTagIndex);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _config.DataStartIndex = EditorGUILayout.IntField("数据起始行:", _config.DataStartIndex);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            // 重置按钮
            if (GUILayout.Button("重置所有设置", GUILayout.Height(30), GUILayout.Width(150)))
            {
                if (EditorUtility.DisplayDialog("确认重置", "确定要重置所有设置为默认值吗？", "确定", "取消"))
                {
                    _config.ResetToDefaults();
                    RefreshExcelFiles();
                    AddConsoleOutput("设置已重置为默认值");
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(30);
            if (GUILayout.Button("刷新Excel文件列表")) RefreshExcelFiles();
            EditorGUILayout.Space();
        }

        private void DrawFileSelection()
        {
            if (_excelFiles.Count > 0)
            {
                GUILayout.Label($"找到 {_excelFiles.Count} 个Excel文件", EditorStyles.boldLabel);

                string[] excelNames = _excelFiles.ConvertAll(f => Path.GetFileName(f)).ToArray();
                int selectedIndex = Mathf.Max(0, _excelFiles.IndexOf(_selectedExcel));
                selectedIndex = EditorGUILayout.Popup("选择Excel文件:", selectedIndex, excelNames);

                if (selectedIndex >= 0 && selectedIndex < _excelFiles.Count)
                {
                    _selectedExcel = _excelFiles[selectedIndex];
                }

                EditorGUILayout.Space();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox("未找到Excel文件", MessageType.Warning);
                if (GUILayout.Button("生成Excel模板文件", GUILayout.Height(38), GUILayout.Width(300)))
                {
                    _dataProcessor.GenerateSampleExcel();
                    RefreshExcelFiles();
                }
                EditorGUILayout.EndHorizontal();
            }


            if (!File.Exists(_config.CustomDataTypeFilePath))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox("未找到CustomDataType.fbs文件", MessageType.Warning);
                if (GUILayout.Button("生成CustomDataType.fbs模板文件", GUILayout.Height(38),GUILayout.Width(300)))
                {
                    _dataProcessor.GenerateCustomDataTypeFile();
                }
                EditorGUILayout.EndHorizontal();
            }

        }

        private void DrawActionButtons()
        {
            if (_excelFiles.Count > 0)
            {
                EditorGUI.BeginDisabledGroup(!_config.flatcAvailable);

                EditorGUILayout.BeginHorizontal();
                string tableName = Path.GetFileNameWithoutExtension(_selectedExcel);
                if (GUILayout.Button($"打当前选中的Excel表({tableName}.xlsx)", GUILayout.Height(30)))
                {
                    GenerateExcelFiles();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("一键打所有Excel表", GUILayout.Height(30)))
                {
                    if (EditorUtility.DisplayDialog("一键打所有Excel表", "确定要一键打所有Excel表？", "确定", "取消"))
                    {
                        GenerateAllExcelFiles();
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("拷贝所有.bytes文件到Resources", GUILayout.Height(30)))
                {
                    CopyBytesToResources();
                }

                EditorGUILayout.EndHorizontal();

                EditorGUI.EndDisabledGroup();
            }
        }


        private void DrawConsoleOutput()
        {
            if (_consoleOutput.Length > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label("生成日志:", EditorStyles.boldLabel);
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(500));
                EditorGUILayout.TextArea(_consoleOutput.ToString(), GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
            }
        }

        private void RefreshExcelFiles()
        {
            _excelFiles = _dataProcessor.GetExcelFiles();
        }

        private void GenerateAllExcelFiles()
        {
            if (_excelFiles.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "没有找到Excel文件", "确定");
                return;
            }

            _consoleOutput.Clear();
            AddConsoleOutput("开始批量生成所有Excel文件数据...");

            int successCount = 0;
            int failCount = 0;

            try
            {
                foreach (string excelFile in _excelFiles)
                {
                    try
                    {
                        AddConsoleOutput($"处理文件: {Path.GetFileName(excelFile)}");
                        GenerateSingleExcelFile(excelFile);
                        successCount++;
                        AddConsoleOutput($"✓ {Path.GetFileName(excelFile)} 生成成功");
                    }
                    catch (System.Exception e)
                    {
                        failCount++;
                        AddConsoleOutput($"✗ {Path.GetFileName(excelFile)} 生成失败: {e.Message}");
                        Debug.LogError($"生成 {excelFile} 失败: {e}");
                    }
                }

                AssetDatabase.Refresh();

                AddConsoleOutput($"批量生成完成! 成功: {successCount}, 失败: {failCount}");
                EditorUtility.DisplayDialog("完成",
                    $"批量生成完成!\n成功: {successCount} 个文件\n失败: {failCount} 个文件", "确定");
            }
            catch (System.Exception e)
            {
                AddConsoleOutput($"批量生成过程出错: {e.Message}");
                EditorUtility.DisplayDialog("错误", $"批量生成失败: {e.Message}", "确定");
            }
        }

        private void GenerateExcelFiles()
        {
            if (_excelFiles.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "没有找到Excel文件", "确定");
                return;
            }

            if (!_excelFiles.Contains(_selectedExcel))
            {
                EditorUtility.DisplayDialog("提示", $"没有找到Excel文件,{_selectedExcel}", "确定");
                return;
            }


            _consoleOutput.Clear();
            AddConsoleOutput("开始生成目标Excel文件数据...");

            try
            {
                AddConsoleOutput($"处理文件: {Path.GetFileName(_selectedExcel)}");
                GenerateSingleExcelFile(_selectedExcel);
                AddConsoleOutput($"✓ {Path.GetFileName(_selectedExcel)} 生成成功");
            }
            catch (System.Exception e)
            {
                AddConsoleOutput($"✗ {Path.GetFileName(_selectedExcel)} 生成失败: {e.Message}");
                Debug.LogError($"生成 {_selectedExcel} 失败: {e}");
            }

            AssetDatabase.Refresh();

            AddConsoleOutput($"生成完成! tableName : {_selectedExcel}");
            EditorUtility.DisplayDialog("完成",
                $"生成完成! \ntableName : {_selectedExcel}", "确定");
        }


        private void GenerateSingleExcelFile(string excelPath)
        {
            List<ExcelTableData> tableDataArr = _dataProcessor.ParseExcelFile(excelPath);
            _codeGenerator.InitCustomDataType();
            for (int i = 0; i < tableDataArr.Count; i++)
            {
                ExcelTableData tableData = tableDataArr[i];
                string fbsContent = _codeGenerator.GenerateFbsContent(tableData);
                string jsonContent = _codeGenerator.GenerateJsonContent(tableData);

                // 确保输出目录存在
                if (!Directory.Exists(_config.OutputPath))
                    Directory.CreateDirectory(_config.OutputPath);
                if (!Directory.Exists(_config.DataPathOutputPath))
                    Directory.CreateDirectory(_config.DataPathOutputPath);

                string baseName = $"{tableData.tableName}_{tableData.workbookName}";
                string fbsPath = Path.Combine(_config.DataPathOutputPath, $"{baseName}.fbs");
                string jsonPath = Path.Combine(_config.DataPathOutputPath, $"{baseName}.json");
                string binPath = Path.Combine(_config.DataPathOutputPath, $"{baseName}.bin");
                string bytesPath = Path.Combine(_config.DataPathOutputPath, $"{baseName}.bytes");

                File.WriteAllText(fbsPath, fbsContent);
                AddConsoleOutput($"生成文件: {fbsPath}");


                File.WriteAllText(jsonPath, jsonContent);
                AddConsoleOutput($"生成文件: {jsonPath}");


                // 编译
                bool success = _compilerService.Compile(_config.FlatcPath, fbsPath, jsonPath, _config.OutputPath, _config.DataPathOutputPath, AddConsoleOutput);

                if (success)
                {
                    // 检查生成的文件并根据选择处理
                    if (File.Exists(binPath))
                    {
                        AddConsoleOutput($"✓ 生成二进制文件: {binPath}");
                        File.Copy(binPath, bytesPath, true);
                        AddConsoleOutput($"✓ 生成.bytes文件: {bytesPath}");
                    }
                    else
                    {
                        AddConsoleOutput($"✗ 未找到二进制文件: {binPath}");
                    }
                } 
            }
        }

        private void CopyBytesToResources()
        {
            if (!Directory.Exists(_config.DataPathOutputPath))
            {
                EditorUtility.DisplayDialog("错误", "输出路径不存在，请先生成.bytes文件", "确定");
                return;
            }

            _consoleOutput.Clear();
            AddConsoleOutput("开始拷贝.bytes文件到Resources...");

            try
            {
                string targetFolder = _config.CopyBytesPath;

                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                    AddConsoleOutput($"创建目标目录: {targetFolder}");
                }

                // 获取所有.bytes文件
                string[] bytesFiles = Directory.GetFiles(_config.DataPathOutputPath, "*.bytes");

                if (bytesFiles.Length == 0)
                {
                    AddConsoleOutput("未找到.bytes文件，请先生成.bytes文件");
                    EditorUtility.DisplayDialog("提示", "未找到.bytes文件，请先生成.bytes文件", "确定");
                    return;
                }

                int copiedCount = 0;
                int failedCount = 0;

                foreach (string sourceFile in bytesFiles)
                {
                    try
                    {
                        string fileName = Path.GetFileName(sourceFile);
                        string targetFile = Path.Combine(targetFolder, fileName);

                        // 拷贝文件，覆盖已存在的文件
                        File.Copy(sourceFile, targetFile, true);
                        AddConsoleOutput($"✓ 拷贝文件: {fileName}");
                        copiedCount++;
                    }
                    catch (System.Exception e)
                    {
                        AddConsoleOutput($"✗ 拷贝失败: {Path.GetFileName(sourceFile)} - {e.Message}");
                        failedCount++;
                    }
                }

                AssetDatabase.Refresh();

                AddConsoleOutput($"拷贝完成! 成功: {copiedCount}, 失败: {failedCount}");
                EditorUtility.DisplayDialog("完成",
                    $"拷贝完成!\n成功: {copiedCount} 个文件\n失败: {failedCount} 个文件", "确定");
            }
            catch (System.Exception e)
            {
                AddConsoleOutput($"拷贝过程出错: {e.Message}");
                EditorUtility.DisplayDialog("错误", $"拷贝失败: {e.Message}", "确定");
            }
        }

        private void AddConsoleOutput(string message)
        {
            _consoleOutput.AppendLine($"[{System.DateTime.Now:HH:mm:ss}] {message}");
            Repaint();
        }
    }
}