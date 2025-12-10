using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace MFramework.Runtime
{
    // GameLauncher.cs - 框架启动入口
    public class GameLauncher : MonoBehaviour
    {
        //[SerializeField]
        //private FrameworkConfig frameworkConfig;

        private static GameLauncher _instance;
        private FrameworkManager frameworkManager;

        private Queue<IGameBase> m_QueueGameModels = new Queue<IGameBase>();
        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitPlayerLoopHelper();
            // 初始化框架
            InitializeFramework();
        }

        private void InitPlayerLoopHelper()
        {
            var playerLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();
            PlayerLoopHelper.Initialize(ref playerLoop, InjectPlayerLoopTimings.Standard);
            //PlayerLoopHelper.Initialize(ref playerLoop, InjectPlayerLoopTimings.All);
        }

        private async void InitializeFramework()
        {
            // 第一步：提前初始化日志系统
            await InitializeLogSystemImmediately();

            // 显示启动界面
            ShowLaunchScreen();

            // 分阶段初始化框架
            await InitializeFrameworkStepByStep();

            // 框架启动完成，进入游戏
            OnFrameworkInitialized();
        }

        public virtual void ShowLaunchScreen()
        {
            Debugger.Log("显示框架启动前");
        }

        public virtual void OnFrameworkInitialized()
        {
            Debugger.Log("显示框架启动完成");
        }


        // GameLauncher.cs - 详细初始化流程
        private async UniTask InitializeFrameworkStepByStep()
        {
            // 严格按照依赖顺序初始化
            m_QueueGameModels = new Queue<IGameBase>();

            // 第1步：核心基础模块
            InitializeCoreModules();

            // 第2步：资源管理模块
            await InitializeResourceModules();

            // 第3步：游戏功能模块
            await InitializeGameplayModules();

            // 第4步：高级功能模块
            await InitializeAdvancedModules();

            var totalSteps = m_QueueGameModels.Count;
            int currentStep = 0;

            while (m_QueueGameModels.Count > 0)
            {
                var module = m_QueueGameModels.Dequeue();
                frameworkManager.RegisterModule(module);
                await module.Init();
                UpdateLaunchProgress(++currentStep, totalSteps, $"初始化完成:{module}");
            }

            UpdateLaunchProgress(totalSteps, totalSteps, "框架启动完成！");
        }

        private void UpdateLaunchProgress(int current, int total, string message)
        {
            var progress = ((float)current / total) * 100;
            ////TODO 通知UI更新进度
            //EventManager.Publish(new LaunchProgressEvent
            //{
            //    Progress = progress,
            //    Message = message
            //});
            Debugger.Log($"初始化进度：{progress}% , {message}", LogType.FrameCore);
        }

        private async UniTask InitializeLogSystemImmediately()
        {
            var logger = new LoggerModule();
            frameworkManager = new FrameworkManager();
            frameworkManager.RegisterModule<ILoggerModule>(logger);
            await logger.Init();
        }

        private void InitializeCoreModules()
        {
            // 2. 事件系统（基础通信机制）
            var eventManager = new EventManager();
            //frameworkManager.RegisterModule<IEventManager>(eventManager);
            //await eventManager.Initialize();
            m_QueueGameModels.Enqueue(eventManager);

            m_QueueGameModels.Enqueue(new ResourcesManager());

            m_QueueGameModels.Enqueue(new UIManager());

            m_QueueGameModels.Enqueue(new PersistenceDataManager());

            m_QueueGameModels.Enqueue(new TimerManager());

            m_QueueGameModels.Enqueue(new AudioManager());
            //// 3. 配置管理系统
            //var configManager = new ConfigManager();
            //m_QueueGameModels.Enqueue(new ConfigManager());

            //// 4. 对象池系统
            //var poolManager = new PoolManager();
            //m_QueueGameModels.Enqueue(new PoolManager());
        }

        private async UniTask InitializeResourceModules()
        {
            //// 5. 资源管理模块
            //var resourceManager = new ResourceManager();
            //frameworkManager.RegisterModule<IResourceManager>(resourceManager);
            //await resourceManager.Initialize();

            //// 6. 场景管理模块
            //var sceneManager = new SceneManager();
            //frameworkManager.RegisterModule<ISceneManager>(sceneManager);
            //await sceneManager.Initialize();

            await UniTask.CompletedTask;

        }

        private async UniTask InitializeGameplayModules()
        {
            //// 7. UI管理系统
            //var uiManager = new UIManager();
            //frameworkManager.RegisterModule<IUIManager>(uiManager);
            //await uiManager.Initialize();

            //// 8. 音频管理系统
            //var audioManager = new AudioManager();
            //frameworkManager.RegisterModule<IAudioManager>(audioManager);
            //await audioManager.Initialize();

            //// 9. 定时器系统
            //var timerManager = new TimerManager();
            //frameworkManager.RegisterModule<ITimerManager>(timerManager);
            //await timerManager.Initialize();

            //// 10. 状态机系统
            //var stateMachineManager = new StateMachineManager();
            //frameworkManager.RegisterModule<IStateMachineManager>(stateMachineManager);
            //await stateMachineManager.Initialize();
            await UniTask.CompletedTask;
        }

        private async UniTask InitializeAdvancedModules()
        {
            //// 11. 网络模块（可选）
            //if (frameworkConfig.enableNetwork)
            //{
            //    var networkManager = new NetworkManager();
            //    frameworkManager.RegisterModule<INetworkManager>(networkManager);
            //    await networkManager.Initialize();
            //}

            //// 12. 热更新模块（可选）
            //if (frameworkConfig.enableHotUpdate)
            //{
            //    var hotUpdateManager = new HotUpdateManager();
            //    frameworkManager.RegisterModule<IHotUpdateManager>(hotUpdateManager);
            //    await hotUpdateManager.Initialize();
            //}

            //// 13. 调试工具模块
            //var debugManager = new DebugManager();
            //frameworkManager.RegisterModule<IDebugManager>(debugManager);
            //await debugManager.Initialize();
            await UniTask.CompletedTask;
        }

        #region mono

        private void Update()
        {
            frameworkManager.Update();
        }

        private void OnDestroy()
        {
            frameworkManager.OnDestroy();
            frameworkManager = null;
        }
        #endregion
    }
}