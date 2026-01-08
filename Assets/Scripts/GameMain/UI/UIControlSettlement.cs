using MFramework.Runtime;
using Cysharp.Threading.Tasks;

namespace GameMain
{
    public  class UIControlSettlement : UIControllerBase
    {
        public override async UniTask Init(IUIView view, IUIModel model)
        {
            await base.Init(view, model);
        }


        public void ResetScore()
        {
            (Model as UIModelSettlement).ResetScore();
        }
    }
}
