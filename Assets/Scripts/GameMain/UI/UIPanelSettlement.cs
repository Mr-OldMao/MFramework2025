using MFramework.Runtime;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameMain
{
    [UIBind(typeof(UIControlSettlement), typeof(UIModelSettlement))]
    [UILayer(UILayerType.Popup)]
    public class UIPanelSettlement : UIViewBase
    {
        // UI字段
        private RectTransform rootNode;
        private RectTransform rectGroup;
        private RectTransform rectNode1;
        private RectTransform rectNode2;
        private RectTransform rectNode3;
        private RectTransform rectNode4;
        private RectTransform txtTotalScore;
        private RectTransform rectNode5;
        private RectTransform rectNodeKillEnemy1;
        private RectTransform rectP1Enemy1;
        private RectTransform txtP1Score1;
        private RectTransform txtP1KillTankCount1;
        private RectTransform rectP2Enemy1;
        private RectTransform txtP2KillTankCount1;
        private RectTransform txtP2Score1;
        private RectTransform rectNodeKillEnemy2;
        private RectTransform rectP1Enemy2;
        private RectTransform txtP1Score2;
        private RectTransform txtP1KillTankCount2;
        private RectTransform rectP2Enemy2;
        private RectTransform txtP2KillTankCount2;
        private RectTransform txtP2Score2;
        private RectTransform rectNodeKillEnemy3;
        private RectTransform rectP1Enemy3;
        private RectTransform txtP1Score3;
        private RectTransform txtP1KillTankCount3;
        private RectTransform rectP2Enemy3;
        private RectTransform txtP2KillTankCount3;
        private RectTransform txtP2Score3;
        private RectTransform rectNodeKillEnemy4;
        private RectTransform rectP1Enemy4;
        private RectTransform txtP1Score4;
        private RectTransform txtP1KillTankCount4;
        private RectTransform rectP2Enemy4;
        private RectTransform txtP2KillTankCount4;
        private RectTransform txtP2Score4;
        private RectTransform rectNode6;
        private RectTransform rectNode7;
        private RectTransform rectP1KillTotalCount;
        private RectTransform txtP1KillTotalCount;
        private RectTransform rectP2KillTotalCount;
        private RectTransform txtP2KillTotalCount;

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

        public override async UniTask ShowPanel()
        {
            base.ShowPanel();
            GameStateType gameStateType = GameMainLogic.Instance.GameStateType;
            GameMainLogic.Instance.GameStateType = GameStateType.GameSettlement;



            for (int i = 0; i < rectP2Enemy1.childCount; i++)
            {
                rectP2Enemy1.GetChild(i).gameObject.SetActive(GameMainLogic.Instance.GamePlayerType == GamePlayerType.Multi);
                rectP2Enemy2.GetChild(i).gameObject.SetActive(GameMainLogic.Instance.GamePlayerType == GamePlayerType.Multi);
                rectP2Enemy3.GetChild(i).gameObject.SetActive(GameMainLogic.Instance.GamePlayerType == GamePlayerType.Multi);
                rectP2Enemy4.GetChild(i).gameObject.SetActive(GameMainLogic.Instance.GamePlayerType == GamePlayerType.Multi);
            }
            for (int i = 0; i < rectP2KillTotalCount.childCount; i++)
            {
                rectP2KillTotalCount.GetChild(i).gameObject.SetActive(GameMainLogic.Instance.GamePlayerType == GamePlayerType.Multi);
            }
            await UniTask.Delay(1);
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectGroup);

            

            switch (gameStateType)
            {
                case GameStateType.Unstart:
                case GameStateType.GameStart:
                case GameStateType.GameRunning:
                case GameStateType.GamePause:
                case GameStateType.GameSettlement:
                    Debugger.LogError("游戏状态异常，gameStateType：" + gameStateType);
                    break;
                case GameStateType.GameWin:
                    Debugger.LogError("即将进入加载页下一关卡");
                    await UniTask.Delay(2000);
                    GameMainLogic.Instance.Player1Entity.IsExtendBeforeDataNextGenerate = true;
                    await GameEntry.UI.GetController<UIControlMap>().GenerateMapNextStage();
                    var UIPanelLoad = await GameEntry.UI.ShowViewAsync<UIPanelLoad>();
                    HidePanel();
                    UIPanelLoad.ShowLoadStage(() =>
                    {
                        GameMainLogic.Instance.GameStateType = GameStateType.GameStart;
                    });
                    break;
                case GameStateType.GameFail:
                    await UniTask.Delay(3000);
                    Debugger.LogError("游戏结束，结算完毕，准备返回菜单界面");
                    GameEntry.UI.ShowView<UIPanelMenu>(this);
                    GameMainLogic.Instance.Player1Entity.IsInitLife = true;
                    GameMainLogic.Instance.Player1Entity.IsExtendBeforeDataNextGenerate = false;
                    GameMainLogic.Instance.Player1Entity.ResetTankData();
                    break;
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
            btnClose.onClick.AddListener(() =>
            {
                GameEntry.UI.ShowView<UIPanelLoad>();
                HidePanel();
            });
        }

        protected override void UnRegisterEvent()
        {
            btnClose.onClick.RemoveAllListeners();
        }

    }
}
