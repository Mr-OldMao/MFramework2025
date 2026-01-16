using Cysharp.Threading.Tasks;
using MFramework.Runtime;
using TMPro;
using TTSDK;
using TTSDK.UNBridgeLib.LitJson;
using UnityEngine;
using UnityEngine.UI;

namespace GameMain
{
    //[UIBind(typeof(UIControlSidebar), typeof(UIModelSidebar))]
    [UILayer(UILayerType.Tips)]
    public  class UIPanelSidebar : UIViewBase
    {
        // UI字段
        private RectTransform rootNode;
        private RectTransform rectSidebarGroup;
        private Button btnClose;
        private RectTransform rectReward;
        private Button btnReceiveReward;

        private TextMeshProUGUI txtReceiveReward;

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
            UpdateReceiveRewardState(false);

            return UniTask.CompletedTask;
        }

        public override UniTask HidePanel()
        {
            base.HidePanel();
            return UniTask.CompletedTask;
        }

        protected override void RegisterEvent()
        {
            btnClose.onClick.AddListener(() =>
            {
                TTSDKManager.Instance.ShowRevisitGuide();
                HidePanel();
            });

            btnReceiveReward.onClick.AddListener(() =>
            {
                TTSDKManager.Instance.GuideClickSidebar(() =>
                {
                    UpdateReceiveRewardState(true);
                });
            });
        }

        protected override void UnRegisterEvent()
        {
            btnClose.onClick.RemoveAllListeners();
            btnReceiveReward.onClick.RemoveAllListeners();
        }


        private void UpdateReceiveRewardState(bool isReceived)
        {
            txtReceiveReward.text = isReceived ? "已领取奖励" : "跳转侧边栏领取奖励";

            btnReceiveReward.interactable = !isReceived;
        }
    }
}
