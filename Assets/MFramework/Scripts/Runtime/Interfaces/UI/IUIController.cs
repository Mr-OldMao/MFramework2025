using System.Threading.Tasks;

namespace MFramework.Runtime
{
    public interface IUIController
    {
        IUIView View { get;}
        IUIModel Model { get; }

        Task Initialize(IUIView view, IUIModel model);
        Task Show(object showData = null, object showBeforeData = null);
        Task Hide(object hideData = null, object hideBoforeData = null);
        void DestoryUI(); //TODO


    }
}
