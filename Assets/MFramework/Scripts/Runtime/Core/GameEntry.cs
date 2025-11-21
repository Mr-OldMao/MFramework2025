// GameEntry.cs - 框架全局静态入口
namespace MFramework.Runtime
{
    public static class GameEntry
    {
        private static FrameworkManager Framework => FrameworkManager.Instance;

        // 核心模块快捷访问
        //public static ILoggerModule Logger => Framework?.GetModule<ILoggerModule>();
        public static EventManager Event => Framework?.GetModule<EventManager>();

        // 其他模块（按需添加）
        public static ResourcesManager Resource => Framework?.GetModule<IResourcesManager>() as ResourcesManager;
        public static UIManager UI =>  Framework?.GetModule<IUIManager>() as UIManager;
        public static PersistenceDataManager LocalData => Framework?.GetModule<IPersistenceDataManager>() as PersistenceDataManager;
        public static IAudioManager Audio => Framework?.GetModule<IAudioManager>();
        public static ISceneManager Scene => Framework?.GetModule<ISceneManager>();
        public static ITimerManager Timer => Framework?.GetModule<ITimerManager>();
        public static IPoolManager Pool => Framework?.GetModule<IPoolManager>();
        //public static DataManager Data => Framework?.GetModule<IDataManager>() as DataManager;

        // 框架状态
        public static bool IsInitialized => Framework != null;

        // 快捷方法
        public static T GetModule<T>() where T : class, IGameBase
        {
            return Framework?.GetModule<T>();
        }

        // 初始化检查
        public static void EnsureInitialized()
        {
            if (!IsInitialized)
            {
                throw new System.InvalidOperationException("框架未初始化，请确保GameLauncher已启动");
            }
        }
    }
}