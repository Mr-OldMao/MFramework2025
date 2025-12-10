using Cysharp.Threading.Tasks;

namespace MFramework.Runtime
{
    public abstract class UIModelBase : IUIModel
    {
        public abstract UniTask Init();
        public virtual void Reset() { }

        public IUIController Controller { get; set; }
        protected UIModelBase(IUIController controller)
        {
            Controller = controller;
        }
        public virtual void Shutdown() { }
    }
}
