using System;

namespace MFramework.Runtime
{
    [AttributeUsage(AttributeTargets.Class)]
    public class UIHideAttribute : Attribute
    {
        public UIHideType hideType { get; }
        public UIHideAttribute(UIHideType hideType)
        {
            this.hideType = hideType;
        }
    }
}
