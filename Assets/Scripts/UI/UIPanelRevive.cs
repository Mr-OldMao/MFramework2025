using MFramework.Runtime;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MiniGameSDK;

namespace GameMain
{
    [UIBind(typeof(UIControlRevive), typeof(UIModelRevive))]
    [UILayer(UILayerType.Popup)]
    public class UIPanelRevive : UIViewBase
    {
        // UI字段
        private RectTransform rootNode;
        private Image imgMask;
        private Image rectHint;
        private RectTransform txtDes;
        private Button btnRestartCurStage;
        private Button btnRevive;
        private Button btnNotRevive;

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


            GameOverType gameOverType = ((UIModelRevive)Controller.Model).gameOverType;

            btnRevive.gameObject.SetActive(gameOverType == GameOverType.LifeZero);

            await UniTask.Delay(1000);

            Debug.Log("UIPanelRevive ShowPanel");
            GameMainLogic.Instance.GameParse(true, false);
        }

        public override UniTask HidePanel()
        {
            base.HidePanel();

            GameMainLogic.Instance.GameParse(false, false);
            return UniTask.CompletedTask;
        }

        protected override void RegisterEvent()
        {
            btnNotRevive.onClick.AddListener(() =>
            {
                if (GameMainLogic.Instance.GameStateType != GameStateType.GameFail)
                {
                    GameMainLogic.Instance.GameStateType = GameStateType.GameFail;
                }
                HidePanel();
            });

            btnRestartCurStage.onClick.AddListener(() =>
            {
                SDKManager.Instance.ShowAdvReward((isPlayed) =>
                {
                    if (isPlayed)
                    {
                        GameMainLogic.Instance.Player1Entity.PlayerRestartByAdv();
                        GameMainLogic.Instance.IsCurStageReward = true;
                        GameEntry.Event.DispatchEvent(GameEventType.CancelStopAllEnemyMove);
                        GameEntry.UI.GetController<UIControlMap>().RecycleMapEntity();
                        GameEntry.UI.GetController<UIControlMap>().GenerateMapCurrentStage();
                        GameEntry.UI.GetModel<UIModelSettlement>().ResetScore();
                        GameMainLogic.Instance.GameStateType = GameStateType.GameStart;
                        HidePanel();
                    }

                }, (msg) =>
                {
                    Debugger.Log($"btnRestartCurStage loadFailCallback,msg:{msg}");

                    GameMainLogic.Instance.Player1Entity.PlayerRestartByAdv();
                    GameMainLogic.Instance.IsCurStageReward = true;
                    GameEntry.Event.DispatchEvent(GameEventType.CancelStopAllEnemyMove);
                    GameEntry.UI.GetController<UIControlMap>().RecycleMapEntity();
                    GameEntry.UI.GetController<UIControlMap>().GenerateMapCurrentStage();
                    GameEntry.UI.GetModel<UIModelSettlement>().ResetScore();
                    GameMainLogic.Instance.GameStateType = GameStateType.GameStart;
                    HidePanel();
                });
            });

            btnRevive.onClick.AddListener(() =>
            {
                SDKManager.Instance.ShowAdvReward((isPlayed) =>
                {
                    if (isPlayed)
                    {
                        GameMainLogic.Instance.Player1Entity.PlayerReviveByAdv();
                        HidePanel();
                    }
                }, (msg) =>
                {
                    Debugger.Log($"btnRevive loadFailCallback,msg:{msg}");
                    GameMainLogic.Instance.Player1Entity.PlayerReviveByAdv();
                    HidePanel();
                });
            });
        }

        protected override void UnRegisterEvent()
        {
            btnNotRevive.onClick.RemoveAllListeners();
            btnRestartCurStage.onClick.RemoveAllListeners();
            btnRevive.onClick.RemoveAllListeners();
        }
    }
}
