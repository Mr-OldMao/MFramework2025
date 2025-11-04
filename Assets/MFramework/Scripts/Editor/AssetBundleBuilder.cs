using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace MFramework.Editor
{
    public class AssetBundleBuilder : EditorWindow
    {
        private string targetFolder = "Assets/Art/Download";
        private BuildTarget buildTarget = BuildTarget.StandaloneWindows;
        private bool clearManifest = true;
        private bool forceRebuild = false;
        private string outputPath = "AssetBundles";

        [MenuItem("Tools/AssetBundle Builder")]
        public static void ShowWindow()
        {
            GetWindow<AssetBundleBuilder>("AssetBundle Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("AssetBundle 打包工具", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 目标文件夹设置
            EditorGUILayout.LabelField("目标文件夹:", targetFolder);
            EditorGUILayout.Space();

            // 构建平台选择
            buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("构建平台:", buildTarget);
            EditorGUILayout.Space();

            // 输出路径
            outputPath = EditorGUILayout.TextField("输出路径:", outputPath);
            EditorGUILayout.Space();

            // 选项
            clearManifest = EditorGUILayout.Toggle("清理 Manifest", clearManifest);
            forceRebuild = EditorGUILayout.Toggle("强制重建", forceRebuild);
            EditorGUILayout.Space();

            // 按钮
            if (GUILayout.Button("构建 AssetBundles", GUILayout.Height(30)))
            {
                BuildAllAssetBundles();
            }

            if (GUILayout.Button("设置 AssetBundle 名称", GUILayout.Height(25)))
            {
                SetAssetBundleNames();
            }

            if (GUILayout.Button("清理 AssetBundle 名称", GUILayout.Height(25)))
            {
                ClearAssetBundleNames();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("将自动打包 " + targetFolder + " 文件夹下的所有资源", MessageType.Info);
        }

        [MenuItem("Tools/AssetBundle Builder/快速构建")]
        public static void QuickBuild()
        {
            BuildAllAssetBundlesQuick();
        }

        private static void BuildAllAssetBundlesQuick()
        {
            string folderPath = "Assets/Art/Download";
            string outputPath = "AssetBundles";

            SetAssetBundleNamesForFolder(folderPath);
            BuildAssetBundles(outputPath, BuildTarget.StandaloneWindows, true, false);
        }

        private void BuildAllAssetBundles()
        {
            try
            {
                // 设置AssetBundle名称
                SetAssetBundleNames();

                // 构建AssetBundles
                BuildAssetBundles(outputPath, buildTarget, clearManifest, forceRebuild);

                EditorUtility.DisplayDialog("成功", "AssetBundle 构建完成！", "确定");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("错误", "构建失败: " + e.Message, "确定");
                Debug.LogError("AssetBundle 构建失败: " + e.Message);
            }
        }

        private void SetAssetBundleNames()
        {
            SetAssetBundleNamesForFolder(targetFolder);
            AssetDatabase.RemoveUnusedAssetBundleNames();
            AssetDatabase.Refresh();
            Debug.Log("AssetBundle 名称设置完成");
        }

        private void ClearAssetBundleNames()
        {
            ClearAssetBundleNamesForFolder(targetFolder);
            AssetDatabase.RemoveUnusedAssetBundleNames();
            AssetDatabase.Refresh();
            Debug.Log("AssetBundle 名称清理完成");
        }

        public static void SetAssetBundleNamesForFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Debug.LogWarning("文件夹不存在: " + folderPath);
                return;
            }

            // 获取文件夹下的所有资源文件
            string[] allFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
            int count = 0;

            foreach (string filePath in allFiles)
            {
                // 跳过.meta文件
                if (filePath.EndsWith(".meta"))
                    continue;

                string assetPath = filePath.Replace("\\", "/");
                AssetImporter importer = AssetImporter.GetAtPath(assetPath);

                if (importer != null)
                {
                    // 基于相对路径设置AssetBundle名称
                    string bundleName = GetBundleNameFromPath(assetPath, folderPath);
                    importer.assetBundleName = bundleName.ToLower(); // 统一小写
                    count++;
                }
            }

            Debug.Log($"已为 {count} 个资源设置 AssetBundle 名称");
        }

        public static void ClearAssetBundleNamesForFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                return;

            string[] allFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
            int count = 0;

            foreach (string filePath in allFiles)
            {
                if (filePath.EndsWith(".meta"))
                    continue;

                string assetPath = filePath.Replace("\\", "/");
                AssetImporter importer = AssetImporter.GetAtPath(assetPath);

                if (importer != null && !string.IsNullOrEmpty(importer.assetBundleName))
                {
                    importer.assetBundleName = null;
                    count++;
                }
            }

            Debug.Log($"已清理 {count} 个资源的 AssetBundle 名称");
        }

        private static string GetBundleNameFromPath(string assetPath, string baseFolderPath)
        {
            // 获取相对于基础文件夹的路径
            string relativePath = assetPath.Replace(baseFolderPath + "/", "");

            // 移除文件扩展名
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(relativePath);
            string directory = Path.GetDirectoryName(relativePath);

            // 如果文件在子文件夹中，使用"文件夹/文件名"作为bundle名称
            if (!string.IsNullOrEmpty(directory) && directory != ".")
            {
                return Path.Combine(directory, fileNameWithoutExtension).Replace("\\", "/");
            }
            else
            {
                return fileNameWithoutExtension;
            }
        }

        public static void BuildAssetBundles(string outputPath, BuildTarget target, bool clearManifest, bool forceRebuild)
        {
            // 确保输出目录存在
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            BuildAssetBundleOptions options = BuildAssetBundleOptions.None;

            if (clearManifest)
            {
                options |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
            }

            if (forceRebuild)
            {
                options |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
            }

            // 构建AssetBundles
            BuildPipeline.BuildAssetBundles(outputPath, options, target);

            Debug.Log($"AssetBundles 构建完成！输出路径: {outputPath}");
            Debug.Log($"构建平台: {target}");

            // 显示构建结果
            ShowBuildResult(outputPath);
        }

        private static void ShowBuildResult(string outputPath)
        {
            string[] files = Directory.GetFiles(outputPath);
            List<string> bundleFiles = new List<string>();

            foreach (string file in files)
            {
                if (!file.EndsWith(".meta") && !file.EndsWith(".manifest"))
                {
                    FileInfo fileInfo = new FileInfo(file);
                    bundleFiles.Add($"{Path.GetFileName(file)} ({fileInfo.Length / 1024} KB)");
                }
            }

            Debug.Log($"生成的 AssetBundle 文件 ({bundleFiles.Count} 个):");
            foreach (string bundleFile in bundleFiles)
            {
                Debug.Log($" - {bundleFile}");
            }
        }
    } 
}