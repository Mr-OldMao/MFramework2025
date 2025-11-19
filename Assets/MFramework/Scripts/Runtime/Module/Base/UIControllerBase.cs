using System.Threading.Tasks;

namespace MFramework.Runtime
{
    public abstract class UIControllerBase : IUIController
    {
        public IUIView View { get; private set; }
        public IUIModel Model { get; private set; }

        public virtual async Task Init(IUIView view, IUIModel model)
        {
            View = view;
            Model = model;
            await Model?.Init();
            await View?.Init();
        }

        public virtual void OnDestory() { }

        public virtual void Shutdown() { }
    }
}
