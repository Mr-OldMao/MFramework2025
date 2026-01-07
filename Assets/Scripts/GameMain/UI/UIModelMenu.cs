using MFramework.Runtime;
using Cysharp.Threading.Tasks;

namespace GameMain
{
    public  class UIModelMenu : UIModelBase
    {
        public UIModelMenu(IUIController controller) : base(controller)
        {
            
        }
        public override async UniTask Init()
        {
             await UniTask.CompletedTask;
        }


    }
}
