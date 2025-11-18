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

        protected override Task OnShowAfter(object data)
        {
            Debugger.Log($"{this.GetType().Name}, OnShowBefore ,data: {data}");
            return base.OnShowAfter(data);
        }

        protected override Task OnHideBefore(object data)
        {
            Debugger.Log($"{this.GetType().Name}, OnHideBefore ,data: {data}");
            return base.OnHideBefore(data);
        }

        protected override Task OnShow(object data)
        {
            Debugger.Log($"{this.GetType().Name},OnShow , data: {data}");

            View.ShowPanel(Model);
            if (data != null && data is IUIModel)
            {
                View.RefreshUI(data as IUIModel);
            }
            GameEntry.Event.RegisterEvent(GameEventType.TestUIEvent, () =>
            {
                View.RefreshUI(Model);
            });

            return base.OnShow(data);
        }

        protected override Task OnHide(object data)
        {
            Debugger.Log($"{this.GetType().Name},OnHide , data: {data}");
            View.HidePanel(Model);

            GameEntry.Event.UnRegisterEvent(GameEventType.TestUIEvent);

            return base.OnHide(data);
        }

        public override void OnDestory()
        {
            base.OnDestory();
            OnHide(null);
        }

        public void SetTitleData(string titleDes)
        {
            (Model as UIModelMain).Title = titleDes;
        }
    }
}
