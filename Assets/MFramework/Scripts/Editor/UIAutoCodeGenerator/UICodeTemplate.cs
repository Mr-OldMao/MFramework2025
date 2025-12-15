using MFramework.Runtime;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;

namespace MFramework.Editor
{
    public partial class UIAutoCodeGenerator
    {
        private string GenerateViewScriptContent(string prefabName, List<UIFieldInfo> uiFields, UILayerType uILayerType, bool isOnlyGenerateViewScript)
        {
            string className = GetViewClassName(prefabName);
            string controlClassName = GetControlClassName(prefabName);
            string modelClassName = GetModelClassName(prefabName);

            StringBuilder sb = new StringBuilder();

            // 1. 命名空间
            sb.AppendLine("using MFramework.Runtime;");
            sb.AppendLine("using Cysharp.Threading.Tasks;");
            sb.AppendLine("using TMPro;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using UnityEngine.UI;");
            sb.AppendLine();

            // 2. 命名空间和类定义
            sb.AppendLine($"namespace {namespaceName}");
            sb.AppendLine("{");
            if (isOnlyGenerateViewScript)
            {
                sb.AppendLine($"    //[UIBind(typeof({controlClassName}), typeof({modelClassName}))]");
            }
            else
            {
                sb.AppendLine($"    [UIBind(typeof({controlClassName}), typeof({modelClassName}))]");
            }

            sb.AppendLine($"    [UILayer(UILayerType.{uILayerType})]");

            string partialKeyword = generatePartialClass ? " partial" : "";
            sb.AppendLine($"    public {partialKeyword} class {className} : UIViewBase");
            sb.AppendLine("    {");

            // 3. UI字段
            if (uiFields.Count > 0)
            {
                sb.AppendLine("        // UI字段");
                foreach (var field in uiFields)
                {
                    sb.AppendLine($"        public {field.FieldType} {field.FieldName};");
                }
                sb.AppendLine();
            }

            // 4. 初始化方法
            sb.AppendLine("        public override async UniTask Init()");
            sb.AppendLine("        {");
            sb.AppendLine("            await base.Init();");
            sb.AppendLine("        }");
            sb.AppendLine();

            // 5. 刷新UI方法
            sb.AppendLine("        public override void RefreshUI(IUIModel model = null)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (model is not null)");
            sb.AppendLine("            {");
            sb.AppendLine($"                ");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();

            // 6. 显示/隐藏面板方法
            sb.AppendLine("        public override UniTask ShowPanel()");
            sb.AppendLine("        {");
            sb.AppendLine("            return UniTask.CompletedTask;");
            sb.AppendLine("        }");
            sb.AppendLine();

            sb.AppendLine("        public override UniTask HidePanel()");
            sb.AppendLine("        {");
            sb.AppendLine("            return UniTask.CompletedTask;");
            sb.AppendLine("        }");
            sb.AppendLine();

            // 7. 事件注册/注销方法
            sb.AppendLine("        protected override void RegisterEvent()");
            sb.AppendLine("        {");
            if (uiFields.Any(f => f.ComponentType == typeof(Button)))
            {
                sb.AppendLine("            // btnClose.onClick.AddListener(OnCloseClick);");
            }
            else
            {
                sb.AppendLine("            ");
            }
            sb.AppendLine("        }");
            sb.AppendLine();

            sb.AppendLine("        protected override void UnRegisterEvent()");
            sb.AppendLine("        {");
            if (uiFields.Any(f => f.ComponentType == typeof(Button)))
            {
                sb.AppendLine("            // btnClose.onClick.RemoveListener(OnCloseClick);");
            }
            else
            {
                sb.AppendLine("            ");
            }
            sb.AppendLine("        }");
            sb.AppendLine();

            // 8. 私有方法区域
            if (uiFields.Any(f => f.ComponentType == typeof(Button)))
            {
                sb.AppendLine("        private void OnCloseClick()");
                sb.AppendLine("        {");
                sb.AppendLine("            ");
                sb.AppendLine("        }");
                sb.AppendLine();
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private string GenerateControlScriptContent(string prefabName)
        {
            string className = GetControlClassName(prefabName);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("using MFramework.Runtime;");
            sb.AppendLine("using Cysharp.Threading.Tasks;");
            sb.AppendLine();

            sb.AppendLine($"namespace {namespaceName}");
            sb.AppendLine("{");

            string partialKeyword = generatePartialClass ? " partial" : "";
            sb.AppendLine($"    public {partialKeyword} class {className} : UIControllerBase");
            sb.AppendLine("    {");

            sb.AppendLine("        public override async UniTask Init(IUIView view, IUIModel model)");
            sb.AppendLine("        {");
            sb.AppendLine("            await base.Init(view, model);");
            sb.AppendLine("        }");
            sb.AppendLine();

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private string GenerateModelScriptContent(string prefabName)
        {
            string className = GetModelClassName(prefabName);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("using MFramework.Runtime;");
            sb.AppendLine("using Cysharp.Threading.Tasks;");
            sb.AppendLine();

            sb.AppendLine($"namespace {namespaceName}");
            sb.AppendLine("{");

            string partialKeyword = generatePartialClass ? " partial" : "";
            sb.AppendLine($"    public {partialKeyword} class {className} : UIModelBase");
            sb.AppendLine("    {");

            sb.AppendLine($"        public {className}(IUIController controller) : base(controller)");
            sb.AppendLine("        {");
            sb.AppendLine("            ");
            sb.AppendLine("        }");

            sb.AppendLine("        public override async UniTask Init()");
            sb.AppendLine("        {");
            sb.AppendLine("             await UniTask.CompletedTask;");
            sb.AppendLine("        }");
            sb.AppendLine();

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}
