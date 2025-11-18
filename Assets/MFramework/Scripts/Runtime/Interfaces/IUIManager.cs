using System.Threading.Tasks;

namespace MFramework.Runtime
{
    public interface IUIManager : IGameModule
    {
        void ShowView<T>(object showData = null, object showBeforeData = null) where T : UIViewBase;

        void HideView<T>(object hideData = null, object hideBeforeData = null) where T : UIViewBase;

        Task<T> ShowViewAsync<T>(object showData = null, object showBeforeData = null) where T : UIViewBase;

        Task HideViewAsync<T>(object hideData = null, object hideBeforeData = null) where T : UIViewBase;

        void Clear<T>() where T : UIViewBase;
        void Clear(IUIView view);
        void ClearAll();

        T GetView<T>() where T : UIViewBase;

        T GetModel<T>() where T : UIModelBase;

        T GetController<T>() where T : UIControllerBase;
    }

}