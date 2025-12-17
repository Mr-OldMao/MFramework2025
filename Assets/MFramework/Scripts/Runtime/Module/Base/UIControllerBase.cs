using Cysharp.Threading.Tasks;
using Unity.VisualScripting;

namespace MFramework.Runtime
{
    public abstract class UIControllerBase : IUIController
    {
        public IUIView View { get; private set; }
        public IUIModel Model { get; private set; }

        public virtual async UniTask Init(IUIView view, IUIModel model)
        {
            View = view;
            Model = model;
            if (!Model.IsUnityNull())
            {
                await Model.Init();
            }
            if (!View.IsUnityNull())
            {
                await View.Init();
            }
        }

        public virtual void OnDestory() { }

        public virtual void Shutdown() { }
    }
}
