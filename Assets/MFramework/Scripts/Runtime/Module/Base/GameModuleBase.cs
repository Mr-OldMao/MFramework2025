// GameModuleBase.cs - 模块基类
using System.Diagnostics;
using System.Threading.Tasks;
namespace MFramework.Runtime
{
    public abstract class GameModuleBase : IGameModule
    {
        public ModuleState State { get; protected set; } = ModuleState.Uninitialized;

        public virtual int Priority => 100;

        protected ILoggerModule Logger => FrameworkManager.Instance.GetModule<ILoggerModule>();
        protected IEventManager EventManager => FrameworkManager.Instance.GetModule<IEventManager>();

        public virtual async Task Initialize()
        {
            Logger?.Log($"{GetType().Name} 开始初始化...", LogType.FrameNormal);
            await OnInitialize();
            Logger?.Log($"{GetType().Name} 初始化完成", LogType.FrameNormal);
        }

        public virtual void Shutdown()
        {
            OnShutdown();
        }

        protected abstract Task OnInitialize();
        protected abstract void OnShutdown();
    }

    public enum ModuleState
    {
        Uninitialized,
        Initializing,
        Initialized,
        ShuttingDown,
        Shutdown
    } 
}