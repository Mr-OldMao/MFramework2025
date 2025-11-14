using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace MFramework.Runtime
{
    public interface IUIManager : IGameModule
    {
        Task<T> OpenView<T>(object data = null) where T : UIBase;
        void CloseView<T>() where T : UIBase;
        void CloseView(IUIView view);
        T GetView<T>() where T : UIBase;
        void CloseAll();
    }

}