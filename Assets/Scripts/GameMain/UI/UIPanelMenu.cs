using Cysharp.Threading.Tasks;
using MFramework.Runtime;
using MiniGameSDK;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameMain
{
    [UIBind(typeof(UIControlMenu), typeof(UIModelMenu))]
    [UILayer(UILayerType.Tips)]
    public class UIPanelMenu : UIViewBase
    {
        // UI字段
        private RectTransform rootNode;
        private Image imgBg;
        private Button btnGameStart;
        private Button btnSidebar;
        private RectTransform rectSidebar;
        private TextMeshProUGUI txtTopScore;
        private Button btnRankList;

        private RectTransform rectRankList;
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

            //int topScore = GameMainLogic.Instance.GetUserDataBase().topScore;
            //txtTopScore.text = $"HI- {topScore}";
#if SDK_DY
            txtTopScore.gameObject.SetActive(true);
            SDKManager.GetSpecial<DouyinSDK>().GetRankData((int topSelfScore) =>
            {
                txtTopScore.text = $"HI- {topSelfScore}";
            });
#else
            txtTopScore.gameObject.SetActive(false);
#endif


            if (GameMainLogic.Instance.GameStateType == GameStateType.GameSettlement)
            {
                ShowGameStartBtn(false);
                await GameEntry.UI.GetController<UIControlMap>().GenerateMapFirstStage();
                ShowGameStartBtn(true);
            }
            else
            {
                ShowGameStartBtn(true);
            }
            //return UniTask.CompletedTask;

        }


        public override UniTask HidePanel()
        {
            base.HidePanel();
            return UniTask.CompletedTask;
        }

        protected override void RegisterEvent()
        {
            btnGameStart.onClick.AddListener(async () =>
            {
                HidePanel();

                var UIPanelLoad = await GameEntry.UI.ShowViewAsync<UIPanelLoad>();
                UIPanelLoad.ShowLoadStage(() =>
                {
                    GameMainLogic.Instance.GameStateType = GameStateType.GameStart;
                });
            });

            btnSidebar.onClick.AddListener(() =>
            {
                Debug.Log("btnSidebar");
                GameEntry.UI.ShowView<UIPanelSidebar>();
            });

#if SDK_DY
            rectRankList.gameObject.SetActive(true);
            rectSidebar.gameObject.SetActive(true);
            btnRankList.onClick.AddListener(() =>
            {
                SDKManager.GetSpecial<DouyinSDK>().ShowRankList();
            });
#else
            rectRankList.gameObject.SetActive(false);
            rectSidebar.gameObject.SetActive(false);
#endif


        }

        public class NavigateToSceneInfo
        {
            public string scene = "sidebar";
        }

        protected override void UnRegisterEvent()
        {
            btnGameStart.onClick.RemoveAllListeners();
        }
        


        public void ShowGameStartBtn(bool isShow)
        {
            btnGameStart.gameObject.SetActive(isShow);
        }
    }
}
