using System.Threading.Tasks;
using static MFramework.Runtime.UIViewBase;

namespace MFramework.Runtime
{
    public interface IUIView : IGameModule
    {
        UIStateProgressType StateProgress { get; }
        UILayerType Layer { get; }
        bool IsActive { get; }

        Task Show(object showData = null, object showBeforeData = null);
        Task Hide(object hideData = null, object hideBoforeData = null);
        void DestoryUI();
    }
}
