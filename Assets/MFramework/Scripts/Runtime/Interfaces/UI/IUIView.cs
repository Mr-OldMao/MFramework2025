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


        Task ShowPanel();
        Task HidePanel();
        void RefreshUI(IUIModel uIModel);

        void SetSprite(Image img, EAtlasType atlasType, string spriteName, Action<Sprite> callback = null);
        Task<Sprite> SetSpriteAsync(Image img, EAtlasType atlasType, string spriteName);

        void OnDestory();
    }
}
