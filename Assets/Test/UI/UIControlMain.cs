using MFramework.Runtime;
using System.Threading.Tasks;

namespace GameMain
{
    public class UIControlMain : UIControllerBase
    {
        public override async Task Initialize(IUIView view, IUIModel model)
        {
            Debugger.Log($"{this.GetType()}, init start  ");
            await base.Initialize(view, model);

            Debugger.Log($"{this.GetType()}, init end  ");
        }

        protected override Task OnShowBefore(object data)
        {
            Debugger.Log($"{this.GetType().Name}, OnShowBefore ,data: {data}");
            return base.OnShowBefore(data);
        }

        protected override Task OnHideBefore(object data)
        {
            Debugger.Log($"{this.GetType().Name}, OnHideBefore ,data: {data}");
            return base.OnHideBefore(data);
        }

        protected override void OnShow(object data)
        {
            View.ShowPanel(Model);


            GameEntry.Event.RegisterEvent(GameEventType.TestUIEvent, () =>
            {
                View.RefreshUI(Model);
            });

        }

        protected override void OnHide(object data)
        {
            Debugger.Log($"{this.GetType().Name},OnHide , data: {data}");
            View.HidePanel(Model);

            GameEntry.Event.UnRegisterEvent(GameEventType.TestUIEvent);
        }
    }
}
