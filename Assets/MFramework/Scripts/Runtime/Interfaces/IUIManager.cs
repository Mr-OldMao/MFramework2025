using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFramework.Runtime
{
    public interface IUIManager : IGameModule
    {
        void ShowPanel(string panelName);
        void HidePanel(string panelName);
        T GetPanel<T>(string panelName) where T : UnityEngine.Component;
    }

}