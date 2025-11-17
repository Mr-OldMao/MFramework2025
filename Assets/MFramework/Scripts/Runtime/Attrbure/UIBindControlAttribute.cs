using System;

namespace MFramework.Runtime
{
    [AttributeUsage(AttributeTargets.Class)]
    public class UIBindControlAttribute : Attribute
    {
        public UIControllerBase uiControllerBase;
        public UIModelBase uiModelBase;
        public UIBindControlAttribute(Type uiController,Type uiModel)
        {
            if (uiControllerBase == null)
            {
                if (!typeof(UIControllerBase).IsAssignableFrom(uiController))
                {
                    Debugger.LogError("UI控制类绑定失败，请检查是否继承自UIControllerBase");
                    return;
                }
                uiControllerBase = (UIControllerBase)Activator.CreateInstance(uiController);
            }

            if (uiModelBase == null)
            {
                if (!typeof(UIModelBase).IsAssignableFrom(uiModel))
                {
                    Debugger.LogError("UI数据类绑定失败，请检查是否继承自UIModelBase");
                    return;
                }
                uiModelBase = (UIModelBase)Activator.CreateInstance(uiModel);
            }
        }
    }
}
