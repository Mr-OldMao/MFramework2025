using MFramework.Runtime;
using Cysharp.Threading.Tasks;

namespace GameMain
{
    public  class UIModelMap : UIModelBase
    {
        public UIModelMap(IUIController controller) : base(controller)
        {
            
        }
        public override async UniTask Init()
        {
             await UniTask.CompletedTask;
        }

    }
}
