using Cysharp.Threading.Tasks;
using MFramework.Runtime;
using TMPro;
using TTSDK;
using TTSDK.UNBridgeLib.LitJson;
using UnityEngine;
using UnityEngine.UI;

namespace GameMain
{
    [UIBind(typeof(UIControlSidebar), typeof(UIModelSidebar))]
    [UILayer(UILayerType.Tips)]
    public class UIPanelSidebar : UIViewBase
    {
        // UI字段
        private RectTransform rootNode;
        private RectTransform rectSidebarGroup;
        private Button btnClose;
        private RectTransform rectReward;
        private Button btnReceiveReward;
        private Button btnJumpSidebar;
        private TextMeshProUGUI txtTodayReceivedReward;

        private Image imgRewardIcon;
        private TextMeshProUGUI txtRewardDes;


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

        public override async UniTask ShowPanel()
        {
            base.ShowPanel();
            UpdateReceiveRewardState();

            txtRewardDes.text = ((UIControlSidebar)Controller).GetRewardDes();
            await SetSpriteAsync(imgRewardIcon, EAtlasType.reward, ((UIControlSidebar)Controller).fB_Reward_Sidebar.AssetName);

            //return UniTask.CompletedTask;
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
                Debug.Log("领取奖励TODO " + GameMainLogic.Instance.IsCanGetTodayReward);
                if (GameMainLogic.Instance.IsCanGetTodayReward)
                {
                    GameMainLogic.Instance.IsGetTodayReward = true;
                    UpdateReceiveRewardState();
                }
            });

            btnJumpSidebar.onClick.AddListener(() =>
            {
                TTSDKManager.Instance.GuideClickSidebar((isSucc) =>
                {
                    if (isSucc)
                    {
                        GameMainLogic.Instance.SetLastEnterGameDateTime();
                    }
                    UpdateReceiveRewardState();
                });
            });
        }

        protected override void UnRegisterEvent()
        {
            btnClose.onClick.RemoveAllListeners();
            btnReceiveReward.onClick.RemoveAllListeners();
        }


        private void UpdateReceiveRewardState()
        {
            btnJumpSidebar.gameObject.SetActive(!GameMainLogic.Instance.IsCanGetTodayReward);
            btnReceiveReward.gameObject.SetActive(
                GameMainLogic.Instance.IsCanGetTodayReward
                && !GameMainLogic.Instance.IsGetTodayReward);
            txtTodayReceivedReward.gameObject.SetActive(GameMainLogic.Instance.IsGetTodayReward);
        }
    }
}
