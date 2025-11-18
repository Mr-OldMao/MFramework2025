using System.Threading.Tasks;

namespace MFramework.Runtime
{
    public interface IUIController
    {
        IUIView View { get;}
        IUIModel Model { get; }

        Task Initialize(IUIView view, IUIModel model);
        Task Show(object showBeforeData = null, object showAfterData = null);
        Task Hide(object hideAfterData = null, object hideBoforeData = null);
        void OnDestory();
    }
}
