using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MFramework.Runtime.UIViewBase;

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
