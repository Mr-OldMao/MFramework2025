using Cysharp.Threading.Tasks;
namespace MFramework.Runtime
{
    public interface IUIController : IShutdown
    {
        UniTask Init(IUIView view, IUIModel model);
        IUIView View { get;}
        IUIModel Model { get; }
    }
}
