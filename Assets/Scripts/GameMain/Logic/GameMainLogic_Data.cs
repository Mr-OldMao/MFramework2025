
using MFramework.Runtime;
using Unity.VisualScripting;

namespace GameMain
{
    public partial class GameMainLogic
    {
        public PlayerEntity Player1Entity { get; set; }

        /// <summary>
        /// 当前关卡剩余生成敌人数量
        /// </summary>
        public int RemainEnemyTankNum { get; set; }

        public bool IsGameWin { get; private set; } = false;

        public bool IsGameFail { get; private set; } = false;


        public GameStateType GameStateType { get; set; } = GameStateType.Unstart;

        public bool JudgeGameWin()
        {
            IsGameWin = RemainEnemyTankNum == 0 && GameEntry.Pool.GetPool(PoolIdTankEnemy).UsedCount == 0;
            if (IsGameWin)
            {
                Debugger.LogError("游戏结束-胜利");
                GameStateType = GameStateType.GameWin;
                GameEntry.Event.DispatchEvent(GameEventType.GameWin);
            }
            return IsGameWin;
        }

        public bool JudgeGameFail()
        {
            IsGameFail = Player1Entity.remainLife < 0;
            if (IsGameFail)
            {
                Debugger.LogError("游戏结束-失败");
                GameStateType = GameStateType.GameFail;
                GameEntry.Event.DispatchEvent(GameEventType.GameFail);
            }
            return IsGameFail;
        }
    }


    public enum GameStateType
    {
        Unstart,
        GameStart,
        GameRunning,
        GamePause,
        GameWin,
        GameFail,
        GameSettlement,
    }
}
