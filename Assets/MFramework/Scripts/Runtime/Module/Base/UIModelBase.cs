using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFramework.Runtime
{
    public abstract class UIModelBase : IUIModel
    {
        public virtual void Initialize() { }
        public virtual void Reset() { }

        // 数据变更事件
        public event Action<string> OnDataChanged;

        protected void NotifyDataChanged(string propertyName = "")
        {
            OnDataChanged?.Invoke(propertyName);
        }
    }
}
