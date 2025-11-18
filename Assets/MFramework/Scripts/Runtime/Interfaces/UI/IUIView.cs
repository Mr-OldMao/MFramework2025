using System.Threading.Tasks;
using UnityEngine;
using static MFramework.Runtime.UIViewBase;

namespace MFramework.Runtime
{
    public interface IUIView : IGameModule
    {
        GameObject UIForm { get; }

        UILayerType Layer { get; }

        IUIController Controller { get; set; }
        bool IsActive { get; }

        void OnDestory();

        void ShowPanel(IUIModel uIModel);

        void HidePanel(IUIModel uIModel);

        void RefreshUI(IUIModel uIModel);
    }
}
