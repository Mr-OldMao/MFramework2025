using System.Threading.Tasks;

namespace MFramework.Runtime
{
    public abstract class UIModelBase : IUIModel
    {
        public abstract Task Init();
        public virtual void Reset() { }

        public IUIController Controller { get; set; }
        protected UIModelBase(IUIController controller)
        {
            Controller = controller;
        }

        public void DispatchEvent(int eventId)
        {
            GameEntry.Event.DispatchEvent(eventId);
        }

        public void DispatchEvent(GameEventType gameEventType)
        {
            GameEntry.Event.DispatchEvent(gameEventType);
        }

        public void DispatchEvent<T>(int eventId, T eventData)
        {
            GameEntry.Event.DispatchEvent(eventId, eventData);
        }
    }
}
