using MFramework.Runtime;
using Cysharp.Threading.Tasks;

namespace GameMain
{
    public  class UIModelSettlement : UIModelBase
    {
        public UIModelSettlement(IUIController controller) : base(controller)
        {
            
        }
        public override async UniTask Init()
        {
             await UniTask.CompletedTask;
        }

    }
}
