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
        private TextMeshProUGUI txtTotalScore;
        private RectTransform rectNode5;
        private RectTransform rectNodeKillEnemy1;
        private RectTransform rectP1Enemy1;
        private TextMeshProUGUI txtP1Score1;
        private TextMeshProUGUI txtP1KillTankCount1;
        private RectTransform rectP2Enemy1;
        private TextMeshProUGUI txtP2KillTankCount1;
        private TextMeshProUGUI txtP2Score1;
        private RectTransform rectNodeKillEnemy2;
        private RectTransform rectP1Enemy2;
        private TextMeshProUGUI txtP1Score2;
        private TextMeshProUGUI txtP1KillTankCount2;
        private RectTransform rectP2Enemy2;
        private TextMeshProUGUI txtP2KillTankCount2;
        private TextMeshProUGUI txtP2Score2;
        private RectTransform rectNodeKillEnemy3;
        private RectTransform rectP1Enemy3;
        private TextMeshProUGUI txtP1Score3;
        private TextMeshProUGUI txtP1KillTankCount3;
        private RectTransform rectP2Enemy3;
        private TextMeshProUGUI txtP2KillTankCount3;
        private TextMeshProUGUI txtP2Score3;
        private RectTransform rectNodeKillEnemy4;
        private RectTransform rectP1Enemy4;
        private TextMeshProUGUI txtP1Score4;
        private TextMeshProUGUI txtP1KillTankCount4;
        private RectTransform rectP2Enemy4;
        private TextMeshProUGUI txtP2KillTankCount4;
        private TextMeshProUGUI txtP2Score4;
        private RectTransform rectNode6;
        private RectTransform rectNode7;
        private RectTransform rectP1KillTotalCount;
        private TextMeshProUGUI txtP1KillTotalCount;
        private RectTransform rectP2KillTotalCount;
        private TextMeshProUGUI txtP2KillTotalCount;

        private Button btnClose;



        public override async UniTask Init()
        {
            await base.Init();
        }

        public override void RefreshUI(IUIModel model = null)
        {
            UIModelSettlement _model = Controller.Model as UIModelSettlement;
            var dicplayer1KillData = _model.GetDicPlayer1KillData();

            //TODO
            int p1KillCountEnemy1 = dicplayer1KillData[201].KillCount + dicplayer1KillData[301].KillCount;
            int p1KillCountEnemy2 = dicplayer1KillData[202].KillCount + dicplayer1KillData[302].KillCount;
            int p1KillCountEnemy3 = 0;
            int p1KillCountEnemy4 = dicplayer1KillData[203].KillCount + dicplayer1KillData[204].KillCount
                + dicplayer1KillData[205].KillCount + dicplayer1KillData[303].KillCount;
            int p1KillTotalCount = p1KillCountEnemy1 + p1KillCountEnemy2 + p1KillCountEnemy3 + p1KillCountEnemy4;

            int p1KillScoreEnemy1 = dicplayer1KillData[201].KillScore + dicplayer1KillData[301].KillScore;
            int p1KillScoreEnemy2 = dicplayer1KillData[202].KillScore + dicplayer1KillData[302].KillScore;
            int p1KillScoreEnemy3 = 0;
            int p1KillScoreEnemy4 = dicplayer1KillData[203].KillScore + dicplayer1KillData[204].KillScore
                + dicplayer1KillData[205].KillScore + dicplayer1KillData[303].KillScore;
            int p1KillTotalScore = p1KillScoreEnemy1 + p1KillScoreEnemy2 + p1KillScoreEnemy3 + p1KillScoreEnemy4;

            txtP1KillTankCount1.text = $"{p1KillCountEnemy1}";
            txtP1KillTankCount2.text = $"{p1KillCountEnemy2}";
            txtP1KillTankCount3.text = $"{p1KillCountEnemy3}";
            txtP1KillTankCount4.text = $"{p1KillCountEnemy4}";

            txtP1Score1.text = $"{p1KillScoreEnemy1}";
            txtP1Score2.text = $"{p1KillScoreEnemy2}";
            txtP1Score3.text = $"{p1KillScoreEnemy3}";
            txtP1Score4.text = $"{p1KillScoreEnemy4}";


            txtTotalScore.text = $"{p1KillTotalCount}";
            txtP1KillTotalCount.text = $"{p1KillTotalScore}";

        }

        public override async UniTask ShowPanel()
        {
            base.ShowPanel();
            GameStateType gameStateType = GameMainLogic.Instance.GameStateType;
            GameMainLogic.Instance.GameStateType = GameStateType.GameSettlement;

            await RefreshUILayoutAsync();

            await ProcessNextStage(gameStateType);

            (Controller as UIControlSettlement).ResetScore();
            //return UniTask.CompletedTask;
        }

        private async UniTask ProcessNextStage(GameStateType gameStateType)
        {
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

        private async UniTask RefreshUILayoutAsync()
        {
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
        }

    }
}
