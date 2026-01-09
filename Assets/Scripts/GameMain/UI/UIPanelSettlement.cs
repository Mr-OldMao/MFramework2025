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

        private GameStateType CurGameStateType;

        public override async UniTask Init()
        {
            await base.Init();
        }

        public override async void RefreshUI(IUIModel model = null)
        {
            UIModelSettlement _model = Controller.Model as UIModelSettlement;

            btnClose.gameObject.SetActive(false);

            //TODO
            //P1
            var dicplayer1KillData = _model.GetDicPlayer1KillData();
            int p1KillCountEnemy1 = dicplayer1KillData[201].KillCount + dicplayer1KillData[301].KillCount;
            int p1KillCountEnemy2 = dicplayer1KillData[202].KillCount + dicplayer1KillData[302].KillCount;
            int p1KillCountEnemy3 = 0;
            int p1KillCountEnemy4 = dicplayer1KillData[203].KillCount + dicplayer1KillData[204].KillCount
                + dicplayer1KillData[205].KillCount + dicplayer1KillData[303].KillCount;
            int p1KillTotalCount = p1KillCountEnemy1 + p1KillCountEnemy2 + p1KillCountEnemy3 + p1KillCountEnemy4;

            txtP1KillTankCount1.text = string.Empty;
            txtP1KillTankCount2.text = string.Empty;
            txtP1KillTankCount3.text = string.Empty;
            txtP1KillTankCount4.text = string.Empty;
            txtP1Score1.text = string.Empty;
            txtP1Score2.text = string.Empty;
            txtP1Score3.text = string.Empty;
            txtP1Score4.text = string.Empty;
            txtP1KillTotalCount.text = string.Empty;

            txtTotalScore.text = $"{p1KillTotalCount}";
            int score1 = await DalayShowTxt(p1KillCountEnemy1, DataTools.GetTankEnemy(201).Score, txtP1KillTankCount1, txtP1Score1);
            await UniTask.Delay(500);
            int score2 = await DalayShowTxt(p1KillCountEnemy2, DataTools.GetTankEnemy(202).Score, txtP1KillTankCount2, txtP1Score2);
            await UniTask.Delay(500);
            int score3 = await DalayShowTxt(p1KillCountEnemy3, DataTools.GetTankEnemy(201).Score, txtP1KillTankCount3, txtP1Score3);
            await UniTask.Delay(500);
            int score4 = await DalayShowTxt(p1KillCountEnemy4, DataTools.GetTankEnemy(203).Score, txtP1KillTankCount4, txtP1Score4);
            await UniTask.Delay(500);
            txtP1KillTotalCount.text = $"{(score1 + score2 + score3 + score4)}";

            if (GameMainLogic.Instance.GamePlayerType == GamePlayerType.Multi)
            {
                //P2
                var dicplayer2KillData = _model.GetDicPlayer1KillData();
                int p2KillCountEnemy1 = dicplayer2KillData[201].KillCount + dicplayer2KillData[301].KillCount;
                int p2KillCountEnemy2 = dicplayer2KillData[202].KillCount + dicplayer2KillData[302].KillCount;
                int p2KillCountEnemy3 = 0;
                int p2KillCountEnemy4 = dicplayer2KillData[203].KillCount + dicplayer2KillData[204].KillCount
                    + dicplayer2KillData[205].KillCount + dicplayer2KillData[303].KillCount;
                int p2KillTotalCount = p1KillCountEnemy1 + p1KillCountEnemy2 + p1KillCountEnemy3 + p1KillCountEnemy4;

                txtP2KillTankCount1.text = string.Empty;
                txtP2KillTankCount2.text = string.Empty;
                txtP2KillTankCount3.text = string.Empty;
                txtP2KillTankCount4.text = string.Empty;
                txtP2Score1.text = string.Empty;
                txtP2Score2.text = string.Empty;
                txtP2Score3.text = string.Empty;
                txtP2Score4.text = string.Empty;
                txtP2KillTotalCount.text = string.Empty;

                txtTotalScore.text = $"{p2KillTotalCount}";
                 score1 = await DalayShowTxt(p2KillCountEnemy1, DataTools.GetTankEnemy(201).Score, txtP2KillTankCount1, txtP2Score1);
                await UniTask.Delay(500);
                 score2 = await DalayShowTxt(p2KillCountEnemy2, DataTools.GetTankEnemy(202).Score, txtP2KillTankCount2, txtP2Score2);
                await UniTask.Delay(500);
                 score3 = await DalayShowTxt(p2KillCountEnemy3, DataTools.GetTankEnemy(201).Score, txtP2KillTankCount3, txtP2Score3);
                await UniTask.Delay(500);
                 score4 = await DalayShowTxt(p2KillCountEnemy4, DataTools.GetTankEnemy(203).Score, txtP2KillTankCount4, txtP2Score4);
                await UniTask.Delay(500);
                txtP2KillTotalCount.text = $"{(score1 + score2 + score3 + score4)}";
            }

            btnClose.gameObject.SetActive(true);
        }

        public override async UniTask ShowPanel()
        {
            base.ShowPanel();
            CurGameStateType = GameMainLogic.Instance.GameStateType;
            GameMainLogic.Instance.GameStateType = GameStateType.GameSettlement;

            await RefreshUILayoutAsync();

            (Controller as UIControlSettlement).ResetScore();
            //return UniTask.CompletedTask;
        }

        private async UniTask ProcessNextStage()
        {
            switch (CurGameStateType)
            {
                case GameStateType.Unstart:
                case GameStateType.GameStart:
                case GameStateType.GameRunning:
                case GameStateType.GamePause:
                case GameStateType.GameSettlement:
                    Debugger.LogError("游戏状态异常，gameStateType：" + CurGameStateType);
                    break;
                case GameStateType.GameWin:
                    Debugger.LogError("即将进入加载页下一关卡");
                    //await UniTask.Delay(2000);
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
                    //await UniTask.Delay(3000);
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
            btnClose.onClick.AddListener(async () =>
            {
                await ProcessNextStage();

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


        private async UniTask<int> DalayShowTxt(int num, int score, TextMeshProUGUI txtCount, TextMeshProUGUI txtScore,int delayFrame = 300)
        {
            if (num == 0)
            {
                txtCount.text = $"0";
                txtScore.text = $"0";
                await UniTask.Delay(delayFrame);
            }
            else
            {
                for (int i = 0; i < num; i++)
                {
                    txtCount.text = $"{i + 1}";
                    txtScore.text = $"{(i + 1) * score}";
                    await UniTask.Delay(delayFrame);
                }
            }
            return num * score;
        }
    }
}
