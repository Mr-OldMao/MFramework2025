using MFramework.Runtime;
using Cysharp.Threading.Tasks;

namespace GameMain
{
    public  class UIModelLoad : UIModelBase
    {
        public float LoadingProgress { get; private set; } = 0;


        public UIModelLoad(IUIController controller) : base(controller)
        {
            
        }
        public override async UniTask Init()
        {
             await UniTask.CompletedTask;
        }


        public void SetLoadingProgress(float progress)
        {
            LoadingProgress = progress;
            GameEntry.Event.DispatchEvent(GameEventType.LoadingProgress);
        }
    }
}
