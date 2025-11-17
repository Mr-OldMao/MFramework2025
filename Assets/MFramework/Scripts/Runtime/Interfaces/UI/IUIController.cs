namespace MFramework.Runtime
{
    public interface IUIController
    {
        IUIView View { get; }
        IUIModel Model { get; }

        void Initialize(IUIView view, IUIModel model);
        void OnShow(object data);
        void OnHide();
        void OnClose();
    }
}
