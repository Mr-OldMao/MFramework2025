using MFramework.Runtime;
using Cysharp.Threading.Tasks;

namespace GameMain
{
    public  class UIModelBattle : UIModelBase
    {
        public UIModelBattle(IUIController controller) : base(controller)
        {
            
        }
        public override async UniTask Init()
        {
             await UniTask.CompletedTask;
        }

    }
}
