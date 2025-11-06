using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class AdvancedAssetBundleBuilder : EditorWindow
{
    private string targetFolder = "Assets/Art/Download";
    private BuildTarget buildTarget = BuildTarget.StandaloneWindows;
    private bool clearManifest = true;
    private bool forceRebuild = false;
    private string outputPath = "AssetBundles";
    private Vector2 scrollPosition;

    [MenuItem("Tools/Advanced AssetBundle Builder")]
    public static void ShowWindow()
    {
        GetWindow<AdvancedAssetBundleBuilder>("Advanced AB Builder");
    }

    private void OnGUI()
    {
        GUILayout.Label("高级 AssetBundle 打包工具", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

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

        // 显示打包策略说明
        EditorGUILayout.HelpBox(
            "打包策略:\n" +
            "- Download文件夹下的每个子文件 → 单独AB包\n" +
            "- Download文件夹下的每个子文件夹 → 合并为一个AB包",
            MessageType.Info
        );
        EditorGUILayout.Space();

        // 按钮区域
        if (GUILayout.Button("分析资源结构", GUILayout.Height(30)))
        {
            AnalyzeFolderStructure();
        }

        if (GUILayout.Button("设置AB包名称", GUILayout.Height(30)))
        {
            SetAssetBundleNames();
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("构建AB包", GUILayout.Height(40)))
        {
            BuildAssetBundles();
        }

        if (GUILayout.Button("清理AB名称", GUILayout.Height(40)))
        {
            ClearAssetBundleNames();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();
    }

    private void AnalyzeFolderStructure()
    {
        if (!Directory.Exists(targetFolder))
        {
            Debug.LogError($"目标文件夹不存在: {targetFolder}");
            return;
        }

        Debug.Log("=== 资源结构分析 ===");

        // 分析直接文件
        string[] directFiles = Directory.GetFiles(targetFolder)
            .Where(file => !file.EndsWith(".meta"))
            .ToArray();

        Debug.Log($"Download文件夹下的直接文件 ({directFiles.Length} 个):");
        foreach (string file in directFiles)
        {
            string fileName = Path.GetFileName(file);
            Debug.Log($"  📄 {fileName} → 单独AB包: {GetBundleNameForFile(file)}");
        }

        // 分析子文件夹
        string[] subFolders = Directory.GetDirectories(targetFolder);
        Debug.Log($"子文件夹 ({subFolders.Length} 个):");

        foreach (string folder in subFolders)
        {
            string folderName = Path.GetFileName(folder);
            string[] folderFiles = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories)
                .Where(file => !file.EndsWith(".meta"))
                .ToArray();

            Debug.Log($"  📁 {folderName} (包含 {folderFiles.Length} 个资源) → 合并AB包: {GetBundleNameForFolder(folder)}");

            // 显示文件夹内的文件详情
            foreach (string file in folderFiles.Take(5)) // 只显示前5个文件
            {
                string relativePath = file.Replace(folder + Path.DirectorySeparatorChar, "");
                Debug.Log($"      ↳ {relativePath}");
            }

            if (folderFiles.Length > 5)
            {
                Debug.Log($"      ... 还有 {folderFiles.Length - 5} 个文件");
            }
        }

        Debug.Log("=== 分析完成 ===");
    }

    private void SetAssetBundleNames()
    {
        if (!Directory.Exists(targetFolder))
        {
            Debug.LogError($"目标文件夹不存在: {targetFolder}");
            return;
        }

        try
        {
            // 先清理所有AB名称
            ClearAssetBundleNamesForFolder(targetFolder);

            int fileCount = 0;
            int folderCount = 0;

            // 处理直接文件 - 每个文件单独AB包
            string[] directFiles = Directory.GetFiles(targetFolder)
                .Where(file => !file.EndsWith(".meta"))
                .ToArray();

            foreach (string filePath in directFiles)
            {
                string assetPath = filePath.Replace("\\", "/");
                AssetImporter importer = AssetImporter.GetAtPath(assetPath);

                if (importer != null)
                {
                    string bundleName = GetBundleNameForFile(filePath);
                    importer.assetBundleName = bundleName;
                    fileCount++;
                    Debug.Log($"设置文件AB包: {Path.GetFileName(filePath)} → {bundleName}");
                }
            }

            // 处理子文件夹 - 每个文件夹合并为一个AB包
            string[] subFolders = Directory.GetDirectories(targetFolder);

            foreach (string folderPath in subFolders)
            {
                string bundleName = GetBundleNameForFolder(folderPath);
                string[] folderFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
                    .Where(file => !file.EndsWith(".meta"))
                    .ToArray();

                foreach (string filePath in folderFiles)
                {
                    string assetPath = filePath.Replace("\\", "/");
                    AssetImporter importer = AssetImporter.GetAtPath(assetPath);

                    if (importer != null)
                    {
                        importer.assetBundleName = bundleName;
                    }
                }

                folderCount++;
                Debug.Log($"设置文件夹AB包: {Path.GetFileName(folderPath)} → {bundleName} (包含 {folderFiles.Length} 个资源)");
            }

            AssetDatabase.RemoveUnusedAssetBundleNames();
            AssetDatabase.Refresh();

            Debug.Log($"AB包名称设置完成! 文件AB包: {fileCount}个, 文件夹AB包: {folderCount}个");
            EditorUtility.DisplayDialog("成功", $"AB包名称设置完成!\n文件AB包: {fileCount}个\n文件夹AB包: {folderCount}个", "确定");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"设置AB包名称失败: {e.Message}");
            EditorUtility.DisplayDialog("错误", $"设置AB包名称失败: {e.Message}", "确定");
        }
    }

    private string GetBundleNameForFile(string filePath)
    {
        string fileName = Path.GetFileNameWithoutExtension(filePath);
        // 使用文件名作为AB包名，转换为小写并移除特殊字符
        return CleanBundleName($"file_{fileName}");
    }

    private string GetBundleNameForFolder(string folderPath)
    {
        string folderName = Path.GetFileName(folderPath);
        // 使用文件夹名作为AB包名，转换为小写并移除特殊字符
        return CleanBundleName($"folder_{folderName}");
    }

    private string CleanBundleName(string name)
    {
        // 移除或替换无效字符，保持小写
        return name.ToLower()
            .Replace(" ", "_")
            .Replace("-", "_")
            .Replace("(", "")
            .Replace(")", "");
    }

    private void ClearAssetBundleNames()
    {
        ClearAssetBundleNamesForFolder(targetFolder);
        AssetDatabase.RemoveUnusedAssetBundleNames();
        AssetDatabase.Refresh();
        Debug.Log("已清理所有AB包名称");
        EditorUtility.DisplayDialog("成功", "已清理所有AB包名称", "确定");
    }

    private void ClearAssetBundleNamesForFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath)) return;

        // 清理所有文件和子文件夹中的AB名称
        string[] allFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
            .Where(file => !file.EndsWith(".meta"))
            .ToArray();

        foreach (string filePath in allFiles)
        {
            string assetPath = filePath.Replace("\\", "/");
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);

            if (importer != null)
            {
                importer.assetBundleName = null;
            }
        }
    }

    private void BuildAssetBundles()
    {
        try
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
            BuildPipeline.BuildAssetBundles(outputPath, options, buildTarget);

            // 显示构建结果
            ShowBuildResult();

            Debug.Log($"AssetBundles 构建完成! 输出路径: {outputPath}");
            EditorUtility.DisplayDialog("成功", "AssetBundle 构建完成!", "确定");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"构建失败: {e.Message}");
            EditorUtility.DisplayDialog("错误", $"构建失败: {e.Message}", "确定");
        }
    }

    private void ShowBuildResult()
    {
        if (!Directory.Exists(outputPath)) return;

        string[] files = Directory.GetFiles(outputPath);
        List<string> bundleFiles = new List<string>();
        long totalSize = 0;

        foreach (string file in files)
        {
            if (!file.EndsWith(".meta") && !file.EndsWith(".manifest"))
            {
                FileInfo fileInfo = new FileInfo(file);
                long fileSize = fileInfo.Length;
                totalSize += fileSize;
                bundleFiles.Add($"{Path.GetFileName(file)} ({FormatFileSize(fileSize)})");
            }
        }

        Debug.Log($"=== AssetBundle 构建结果 ===");
        Debug.Log($"输出目录: {outputPath}");
        Debug.Log($"构建平台: {buildTarget}");
        Debug.Log($"总文件数: {bundleFiles.Count}");
        Debug.Log($"总大小: {FormatFileSize(totalSize)}");
        Debug.Log($"生成的 AssetBundle 文件:");

        foreach (string bundleFile in bundleFiles)
        {
            Debug.Log($"  📦 {bundleFile}");
        }

        Debug.Log($"=== 构建完成 ===");
    }

    private string FormatFileSize(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB" };
        int counter = 0;
        decimal number = bytes;

        while (Mathf.Round((float)number) >= 1000 && counter < suffixes.Length - 1)
        {
            number /= 1024;
            counter++;
        }

        return string.Format("{0:n1} {1}", number, suffixes[counter]);
    }

    [MenuItem("Tools/Advanced AssetBundle Builder/快速构建")]
    public static void QuickBuild()
    {
        AdvancedAssetBundleBuilder window = GetWindow<AdvancedAssetBundleBuilder>();
        window.SetAssetBundleNames();
        window.BuildAssetBundles();
    }
}