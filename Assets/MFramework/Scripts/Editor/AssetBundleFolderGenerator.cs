using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class AssetBundleFolderGenerator : EditorWindow
{
    private static readonly string ArtRootPath = "Assets/Art";
    private static readonly string DownloadRootPath = "Assets/Download";

    // 预定义的文件夹结构 - 美术资源目录
    private static readonly List<string> ArtFolderPaths = new List<string>
    {
        // 预制体相关
        "prefabs/ui/common",
        "prefabs/ui/battle",
        "prefabs/ui/system",
        "prefabs/effects/ui",
        "prefabs/effects/environment",
        
        // 模型相关
        "models/characters/heroes",
        "models/characters/npcs",
        "models/environment/terrain",
        "models/environment/buildings",
        "models/props/weapons",
        "models/props/armors",
        
        // 纹理相关
        "textures/characters/heroes",
        "textures/characters/monsters",
        "textures/characters/npcs",
        "textures/ui/icons",
        "textures/ui/backgrounds",
        "textures/ui/frames",
        "textures/environment/terrain",
        "textures/environment/buildings",
        "textures/effects/particles",
        "textures/atlas/common",
        "textures/atlas/ui",
        
        // 材质相关
        "materials/characters",
        "materials/environment",
        "materials/effects",
        "materials/ui",
        
        // 动画相关
        "animations/characters/heroes",
        "animations/characters/monsters",
        "animations/characters/npcs",
        "animations/effects",
        "animations/ui",
        
        // 音频相关
        "audio/music/bgm",
        "audio/music/events",
        "audio/sfx/ui",
        "audio/sfx/battle",
        "audio/sfx/characters",
        "audio/sfx/environment",
        "audio/sfx/weapons",
        
        // Shader相关
        "shaders/common",
        "shaders/characters",
        "shaders/environment",
        "shaders/effects",
        "shaders/ui",
        
        // 字体相关
        "fonts/main",
        "fonts/special",
        "fonts/numbers",
        
        // 场景相关
        "scenes/main",
        "scenes/battle",
        "scenes/special",
        "scenes/ui",
        
        // 其他资源
        "others/videos",
        "others/rawdata",
        "others/references"
    };

    // 程序AB包资源目录结构 - 对应打包分组
    private static readonly List<string> DownloadFolderPaths = new List<string>
    {
        // 按功能模块分组
        "base",              // 基础资源
        "ui",               // UI资源
        "characters",       // 角色资源
        "monsters",         // 怪物资源
        "environment",      // 环境资源
        "effects",          // 特效资源
        "audio",            // 音频资源
        "config",           // 配置数据
        
        // 按资源类型细分
        "ui/common",
        "ui/battle",
        "ui/system",
        "characters/heroes",
        "characters/monsters",
        "characters/npcs",
        "environment/terrain",
        "environment/buildings",
        "effects/skills",
        "effects/ui",
        "audio/bgm",
        "audio/sfx",
        "config/data",
        "config/language"
    };

    [MenuItem("Tools/文件夹批量生成器")]
    public static void ShowWindow()
    {
        var window = GetWindow<AssetBundleFolderGenerator>("文件夹批量生成器");
        window.minSize = new Vector2(500, 600);
        window.Show();
    }

    private Vector2 scrollPosition;
    private bool generateArtFolders = true;
    private bool generateDownloadFolders = true;
    private bool[] artFolderToggleStates;
    private bool[] downloadFolderToggleStates;
    private int selectedTab = 0;
    private readonly string[] tabNames = { "美术资源目录", "程序AB资源目录" };

    private void OnEnable()
    {
        // 初始化选中状态
        artFolderToggleStates = new bool[ArtFolderPaths.Count];
        downloadFolderToggleStates = new bool[DownloadFolderPaths.Count];

        for (int i = 0; i < artFolderToggleStates.Length; i++)
        {
            artFolderToggleStates[i] = true;
        }

        for (int i = 0; i < downloadFolderToggleStates.Length; i++)
        {
            downloadFolderToggleStates[i] = true;
        }
    }

    private void OnGUI()
    {
        GUILayout.Space(10);

        // 标题
        EditorGUILayout.LabelField("核心目录文件夹结构批量生成器", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 目录说明
        EditorGUILayout.HelpBox(
            "美术资源目录 (Assets/Art): 存放原始美术资源，用于编辑和制作\n" +
            "程序AB包资源目录 (Assets/Download): 存放打包资源",
            MessageType.Info
        );

        EditorGUILayout.Space();

        // 标签页
        selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
        EditorGUILayout.Space();

        // 根据选中的标签显示不同的内容
        switch (selectedTab)
        {
            case 0:
                DrawArtFolderTab();
                break;
            case 1:
                DrawDownloadFolderTab();
                break;
        }

        EditorGUILayout.Space();

        // 操作按钮
        DrawActionButtons();
    }

    private void DrawArtFolderTab()
    {
        EditorGUILayout.LabelField("美术资源目录结构", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"路径: {ArtRootPath}", EditorStyles.miniLabel);

        EditorGUILayout.Space();

        // 控制选项
        EditorGUILayout.BeginHorizontal();
        generateArtFolders = EditorGUILayout.Toggle("生成美术目录", generateArtFolders);
        if (GUILayout.Button("全选"))
        {
            SetAllArtFoldersState(true);
        }
        if (GUILayout.Button("全不选"))
        {
            SetAllArtFoldersState(false);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // 文件夹列表
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));

        for (int i = 0; i < ArtFolderPaths.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            artFolderToggleStates[i] = EditorGUILayout.ToggleLeft("", artFolderToggleStates[i], GUILayout.Width(20));

            string folderPath = ArtFolderPaths[i];
            GUIStyle labelStyle = GetArtFolderStyle(folderPath);
            EditorGUILayout.LabelField(folderPath, labelStyle);

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        // 统计信息
        int selectedCount = GetSelectedArtFolderCount();
        EditorGUILayout.LabelField($"已选择: {selectedCount}/{ArtFolderPaths.Count} 个文件夹", EditorStyles.helpBox);
    }

    private void DrawDownloadFolderTab()
    {
        EditorGUILayout.LabelField("程序AB包资源目录结构", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"路径: {DownloadRootPath}", EditorStyles.miniLabel);

        EditorGUILayout.Space();

        // 控制选项
        EditorGUILayout.BeginHorizontal();
        generateDownloadFolders = EditorGUILayout.Toggle("生成程序AB包资源目录", generateDownloadFolders);
        if (GUILayout.Button("全选"))
        {
            SetAllDownloadFoldersState(true);
        }
        if (GUILayout.Button("全不选"))
        {
            SetAllDownloadFoldersState(false);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // 文件夹列表
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));

        for (int i = 0; i < DownloadFolderPaths.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            downloadFolderToggleStates[i] = EditorGUILayout.ToggleLeft("", downloadFolderToggleStates[i], GUILayout.Width(20));

            string folderPath = DownloadFolderPaths[i];
            GUIStyle labelStyle = GetDownloadFolderStyle(folderPath);
            EditorGUILayout.LabelField(folderPath, labelStyle);

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        // 统计信息
        int selectedCount = GetSelectedDownloadFolderCount();
        EditorGUILayout.LabelField($"已选择: {selectedCount}/{DownloadFolderPaths.Count} 个文件夹", EditorStyles.helpBox);
    }

    private void DrawActionButtons()
    {
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("生成选中目录", GUILayout.Height(35)))
        {
            GenerateSelectedFolders();
        }

        if (GUILayout.Button("生成完整结构", GUILayout.Height(35)))
        {
            GenerateCompleteStructure();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // 工具按钮
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("清理空文件夹"))
        {
            CleanEmptyFolders();
        }
        EditorGUILayout.EndHorizontal();
    }

    private GUIStyle GetArtFolderStyle(string folderPath)
    {
        var style = new GUIStyle(EditorStyles.label);

        if (folderPath.StartsWith("prefabs"))
        {
            style.normal.textColor = new Color(0.2f, 0.6f, 1f); // 蓝色
        }
        else if (folderPath.StartsWith("models"))
        {
            style.normal.textColor = new Color(0.8f, 0.4f, 0.1f); // 棕色
        }
        else if (folderPath.StartsWith("textures"))
        {
            style.normal.textColor = new Color(0.9f, 0.6f, 0.2f); // 橙色
        }
        else if (folderPath.StartsWith("materials") || folderPath.StartsWith("shaders"))
        {
            style.normal.textColor = new Color(0.2f, 0.8f, 0.4f); // 绿色
        }
        else if (folderPath.StartsWith("audio"))
        {
            style.normal.textColor = new Color(0.8f, 0.4f, 0.9f); // 紫色
        }
        else
        {
            style.normal.textColor = EditorStyles.label.normal.textColor;
        }

        return style;
    }

    private GUIStyle GetDownloadFolderStyle(string folderPath)
    {
        var style = new GUIStyle(EditorStyles.label);

        if (folderPath.StartsWith("ui"))
        {
            style.normal.textColor = new Color(0.9f, 0.3f, 0.3f); // 红色
        }
        else if (folderPath.StartsWith("characters") || folderPath.StartsWith("monsters"))
        {
            style.normal.textColor = new Color(0.2f, 0.6f, 1f); // 蓝色
        }
        else if (folderPath.StartsWith("environment"))
        {
            style.normal.textColor = new Color(0.2f, 0.8f, 0.4f); // 绿色
        }
        else if (folderPath.StartsWith("effects"))
        {
            style.normal.textColor = new Color(0.8f, 0.4f, 0.9f); // 紫色
        }
        else if (folderPath.StartsWith("audio"))
        {
            style.normal.textColor = new Color(0.9f, 0.6f, 0.2f); // 橙色
        }
        else if (folderPath.StartsWith("config"))
        {
            style.normal.textColor = new Color(0.5f, 0.5f, 0.5f); // 灰色
        }
        else
        {
            style.normal.textColor = EditorStyles.label.normal.textColor;
        }

        return style;
    }

    private void SetAllArtFoldersState(bool state)
    {
        for (int i = 0; i < artFolderToggleStates.Length; i++)
        {
            artFolderToggleStates[i] = state;
        }
    }

    private void SetAllDownloadFoldersState(bool state)
    {
        for (int i = 0; i < downloadFolderToggleStates.Length; i++)
        {
            downloadFolderToggleStates[i] = state;
        }
    }

    private int GetSelectedArtFolderCount()
    {
        int count = 0;
        foreach (bool state in artFolderToggleStates)
        {
            if (state) count++;
        }
        return count;
    }

    private int GetSelectedDownloadFolderCount()
    {
        int count = 0;
        foreach (bool state in downloadFolderToggleStates)
        {
            if (state) count++;
        }
        return count;
    }

    private void GenerateSelectedFolders()
    {
        int artCreatedCount = 0;
        int downloadCreatedCount = 0;

        // 生成美术资源目录
        if (generateArtFolders)
        {
            if (!Directory.Exists(ArtRootPath))
            {
                Directory.CreateDirectory(ArtRootPath);
            }

            for (int i = 0; i < ArtFolderPaths.Count; i++)
            {
                if (artFolderToggleStates[i])
                {
                    string fullPath = Path.Combine(ArtRootPath, ArtFolderPaths[i]);
                    if (!Directory.Exists(fullPath))
                    {
                        Directory.CreateDirectory(fullPath);
                        artCreatedCount++;
                    }
                }
            }
        }

        // 生成程序AB包资源目录
        if (generateDownloadFolders)
        {
            if (!Directory.Exists(DownloadRootPath))
            {
                Directory.CreateDirectory(DownloadRootPath);
            }

            for (int i = 0; i < DownloadFolderPaths.Count; i++)
            {
                if (downloadFolderToggleStates[i])
                {
                    string fullPath = Path.Combine(DownloadRootPath, DownloadFolderPaths[i]);
                    if (!Directory.Exists(fullPath))
                    {
                        Directory.CreateDirectory(fullPath);
                        downloadCreatedCount++;

                        // 在程序AB包资源目录中创建.readme文件说明
                        CreateReadmeFile(fullPath);
                    }
                }
            }
        }

        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("完成",
            $"生成完成！\n" +
            $"美术目录: {artCreatedCount} 个文件夹\n" +
            $"程序AB包资源目录: {downloadCreatedCount} 个文件夹", "确定");

        Debug.Log($"双目录结构生成完成 - 美术: {artCreatedCount}, AB包: {downloadCreatedCount}");
    }

    private void CreateReadmeFile(string folderPath)
    {
        string readmePath = Path.Combine(folderPath, "AB包说明.txt");
        if (!File.Exists(readmePath))
        {
            string folderName = Path.GetFileName(folderPath);
            string content = $@"此文件夹用于存放 '{folderName}' 相关的AssetBundle资源

使用说明:
1. 将需要打包的资源放入此文件夹
2. 在Inspector中设置AssetBundle标签
3. 使用AssetBundle Browser进行打包

注意事项:
- 保持资源依赖关系清晰
- 避免循环依赖
- 控制单个AB包大小";

            File.WriteAllText(readmePath, content);
        }
    }

    private void GenerateCompleteStructure()
    {
        SetAllArtFoldersState(true);
        SetAllDownloadFoldersState(true);
        GenerateSelectedFolders();
    }

    private void CleanEmptyFolders()
    {
        if (EditorUtility.DisplayDialog("确认清理",
            "确定要清理两个目录下的所有空文件夹吗？", "是", "否"))
        {
            int artRemoved = DeleteEmptyFoldersRecursive(ArtRootPath);
            int downloadRemoved = DeleteEmptyFoldersRecursive(DownloadRootPath);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("完成",
                $"清理完成！\n美术目录: {artRemoved} 个空文件夹\n程序AB包资源目录: {downloadRemoved} 个空文件夹", "确定");
        }
    }

    private int DeleteEmptyFoldersRecursive(string path)
    {
        if (!Directory.Exists(path))
            return 0;

        int removedCount = 0;

        string[] subDirectories = Directory.GetDirectories(path);
        foreach (string subDir in subDirectories)
        {
            removedCount += DeleteEmptyFoldersRecursive(subDir);
        }

        string[] allFiles = Directory.GetFiles(path);
        string[] allDirs = Directory.GetDirectories(path);

        bool hasRealFiles = false;
        foreach (string file in allFiles)
        {
            if (!file.EndsWith(".meta") && !file.EndsWith("AB包说明.txt"))
            {
                hasRealFiles = true;
                break;
            }
        }

        if (!hasRealFiles && allDirs.Length == 0)
        {
            Directory.Delete(path, true);
            string metaFile = path + ".meta";
            if (File.Exists(metaFile))
            {
                File.Delete(metaFile);
            }
            removedCount++;
        }

        return removedCount;
    }
}