using MFramework.Runtime;
using Cysharp.Threading.Tasks;

namespace GameMain
{
    public class UIModelMain : UIModelBase
    {
        public string Title { get; private set; }

        public UIModelMain(IUIController controller) : base(controller)
        {

        }

        public override async UniTask Init()
        {
            //模拟异步读表读数据
            await UniTask.Delay(200);
            Title = "UIModelMain_Init()";
        }

        public void UpdateTitle(string title)
        {
            Title = title;
            GameEntry.Event.DispatchEvent(GameEventType.TestUIEvent);
        }
    }
}
