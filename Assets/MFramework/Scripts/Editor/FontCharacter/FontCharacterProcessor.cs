using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace MFramework.Editor
{
    public class FontCharacterProcessor : EditorWindow
    {
        // 使用EditorPrefs保存的键名
        private const string FONT_FILE_PREF_KEY = "FontCharacterProcessor_FontFilePath";

        private string filePath = "Assets/art/font/Art_CN.txt";

        [MenuItem("Tools/字库工具/字库去重工具")]
        public static void ShowWindow()
        {
            GetWindow<FontCharacterProcessor>("字库去重工具");
        }

        void OnEnable()
        {
            // 从EditorPrefs加载保存的路径
            LoadSavedPath();
        }

        void OnDisable()
        {
            // 窗口关闭时保存当前路径
            SaveCurrentPath();
        }

        void OnDestroy()
        {
            // 窗口销毁时保存当前路径
            SaveCurrentPath();
        }

        private void LoadSavedPath()
        {
            // 加载字库文件路径
            string savedPath = EditorPrefs.GetString(FONT_FILE_PREF_KEY, "");
            if (!string.IsNullOrEmpty(savedPath) && File.Exists(savedPath))
            {
                filePath = savedPath;
            }
            else
            {
                // 如果保存的路径不存在，尝试使用默认路径
                if (!File.Exists(filePath))
                {
                    // 尝试查找其他可能的字库文件
                    string[] possibleFiles = Directory.GetFiles("Assets", "*.txt", SearchOption.AllDirectories)
                        .Where(f => f.ToLower().Contains("font") || f.ToLower().Contains("art") || f.ToLower().Contains("cn"))
                        .ToArray();

                    if (possibleFiles.Length > 0)
                    {
                        filePath = possibleFiles[0];
                    }
                }
            }
        }

        private void SaveCurrentPath()
        {
            // 保存字库文件路径
            if (!string.IsNullOrEmpty(filePath))
            {
                EditorPrefs.SetString(FONT_FILE_PREF_KEY, filePath);
            }
        }

        void OnGUI()
        {
            GUILayout.Label("字库文件去重工具", EditorStyles.boldLabel);

            EditorGUILayout.Space();

            // 文件路径选择
            EditorGUILayout.BeginHorizontal();
            string newFilePath = EditorGUILayout.TextField("字库文件路径:", filePath);
            if (newFilePath != filePath)
            {
                filePath = newFilePath;
                SaveCurrentPath(); // 立即保存新路径
            }

            if (GUILayout.Button("选择文件", GUILayout.Width(80)))
            {
                string selectedPath = EditorUtility.OpenFilePanel("选择字库文件",
                    Path.GetDirectoryName(filePath), "txt");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    filePath = selectedPath;
                    SaveCurrentPath(); // 立即保存新路径
                }
            }
            EditorGUILayout.EndHorizontal();

            // 显示文件状态
            if (!File.Exists(filePath))
            {
                EditorGUILayout.HelpBox("文件不存在", MessageType.Warning);
            }
            else
            {
                try
                {
                    string content = File.ReadAllText(filePath, Encoding.UTF8);
                    int charCount = content.Length;
                    int uniqueCount = content.Distinct().Count();

                    string statusText = $"字符总数: {charCount}";
                    if (charCount != uniqueCount)
                    {
                        statusText += $", 唯一字符: {uniqueCount}, 重复字符: {charCount - uniqueCount}";
                    }

                    EditorGUILayout.HelpBox(statusText, MessageType.Info);
                }
                catch (System.Exception e)
                {
                    EditorGUILayout.HelpBox($"读取文件失败: {e.Message}", MessageType.Error);
                }
            }

            EditorGUILayout.Space();

            // 操作按钮
            if (GUILayout.Button("一键去重", GUILayout.Height(40)))
            {
                RemoveDuplicateCharacters();
            }

            EditorGUILayout.Space();

            // 额外功能按钮
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("备份原文件", GUILayout.Height(25)))
            {
                BackupOriginalFile();
            }

            if (GUILayout.Button("重置路径", GUILayout.Height(25)))
            {
                ResetPath();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("功能说明:\n1. 读取指定路径的字库文件\n2. 去除所有重复字符\n3. 保持字符原有顺序\n4. 自动备份原文件", MessageType.Info);
        }

        public void RemoveDuplicateCharacters()
        {
            if (!File.Exists(filePath))
            {
                EditorUtility.DisplayDialog("错误", $"文件不存在: {filePath}", "确定");
                return;
            }

            try
            {
                string content = File.ReadAllText(filePath, Encoding.UTF8);

                if (string.IsNullOrEmpty(content))
                {
                    EditorUtility.DisplayDialog("警告", "文件内容为空", "确定");
                    return;
                }

                int originalCount = content.Length;

                // 检查是否已经有重复字符
                int uniqueCount = content.Distinct().Count();
                if (originalCount == uniqueCount)
                {
                    EditorUtility.DisplayDialog("提示", "文件中没有重复字符，无需处理", "确定");
                    return;
                }

                // 备份原文件
                string backupPath = CreateBackup();

                // 使用HashSet去重并保持顺序
                HashSet<char> seenCharacters = new HashSet<char>();
                StringBuilder result = new StringBuilder();

                foreach (char c in content)
                {
                    if (!seenCharacters.Contains(c))
                    {
                        seenCharacters.Add(c);
                        result.Append(c);
                    }
                }

                string processedContent = result.ToString();

                // 写入去重后的内容
                File.WriteAllText(filePath, processedContent, Encoding.UTF8);

                AssetDatabase.Refresh();

                int processedCount = processedContent.Length;
                int removedCount = originalCount - processedCount;

                EditorUtility.DisplayDialog("处理完成",
                    $"原文件字符数: {originalCount}\n" +
                    $"去重后字符数: {processedCount}\n" +
                    $"去除重复字符: {removedCount}\n" +
                    $"原文件已备份至: {backupPath}", "确定");

                Debug.Log($"字库去重完成: 原{originalCount}字符 -> 现{processedCount}字符, 去除{removedCount}个重复字符");

                // 重新加载路径以确保UI更新
                LoadSavedPath();
                Repaint();
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("错误", $"处理文件时发生错误: {e.Message}", "确定");
                Debug.LogError($"字库去重错误: {e}");
            }
        }

        private string CreateBackup()
        {
            string backupDir = Path.Combine(Path.GetDirectoryName(filePath), "Backups");
            if (!Directory.Exists(backupDir))
            {
                Directory.CreateDirectory(backupDir);
            }

            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupPath = Path.Combine(backupDir, $"{fileName}_backup_{timestamp}{extension}");

            File.Copy(filePath, backupPath, true);
            return backupPath;
        }

        private void BackupOriginalFile()
        {
            if (!File.Exists(filePath))
            {
                EditorUtility.DisplayDialog("错误", $"文件不存在: {filePath}", "确定");
                return;
            }

            try
            {
                string backupPath = CreateBackup();
                EditorUtility.DisplayDialog("备份完成", $"文件已备份至:\n{backupPath}", "确定");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("错误", $"备份文件时发生错误: {e.Message}", "确定");
            }
        }

        private void ResetPath()
        {
            if (EditorUtility.DisplayDialog("确认", "确定要重置路径为默认值吗？", "是", "否"))
            {
                // 清除保存的路径
                EditorPrefs.DeleteKey(FONT_FILE_PREF_KEY);

                // 重置为默认值
                filePath = "Assets/art/font/Art_CN.txt";

                // 尝试查找合适的文件
                LoadSavedPath();
                Repaint();
            }
        }

        // 右键菜单：快速处理选中的文本文件
        [MenuItem("Assets/字库工具/字库文件去重", false, 32)]
        static void ProcessSelectedTextFile()
        {
            Object selected = Selection.activeObject;
            if (selected != null)
            {
                string path = AssetDatabase.GetAssetPath(selected);
                if (path.EndsWith(".txt"))
                {
                    FontCharacterProcessor window = GetWindow<FontCharacterProcessor>();
                    window.filePath = path;
                    window.SaveCurrentPath(); // 保存新路径
                    window.RemoveDuplicateCharacters();
                }
                else
                {
                    EditorUtility.DisplayDialog("错误", "请选择.txt格式的文本文件", "确定");
                }
            }
        }

        [MenuItem("Assets/字库工具/字库文件去重", true)]
        static bool ValidateProcessSelectedTextFile()
        {
            Object selected = Selection.activeObject;
            return selected != null && AssetDatabase.GetAssetPath(selected).EndsWith(".txt");
        }
    } 
}