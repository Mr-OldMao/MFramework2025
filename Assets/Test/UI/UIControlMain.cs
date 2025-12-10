using MFramework.Runtime;
using Cysharp.Threading.Tasks;

namespace GameMain
{
    public class UIControlMain : UIControllerBase
    {
        public override async UniTask Init(IUIView view, IUIModel model)
        {
            Debugger.Log($"{this.GetType()}, init start  ");
            await base.Init(view, model);

            Debugger.Log($"{this.GetType()}, init end  ");
        }

        public void SetTitleData(string titleDes)
        {
            (Model as UIModelMain).UpdateTitle(titleDes);
        }
    }
}
