using GameMain;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace MFramework.Runtime
{
    public interface IUIView : IGameBase
    {
        GameObject UIForm { get; }
        UILayerType Layer { get; }
        IUIController Controller { get; set; }
        bool IsActive { get; }

        UniTask ShowPanel();
        UniTask HidePanel();
        void RefreshUI(IUIModel uIModel);

        void SetSprite(Image img, EAtlasType atlasType, string spriteName, Action<Sprite> callback = null);
        UniTask<Sprite> SetSpriteAsync(Image img, EAtlasType atlasType, string spriteName);
    }
}
