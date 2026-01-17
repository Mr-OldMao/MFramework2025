
using Cysharp.Threading.Tasks;
using MFramework.Runtime;
using System.Threading.Tasks;

namespace GameMain
{
    public partial class GameMainLogic
    {
        public PlayerEntity Player1Entity { get; set; }

        public GamePlayerType GamePlayerType { get; set; } = GamePlayerType.Single;
        /// <summary>
        /// 当前关卡剩余生成敌人数量
        /// </summary>
        public int RemainEnemyTankNum { get; set; }

        public bool IsGameWin { get; private set; } = false;

        public bool IsGameFail { get; private set; } = false;

        private GameStateType m_GameStateType = GameStateType.Unstart;
        public GameStateType GameStateType
        {
            get
            {
                return m_GameStateType;
            }
            set
            {
                GameStateType beforeState = m_GameStateType;
                m_GameStateType = value;
                switch (m_GameStateType)
                {
                    case GameStateType.Unstart:
                        break;
                    case GameStateType.GameMapGenerating:
                        break;
                    case GameStateType.GameMapGenerated:
                        break;
                    case GameStateType.GameStart:
                        GameEntry.Event.DispatchEvent(GameEventType.GameStart);
                        break;
                    case GameStateType.GameRunning:
                        break;
                    case GameStateType.GamePause:
                        GameEntry.Event.DispatchEvent(GameEventType.GamePause);
                        break;
                    case GameStateType.GameWin:
                        GameEntry.Event.DispatchEvent(GameEventType.GameWin);
                        break;
                    case GameStateType.GameFail:
                        if (beforeState != m_GameStateType)
                        {
                            GameEntry.Event.DispatchEvent(GameEventType.GameFail);
                        }
                        break;
                    case GameStateType.GameSettlement:
                        GameEntry.Event.DispatchEvent(GameEventType.GameSettlement);
                        break;
                }
            }
        }

        private void InitRegisterEvent()
        {
            GameEntry.Event.RegisterEvent(GameEventType.GameFail, async () =>
            {
                Debugger.LogError("游戏结束-失败");
                var UIPanelGameOverPanel = await GameEntry.UI.ShowViewAsync<UIPanelGameOver>();
                UIPanelGameOverPanel.ShowPanelPop();
            });

            GameEntry.Event.RegisterEvent(GameEventType.GameWin, () =>
            {
                Debugger.LogError("游戏结束-胜利");
                GameEntry.UI.ShowViewAsync<UIPanelSettlement>();
            });
        }
    }


    public enum GameStateType
    {
        Unstart,
        GameMapGenerating,
        GameMapGenerated,
        GameStart,
        GameRunning,
        GamePause,
        GameWin,
        GameFail,
        GameSettlement,
    }

    public enum GamePlayerType
    {
        Single,
        Multi,
    }
}
