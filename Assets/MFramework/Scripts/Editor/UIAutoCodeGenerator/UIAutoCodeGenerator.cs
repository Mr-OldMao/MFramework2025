using MFramework.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MFramework.Editor
{
    /// <summary>
    /// UI代码自动生成工具
    /// </summary>
    public partial class UIAutoCodeGenerator : EditorWindow
    {
        // EditorPrefs键名
        private readonly static string ROOT_DATA_PATH = Application.dataPath;
        private readonly static string PREFS_KEY_SCRIPT_PATH = ROOT_DATA_PATH + "MFramework.UICodeGenerator.ScriptOutputPath";
        private readonly static string PREFS_KEY_NAMESPACE = ROOT_DATA_PATH + "MFramework.UICodeGenerator.Namespace";
        private readonly static string PREFS_KEY_GENERATE_PARTIAL = ROOT_DATA_PATH + "MFramework.UICodeGenerator.GeneratePartialClass";
        private readonly static string PREFS_KEY_INCLUDE_ALL_COMPONENTS = ROOT_DATA_PATH + "MFramework.UICodeGenerator.IncludeAllComponents";

        // 默认值
        private const string DEFAULT_SCRIPT_PATH = "Assets/Scripts/UI";
        private const string DEFAULT_NAMESPACE = "GameMain";
        private const bool DEFAULT_GENERATE_PARTIAL = false;
        private const bool DEFAULT_INCLUDE_ALL_COMPONENTS = false;
        private UILayerType selectedLayerType; 

        private static readonly string[] DefaultNamesToIgnore = new[]
        {
            "Image", "Button", "Text", "Text (TMP)", "RawImage", "Toggle",
            "Slider", "Scrollbar", "InputField", "Dropdown", "ScrollRect",
            "Panel", "Canvas", "EventSystem", "GameObject", "RectTransform", "Transform",
            "LayoutGroup", "Mask", "TMP_InputField", "TMP_Dropdown", "TextMeshProUGUI"
        };

        // 组件优先级（从高到低）
        private static readonly Dictionary<Type, int> ComponentPriority = new Dictionary<Type, int>
        {
            { typeof(Button), 100 },
            { typeof(Toggle), 95 },
            { typeof(Slider), 90 },
            { typeof(Scrollbar), 85 },
            { typeof(Dropdown), 80 },
            { typeof(TMP_Dropdown), 80 },
            { typeof(InputField), 75 },
            { typeof(TMP_InputField), 75 },
            { typeof(ScrollRect), 70 },
            { typeof(TMP_Text), 65 },
            { typeof(Text), 60 },
            { typeof(Image), 50 },
            { typeof(RawImage), 45 },
            { typeof(LayoutGroup), 40 },
            { typeof(Mask), 35 },
            { typeof(RectTransform), 30 }
        };

        private static readonly Dictionary<Type, string> TypeAliases = new Dictionary<Type, string>
        {
            { typeof(GameObject), "GameObject" },
            { typeof(Button), "Button" },
            { typeof(Image), "Image" },
            { typeof(Text), "Text" },
            { typeof(TMP_Text), "TextMeshProUGUI" },
            { typeof(Toggle), "Toggle" },
            { typeof(Slider), "Slider" },
            { typeof(Scrollbar), "Scrollbar" },
            { typeof(InputField), "InputField" },
            { typeof(TMP_InputField), "TMP_InputField" },
            { typeof(Dropdown), "Dropdown" },
            { typeof(TMP_Dropdown), "TMP_Dropdown" },
            { typeof(ScrollRect), "ScrollRect" },
            { typeof(RawImage), "RawImage" },
            { typeof(LayoutGroup), "LayoutGroup" },
            { typeof(Mask), "Mask" },
            { typeof(RectTransform), "RectTransform" }
        };

        private GameObject selectedPrefab;
        private string prefabPath;
        private string scriptOutputPath;
        private string namespaceName;
        private bool includeAllComponents;
        private bool generatePartialClass;
        private Vector2 scrollPosition;

        [MenuItem("Tools/UI/自动生成UI代码")]
        public static void ShowWindow()
        {
            var window = GetWindow<UIAutoCodeGenerator>("UI代码生成器");
            window.minSize = new Vector2(500, 650);
            window.LoadSettings();
        }


        // 右键菜单快捷方式
        [MenuItem("Assets/生成UI代码", false, 20)]
        private static void GenerateUICodeFromContext()
        {
            GameObject selected = Selection.activeObject as GameObject;
            if (selected != null)
            {
                var window = GetWindow<UIAutoCodeGenerator>("UI代码生成器");
                window.selectedPrefab = selected;
                window.prefabPath = AssetDatabase.GetAssetPath(selected);
                window.UpdateScriptPath();
                window.Show();
            }
            else
            { 
                Debug.LogError("请选择UI预制体");
            }
        }

        [MenuItem("Assets/UI工具/生成UI代码", true)]
        private static bool ValidateGenerateUICodeFromContext()
        {
            return Selection.activeObject is GameObject;
        }

        // 添加清理编辑器缓存的菜单项
        [MenuItem("Tools/UI/清理UI生成器缓存")]
        private static void ClearGeneratorCache()
        {
            EditorPrefs.DeleteKey(PREFS_KEY_SCRIPT_PATH);
            EditorPrefs.DeleteKey(PREFS_KEY_NAMESPACE);
            EditorPrefs.DeleteKey(PREFS_KEY_INCLUDE_ALL_COMPONENTS);
            EditorPrefs.DeleteKey(PREFS_KEY_GENERATE_PARTIAL);

            Debug.Log("UI代码生成器缓存已清理");
        }

        private void OnEnable()
        {
            LoadSettings();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("UI代码自动生成工具", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // 1. 选择预制体
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("选择UI预制体", EditorStyles.boldLabel);

            GameObject newPrefab = (GameObject)EditorGUILayout.ObjectField("UI预制体", selectedPrefab, typeof(GameObject), false);
            if (newPrefab != selectedPrefab)
            {
                selectedPrefab = newPrefab;
                if (selectedPrefab != null)
                {
                    prefabPath = AssetDatabase.GetAssetPath(selectedPrefab);
                    UpdateScriptPath();
                }
            }

            EditorGUILayout.EndVertical();

            //EditorGUILayout.Space(5);

            // 2. 输出设置
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("输出设置", EditorStyles.boldLabel);

            // 脚本输出路径
            string newScriptPath = EditorGUILayout.TextField("脚本输出路径", scriptOutputPath);
            if (newScriptPath != scriptOutputPath)
            {
                scriptOutputPath = newScriptPath;
                SaveSettings();
            }

            // 命名空间
            string newNamespace = EditorGUILayout.TextField("命名空间", namespaceName);
            if (newNamespace != namespaceName)
            {
                namespaceName = newNamespace;
                SaveSettings();
            }

            EditorGUILayout.Space(5);

            // 包含所有组件
            bool newIncludeAll = EditorGUILayout.ToggleLeft("包含所有组件（不按优先级）", includeAllComponents);
            if (newIncludeAll != includeAllComponents)
            {
                includeAllComponents = newIncludeAll;
                SaveSettings();
            }

            // 生成分部类
            bool newGeneratePartial = EditorGUILayout.ToggleLeft("生成分部类（Partial）", generatePartialClass);
            if (newGeneratePartial != generatePartialClass)
            {
                generatePartialClass = newGeneratePartial;
                SaveSettings();
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            selectedLayerType = (UILayerType)EditorGUILayout.EnumPopup("UI层级类型", selectedLayerType);

            // 按钮区域

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUI.enabled = selectedPrefab != null;

            // 生成当前UI按钮
            if (GUILayout.Button("生成UI代码(仅View层)", GUILayout.Width(200), GUILayout.Height(30)))
            {
                GenerateUICode(true, selectedLayerType);
            }

            // 一键生成所有UI按钮
            if (GUILayout.Button("一键生成所有UI代码(MVC层)", GUILayout.Width(200), GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("批量生成", "确定要一键生成所有UI代码吗？", "确定", "取消"))
                {
                    GenerateUIMVCCode(selectedLayerType);
                }
            }

            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);

            // 3. 预览生成的字段
            if (selectedPrefab != null)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("将要生成的字段", EditorStyles.boldLabel);

                var uiFields = AnalyzeUIPrefab(selectedPrefab);

                if (uiFields.Count == 0)
                {
                    EditorGUILayout.HelpBox("未找到可生成的UI字段，请检查预制体。", MessageType.Info);
                }
                else
                {
                    foreach (var field in uiFields)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"{field.FieldType} {field.FieldName}");
                        if (GUILayout.Button("定位", GUILayout.Width(50)))
                        {
                            Selection.activeGameObject = selectedPrefab.transform.Find<Transform>(field.FieldName).gameObject;
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.LabelField($"总计：{uiFields.Count} 个字段", EditorStyles.miniBoldLabel);
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space(5);


            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            // 重置设置按钮
            if (GUILayout.Button("重置设置", GUILayout.Width(100), GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("重置设置", "确定要重置所有设置为默认值吗？", "确定", "取消"))
                {
                    ResetSettings();
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);

            // 5. 帮助信息和当前设置
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("帮助信息", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                "使用说明：\n" +
                "1. 选择UI预制体（命名规则：UIPanelXxx）\n" +
                "2. 设置输出路径、命名空间、UI层级\n" +
                "3. 点击生成按钮\n" +
                "\n注意：\n" +
                "- 默认忽略Image、Button等默认命名的节点\n" +
                "- 优先选择高优先级组件（如Button优先于Image）",
                MessageType.Info
            );

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();

            // 显示当前设置
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"当前设置: 路径={scriptOutputPath},UI层级={selectedLayerType},命名空间={namespaceName}, 分部类={generatePartialClass}", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
        }

        #region 设置管理
        private void LoadSettings()
        {
            // 加载保存的设置，如果没有则使用默认值
            scriptOutputPath = EditorPrefs.GetString(PREFS_KEY_SCRIPT_PATH, DEFAULT_SCRIPT_PATH);
            namespaceName = EditorPrefs.GetString(PREFS_KEY_NAMESPACE, DEFAULT_NAMESPACE);
            includeAllComponents = EditorPrefs.GetBool(PREFS_KEY_INCLUDE_ALL_COMPONENTS, DEFAULT_INCLUDE_ALL_COMPONENTS);
            generatePartialClass = EditorPrefs.GetBool(PREFS_KEY_GENERATE_PARTIAL, DEFAULT_GENERATE_PARTIAL);
        }

        private void SaveSettings()
        {
            // 保存当前设置到EditorPrefs
            EditorPrefs.SetString(PREFS_KEY_SCRIPT_PATH, scriptOutputPath);
            EditorPrefs.SetString(PREFS_KEY_NAMESPACE, namespaceName);
            EditorPrefs.SetBool(PREFS_KEY_INCLUDE_ALL_COMPONENTS, includeAllComponents);
            EditorPrefs.SetBool(PREFS_KEY_GENERATE_PARTIAL, generatePartialClass);
        }

        private void ResetSettings()
        {
            // 重置为默认值
            scriptOutputPath = DEFAULT_SCRIPT_PATH;
            namespaceName = DEFAULT_NAMESPACE;
            includeAllComponents = DEFAULT_INCLUDE_ALL_COMPONENTS;
            generatePartialClass = DEFAULT_GENERATE_PARTIAL;

            // 保存默认值
            SaveSettings();

            Debug.Log("UI代码生成器设置已重置为默认值");
            Repaint(); // 重绘窗口以更新显示
        }
        #endregion

        private void UpdateScriptPath()
        {
            if (string.IsNullOrEmpty(prefabPath)) return;

            // 尝试自动推断脚本路径
            string directory = Path.GetDirectoryName(prefabPath);
            if (directory.Contains("Resources") || directory.Contains("Prefabs"))
            {
                scriptOutputPath = directory.Replace("Resources", "Scripts")
                                          .Replace("Prefabs", "Scripts");
                SaveSettings(); // 保存自动推断的路径
            }
        }

        private List<UIFieldInfo> AnalyzeUIPrefab(GameObject prefab)
        {
            var uiFields = new List<UIFieldInfo>();
            AnalyzeTransform(prefab.transform, "", uiFields);
            return uiFields;
        }

        private void AnalyzeTransform(Transform transform, string parentPath, List<UIFieldInfo> uiFields)
        {
            string currentPath = string.IsNullOrEmpty(parentPath)
                ? transform.name
                : $"{parentPath}/{transform.name}";

            // 检查是否需要生成字段（排除默认命名的节点）
            if (!ShouldIgnoreNode(transform.name))
            {
                var fieldInfo = GetFieldInfoForTransform(transform, currentPath);
                if (fieldInfo != null)
                {
                    uiFields.Add(fieldInfo);
                }
            }

            // 递归分析子节点
            for (int i = 0; i < transform.childCount; i++)
            {
                AnalyzeTransform(transform.GetChild(i), currentPath, uiFields);
            }
        }

        private bool ShouldIgnoreNode(string nodeName)
        {
            if (selectedPrefab.name == nodeName)
            {
                return true;
            }

            if (nodeName.Contains(" "))
            {
                return true;
            }

            // 检查是否是默认命名的节点
            foreach (var defaultName in DefaultNamesToIgnore)
            {
                if (nodeName.Equals(defaultName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                // 也检查类似"Image (1)"这样的变体
                if (nodeName.StartsWith(defaultName + " (") && nodeName.EndsWith(")"))
                {
                    return true;
                }
            }

            // 检查是否包含特定后缀（可扩展）
            string[] ignoreSuffixes = { "Panel", "View", "Container", "Group", "Area" };
            foreach (var suffix in ignoreSuffixes)
            {
                if (nodeName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    return false; // 这些通常不需要忽略，因为它们通常是容器
                }
            }

            return false;
        }

        private UIFieldInfo GetFieldInfoForTransform(Transform transform, string nodePath)
        {
            // 获取所有UI相关的组件
            var components = GetUIComponents(transform);
            if (components.Count == 0)
            {
                return null; // 没有UI组件，不生成字段
            }

            // 根据优先级选择组件类型
            Type selectedComponentType;
            if (includeAllComponents)
            {
                // 如果包含所有组件，选择第一个
                selectedComponentType = components[0];
            }
            else
            {
                // 根据优先级选择
                selectedComponentType = GetHighestPriorityComponent(components);
            }

            // 生成字段名（将节点名转换为camelCase）
            //string fieldName = ConvertToCamelCase(transform.name);
            string fieldName = transform.name;

            //// 确保字段名以合适的前缀开头
            //fieldName = EnsureFieldNamePrefix(fieldName, selectedComponentType);

            return new UIFieldInfo
            {
                FieldName = fieldName,
                FieldType = GetTypeAlias(selectedComponentType),
                NodePath = nodePath,
                ComponentType = selectedComponentType
            };
        }

        private List<Type> GetUIComponents(Transform transform)
        {
            var uiComponents = new List<Type>();
            var components = transform.GetComponents<Component>();

            foreach (var component in components)
            {
                if (component == null) continue;

                Type componentType = component.GetType();
                if (TypeAliases.ContainsKey(componentType) ||
                    IsUIComponentType(componentType))
                {
                    uiComponents.Add(componentType);
                }
            }

            return uiComponents;
        }

        private bool IsUIComponentType(Type type)
        {
            // 检查是否是UI组件类型
            return type == typeof(Transform) ||
                   type.IsSubclassOf(typeof(UIBehaviour)) ||
                   type == typeof(RectTransform) ||
                   type == typeof(Canvas) ||
                   type == typeof(CanvasRenderer);
        }

        private Type GetHighestPriorityComponent(List<Type> components)
        {
            Type highestPriorityType = null;
            int highestPriority = -1;

            foreach (var componentType in components)
            {
                int priority = 0;
                if (ComponentPriority.TryGetValue(componentType, out priority))
                {
                    if (priority > highestPriority)
                    {
                        highestPriority = priority;
                        highestPriorityType = componentType;
                    }
                }
                else if (highestPriorityType == null)
                {
                    highestPriorityType = componentType;
                }
            }

            return highestPriorityType ?? typeof(GameObject);
        }

        private string ConvertToCamelCase(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;

            // 移除特殊字符和空格
            string cleaned = new string(name.Where(c => char.IsLetterOrDigit(c)).ToArray());

            // 转换为camelCase
            if (cleaned.Length > 0)
            {
                cleaned = char.ToLowerInvariant(cleaned[0]) + cleaned.Substring(1);
            }

            return cleaned;
        }

        private string EnsureFieldNamePrefix(string fieldName, Type componentType)
        {
            // 根据组件类型添加合适的前缀
            string prefix = GetComponentPrefix(componentType);

            // 如果字段名已经以合适的前缀开头，直接返回
            if (fieldName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return fieldName;
            }

            // 否则添加前缀
            return prefix + char.ToUpperInvariant(fieldName[0]) + fieldName.Substring(1);
        }

        private string GetComponentPrefix(Type componentType)
        {
            if (componentType == typeof(Button)) return "btn";
            if (componentType == typeof(Image)) return "img";
            if (componentType == typeof(RawImage)) return "rawImg";
            if (componentType == typeof(Text) || componentType == typeof(TMP_Text)) return "txt";
            if (componentType == typeof(Toggle)) return "tog";
            if (componentType == typeof(Slider)) return "sld";
            if (componentType == typeof(Scrollbar)) return "scrBar";
            if (componentType == typeof(InputField) || componentType == typeof(TMP_InputField)) return "input";
            if (componentType == typeof(Dropdown) || componentType == typeof(TMP_Dropdown)) return "dropdown";
            if (componentType == typeof(ScrollRect)) return "scrollRect";
            if (componentType == typeof(LayoutGroup)) return "layout";
            if (componentType == typeof(Mask)) return "mask";

            return string.Empty;
        }

        private string GetTypeAlias(Type type)
        {
            if (TypeAliases.TryGetValue(type, out string alias))
            {
                return alias;
            }
            return type.Name;
        }

        private string GenerateUICode(bool isOnlyGenerateViewScript, UILayerType uILayerType)
        {
            string className = string.Empty;
            if (selectedPrefab == null)
            {
                EditorUtility.DisplayDialog("错误", "请先选择UI预制体", "确定");
                return className;
            }

            try
            {
                if (!Directory.Exists(scriptOutputPath))
                {
                    Directory.CreateDirectory(scriptOutputPath);
                }
                var uiFields = AnalyzeUIPrefab(selectedPrefab);
                string scriptContent = GenerateViewScriptContent(selectedPrefab.name, uiFields, uILayerType, isOnlyGenerateViewScript);
                className = GetViewClassName(selectedPrefab.name);
                string fileName = $"{className}.cs";
                string filePath = Path.Combine(scriptOutputPath, fileName);
                if (File.Exists(filePath))
                {
                    if (!EditorUtility.DisplayDialog("确认覆盖", $"文件 {fileName} 已存在，是否覆盖？", "覆盖", "取消"))
                    {
                        return className;
                    }
                }
                File.WriteAllText(filePath, scriptContent, Encoding.UTF8);
                AssetDatabase.Refresh();
                EditorUtility.FocusProjectWindow();
                UnityEngine.Object script = AssetDatabase.LoadAssetAtPath<MonoScript>(filePath);
                if (script != null)
                {
                    Selection.activeObject = script;
                }

                Debug.Log($"UI代码生成成功：{filePath}");
                if (isOnlyGenerateViewScript)
                {
                    EditorUtility.DisplayDialog("成功", $"UI代码已生成到：{filePath}", "确定");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"生成UI代码失败：{e.Message}\n{e.StackTrace}");
                EditorUtility.DisplayDialog("错误", $"生成UI代码失败：{e.Message}", "确定");
            }
            return className;
        }

        private string GenerateUIControlCode()
        {
            string className = string.Empty;
            if (selectedPrefab == null)
            {
                EditorUtility.DisplayDialog("错误", "请先选择UI预制体", "确定");
                return className;
            }

            try
            {
                if (!Directory.Exists(scriptOutputPath))
                {
                    Directory.CreateDirectory(scriptOutputPath);
                }

                var uiFields = AnalyzeUIPrefab(selectedPrefab);
                string scriptContent = GenerateControlScriptContent(selectedPrefab.name);
                className = GetControlClassName(selectedPrefab.name);
                string fileName = $"{className}.cs";
                string filePath = Path.Combine(scriptOutputPath, fileName);

                if (File.Exists(filePath))
                {
                    if (!EditorUtility.DisplayDialog("确认覆盖", $"文件 {fileName} 已存在，是否覆盖？", "覆盖", "取消"))
                    {
                        return className;
                    }
                }
                File.WriteAllText(filePath, scriptContent, Encoding.UTF8);
                AssetDatabase.Refresh();
                EditorUtility.FocusProjectWindow();
                UnityEngine.Object script = AssetDatabase.LoadAssetAtPath<MonoScript>(filePath);
                if (script != null)
                {
                    Selection.activeObject = script;
                }

                Debug.Log($"UIControl代码生成成功：{filePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"生成UIControl代码失败：{e.Message}\n{e.StackTrace}");
                EditorUtility.DisplayDialog("错误", $"生成UIControl代码失败：{e.Message}", "确定");
            }
            return className;
        }

        private string GenerateUIModelCode()
        {
            string className = string.Empty;
            if (selectedPrefab == null)
            {
                EditorUtility.DisplayDialog("错误", "请先选择UI预制体", "确定");
                return className;
            }

            try
            {
                // 确保输出目录存在
                if (!Directory.Exists(scriptOutputPath))
                {
                    Directory.CreateDirectory(scriptOutputPath);
                }

                // 分析预制体
                var uiFields = AnalyzeUIPrefab(selectedPrefab);

                // 生成脚本
                string scriptContent = GenerateModelScriptContent(selectedPrefab.name);

                // 生成文件名
                className = GetModelClassName(selectedPrefab.name);
                string fileName = $"{className}.cs";
                string filePath = Path.Combine(scriptOutputPath, fileName);

                // 检查文件是否已存在
                if (File.Exists(filePath))
                {
                    if (!EditorUtility.DisplayDialog("确认覆盖", $"文件 {fileName} 已存在，是否覆盖？", "覆盖", "取消"))
                    {
                        return className;
                    }
                }

                // 写入文件
                File.WriteAllText(filePath, scriptContent, Encoding.UTF8);

                // 刷新AssetDatabase
                AssetDatabase.Refresh();

                // 选择生成的脚本
                EditorUtility.FocusProjectWindow();
                UnityEngine.Object script = AssetDatabase.LoadAssetAtPath<MonoScript>(filePath);
                if (script != null)
                {
                    Selection.activeObject = script;
                }

                Debug.Log($"UIModel代码生成成功：{filePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"生成UIModel代码失败：{e.Message}\n{e.StackTrace}");
                EditorUtility.DisplayDialog("错误", $"生成UIModel代码失败：{e.Message}", "确定");
            }
            return className;
        }

        private void GenerateUIMVCCode(UILayerType uILayerType)
        {
            // 获取项目中所有UI预制体
            string classNameView = GenerateUICode(false, uILayerType);
            string classNameControl = GenerateUIControlCode();
            string classNameModel = GenerateUIModelCode();

            // 刷新AssetDatabase
            AssetDatabase.Refresh();

            // 显示结果
            StringBuilder result = new StringBuilder();
            result.AppendLine($"批量生成完成！");
            result.AppendLine($"- {classNameView}");
            result.AppendLine($"- {classNameControl}");
            result.AppendLine($"- {classNameModel}");
            Debug.Log(result.ToString());
            EditorUtility.DisplayDialog("完成", result.ToString(), "确定");
        }

        private string GetViewClassName(string prefabName)
        {
            // 从预制体名提取类名
            string name = prefabName;
            if (name.StartsWith("UIPanel"))
            {
                name = name[7..];
            }
            else if (name.StartsWith("UiPanel"))
            {
                name = name[7..];
            }
            return "UIPanel" + name;
        }

        private string GetControlClassName(string prefabName)
        {
            string name = prefabName;
            if (name.StartsWith("UIPanel"))
            {
                name = name[7..];
            }
            else if (name.StartsWith("UiPanel"))
            {
                name = name[7..];
            }
            return "UIControl" + name;
        }

        private string GetModelClassName(string prefabName)
        {
            string name = prefabName;
            if (name.StartsWith("UIPanel"))
            {
                name = name[7..];
            }
            else if (name.StartsWith("UiPanel"))
            {
                name = name[7..];
            }
            return "UIModel" + name;
        }

        private class UIFieldInfo
        {
            public string FieldName { get; set; }
            public string FieldType { get; set; }
            public string NodePath { get; set; }
            public Type ComponentType { get; set; }
        }
    }
}