using System.Threading.Tasks;
namespace MFramework.Runtime
{
    public interface IUIController : IShutdown
    {
        Task Init(IUIView view, IUIModel model);
        IUIView View { get;}
        IUIModel Model { get; }
    }
}
