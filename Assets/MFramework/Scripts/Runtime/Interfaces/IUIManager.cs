using System.Threading.Tasks;

namespace MFramework.Runtime
{
    public interface IUIManager : IGameBase
    {
        void ShowView<T>() where T : UIViewBase;

        void HideView<T>() where T : UIViewBase;

        Task<T> ShowViewAsync<T>() where T : UIViewBase;

        Task HideViewAsync<T>() where T : UIViewBase;

        void RemoveView<T>() where T : UIViewBase;
        void RemoveView(IUIView view);
        void RemoveAllView();

        T GetView<T>() where T : UIViewBase;

        T GetModel<T>() where T : UIModelBase;

        T GetController<T>() where T : UIControllerBase;
    }

}