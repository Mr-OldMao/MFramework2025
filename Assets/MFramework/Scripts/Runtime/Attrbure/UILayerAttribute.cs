using System;

namespace MFramework.Runtime
{
    [AttributeUsage(AttributeTargets.Class)]
    public class UILayerAttribute : Attribute
    {
        public UILayerType Layer { get; }
        public UILayerAttribute(UILayerType layer)
        {
            Layer = layer;
        }
    }
}
