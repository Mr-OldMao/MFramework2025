using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static MFramework.Runtime.UIBase;

namespace MFramework.Runtime
{
    public interface IUIView : IGameModule
    {
        string ViewName { get; }
        UILayerType Layer { get; }
        bool IsActive { get; }

        void Show(object data = null);
        void Hide();
        void Close();
    }
}
