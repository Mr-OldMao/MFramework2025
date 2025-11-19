using GameMain;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static MFramework.Runtime.UIViewBase;

namespace MFramework.Runtime
{
    public interface IUIView : IGameModule
    {
        GameObject UIForm { get; }
        UILayerType Layer { get; }
        IUIController Controller { get; set; }
        bool IsActive { get; }

        void ShowPanel(IUIModel uIModel = null);
        void HidePanel(IUIModel uIModel = null);
        void RefreshUI(IUIModel uIModel = null);

        void SetSprite(Image img, EAtlasType atlasType, string spriteName, Action<Sprite> callback = null);
        Task<Sprite> SetSpriteAsync(Image img, EAtlasType atlasType, string spriteName);

        void OnDestory();
    }
}
