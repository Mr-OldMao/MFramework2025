using MFramework.Runtime;
using Cysharp.Threading.Tasks;

namespace GameMain
{
    public class UIModelRevive : UIModelBase
    {
        public GameOverType gameOverType;

        public void SetGameOverType(GameOverType gameOverType)
        {
            this.gameOverType = gameOverType;
        }

        public UIModelRevive(IUIController controller) : base(controller)
        {

        }
        public override async UniTask Init()
        {
            await UniTask.CompletedTask;
        }

    }
    public enum GameOverType
    {
        /// <summary>
        /// 生命值归零
        /// </summary>
        LifeZero,
        /// <summary>
        /// 鸟窝被毁
        /// </summary>
        BridDestroy
    }
}
