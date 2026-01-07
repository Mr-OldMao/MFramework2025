using MFramework.Runtime;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

namespace GameMain
{
    //[UIBind(typeof(UIControlGameOver), typeof(UIModelGameOver))]
    [UILayer(UILayerType.Popup)]
    public class UIPanelGameOver : UIViewBase
    {
        // UI字段
        private RectTransform rootNode;
        private RectTransform rectGameOverFull;
        private Image imgBg;
        private RectTransform rectGameOverPop;
        private Button btnClose;

        public override async UniTask Init()
        {
            await base.Init();
        }

        public override void RefreshUI(IUIModel model = null)
        {
            if (model is not null)
            {

            }
        }

        public override UniTask ShowPanel()
        {
            base.ShowPanel();
            rectGameOverPop.gameObject.SetActive(false);
            rectGameOverFull.gameObject.SetActive(false);
            return UniTask.CompletedTask;
        }

        public async void ShowPanelPop()
        {
            rectGameOverPop.gameObject.SetActive(true);
            rectGameOverFull.gameObject.SetActive(false);

            await UniTask.Delay(2000);
            GameEntry.UI.ShowView<UIPanelSettlement>(this);
        }

        public void ShowPanelFull()
        {
            rectGameOverFull.gameObject.SetActive(true);
            rectGameOverPop.gameObject.SetActive(false);
        }

        public override UniTask HidePanel()
        {
            base.HidePanel();
            return UniTask.CompletedTask;
        }

        protected override void RegisterEvent()
        {
            btnClose.onClick.AddListener(async () =>
            {
                await GameEntry.UI.ShowViewAsync<UIPanelLoad>();
                HidePanel();
            });
        }

        protected override void UnRegisterEvent()
        {
            btnClose.onClick.RemoveAllListeners();
        }

    }
}
