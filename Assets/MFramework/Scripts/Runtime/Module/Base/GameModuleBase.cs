// GameModuleBase.cs - 模块基类
using System.Diagnostics;
using Cysharp.Threading.Tasks;
namespace MFramework.Runtime
{
    public abstract class GameModuleBase : IGameBase
    {
        public ModuleState State { get; protected set; } = ModuleState.Uninitialized;
        protected ILoggerModule Logger => FrameworkManager.Instance.GetModule<ILoggerModule>();
        protected IEventManager EventManager => FrameworkManager.Instance.GetModule<IEventManager>();

        public virtual async UniTask Init()
        {
            Logger?.Log($"{GetType().Name} 开始初始化...", LogType.FrameNormal);
            await OnInitialize();
            Logger?.Log($"{GetType().Name} 初始化完成", LogType.FrameNormal);
        }

        public virtual void Shutdown()
        {
            OnShutdown();
        }

        protected abstract UniTask OnInitialize();
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