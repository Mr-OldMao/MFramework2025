using System.Threading.Tasks;

namespace MFramework.Runtime
{
    public interface IUIController
    {
        IUIView View { get;}
        IUIModel Model { get; }

        Task Init(IUIView view, IUIModel model);
    }
}
