using Cysharp.Threading.Tasks;

namespace MFramework.Runtime
{
    public interface IUIManager : IGameBase
    {
        void ShowView<T>() where T : UIViewBase;

        void HideView<T>() where T : UIViewBase;

        UniTask<T> ShowViewAsync<T>() where T : UIViewBase;

        UniTask HideViewAsync<T>() where T : UIViewBase;

        void RemoveView<T>() where T : UIViewBase;
        void RemoveView(IUIView view);
        void RemoveAllView();

        T GetView<T>() where T : UIViewBase;

        T GetModel<T>() where T : UIModelBase;

        T GetController<T>() where T : UIControllerBase;
    }

}