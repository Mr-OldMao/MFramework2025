using MFramework.Runtime;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameMain
{
    [UIBind(typeof(UIControlLoad), typeof(UIModelLoad))]
    [UILayer(UILayerType.Tips)]
    public class UIPanelLoad : UIViewBase
    {
        // UI字段
        public RectTransform rootNode;
        public Image imgBg;
        public RectTransform txtLoading;
        public Slider sdrLoading;
        public Image Background;
        public Image Fill;
        public Image Handle;

        public override async UniTask Init()
        {
            await base.Init();
            sdrLoading.value = 0;
        }

        public override void RefreshUI(IUIModel model = null)
        {
            UIModelLoad modelLoad = model as UIModelLoad;
            if (model is not null)
            {
                modelLoad = model as UIModelLoad;
            }
            else
            {
                modelLoad = Controller.Model as UIModelLoad;
            }
            sdrLoading.value = modelLoad.LoadingProgress;
        }

        public override UniTask ShowPanel()
        {
            base.ShowPanel();
            return UniTask.CompletedTask;
        }

        public override UniTask HidePanel()
        {
            base.HidePanel();
            return UniTask.CompletedTask;
        }

        protected override void RegisterEvent()
        {
            GameEntry.Event.RegisterEvent(GameEventType.LoadingProgress, () =>
                {
                    RefreshUI();
                });
        }

        protected override void UnRegisterEvent()
        {
            GameEntry.Event.UnRegisterEvent(GameEventType.LoadingProgress);
        }

    }
}
