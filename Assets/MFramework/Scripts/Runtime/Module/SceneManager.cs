using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
//using Scene = UnityEngine.SceneManagement.Scene;

namespace MFramework.Runtime
{
    /// <summary>
    /// 场景管理器实现
    /// </summary>
    public class SceneManager : ISceneManager
    {
        // 场景状态管理
        private readonly Dictionary<string, SceneStateData> m_DicSceneStateData = new Dictionary<string, SceneStateData>();
        // 加载队列
        private readonly Queue<SceneLoadRequest> m_QueueLoad = new Queue<SceneLoadRequest>();
        private readonly Queue<SceneUnloadRequest> m_QueueUnload = new Queue<SceneUnloadRequest>();
        // 预加载缓存
        private readonly Dictionary<string, AsyncOperation> m_DicPreloadedCache = new Dictionary<string, AsyncOperation>();

        // 配置
        private bool m_AutoUnloadUnusedAssets = true;

        private bool m_IsLoadingScene = false;
        private bool m_IsUnloadingScene = false;

        private string m_CurrentScene;
        public string CurrentScene { get => m_CurrentScene; }

        public UniTask Init()
        {
            int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCount;
            for (int i = 0; i < sceneCount; i++)
            {
                Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (scene.isLoaded)
                {
                    m_DicSceneStateData[scene.name] = new SceneStateData
                    {
                        sceneName = scene.name,
                        state = ESceneStateType.Loaded,
                        loadMode = LoadSceneMode.Single,
                        isActive = scene == UnityEngine.SceneManagement.SceneManager.GetActiveScene()
                    };

                    if (scene == UnityEngine.SceneManagement.SceneManager.GetActiveScene())
                    {
                        m_CurrentScene = scene.name;
                    }
                }
            }
            return UniTask.CompletedTask;
        }

        public void LoadSceneAsync(string sceneName, LoadSceneMode sceneLoadMode = LoadSceneMode.Single, Action<float> loadingCallback = null,
                                        Action loadedCallback = null, Action unloadedCallback = null, float minLoadTime = 1)
        {
            bool isAddLoadQueue = true;
            //查看预加载
            if (m_DicSceneStateData.TryGetValue(sceneName, out SceneStateData sceneState))
            {
                switch (sceneState.state)
                {
                    case ESceneStateType.None:
                    case ESceneStateType.Unloading:
                    case ESceneStateType.Unloaded:

                        isAddLoadQueue = true;
                        break;
                    case ESceneStateType.Preloaded:
                        isAddLoadQueue = true;
                        if (sceneState.loadMode != sceneLoadMode)
                        {
                            Debugger.LogError($"LoadSceneAsync sceneLoadMode different ，current sceneLoadMode:{sceneLoadMode}, preload sceneLoadMode :{sceneState.loadMode}, sceneName:{sceneName}, state:{sceneState.state}", LogType.FrameCore);
                        }
                        break;
                    case ESceneStateType.Loading:
                    case ESceneStateType.Preloading:
                    case ESceneStateType.Loaded:
                        isAddLoadQueue = false;
                        Debugger.LogError($"LoadSceneAsync fail ，sceneName:{sceneName}, state:{sceneState.state}", LogType.FrameCore);
                        break;
                }
            }
            if (isAddLoadQueue)
            {
                UpdateSceneState(sceneName, ESceneStateType.Loading, sceneLoadMode);
                m_QueueLoad.Enqueue(new SceneLoadRequest()
                {
                    sceneName = sceneName,
                    sceneLoadMode = sceneLoadMode,
                    loadingCallback = loadingCallback,
                    loadedCallback = loadedCallback,
                    unloadedCallback = unloadedCallback,
                    minLoadTime = minLoadTime,
                    isPreload = false
                });
                ProcessLoadQueue().Forget();
            }
        }

        public void PreloadSceneAsync(string sceneName, LoadSceneMode sceneLoadMode, Action<float> loadingCallback = null, Action loadedCallback = null)
        {
            bool isAddLoadQueue = true;
            //查看预加载
            if (m_DicSceneStateData.TryGetValue(sceneName, out SceneStateData sceneState))
            {
                switch (sceneState.state)
                {
                    case ESceneStateType.Unloading:
                    case ESceneStateType.Unloaded:
                    case ESceneStateType.None:
                        isAddLoadQueue = true;
                        break;
                    case ESceneStateType.Loading:
                    case ESceneStateType.Preloading:
                    case ESceneStateType.Preloaded:
                    case ESceneStateType.Loaded:
                        isAddLoadQueue = false;
                        Debugger.LogError($"PreloadSceneAsync fail ，sceneName:{sceneName}, state:{sceneState.state}", LogType.FrameCore);
                        break;
                }
            }
            if (isAddLoadQueue)
            {
                UpdateSceneState(sceneName, ESceneStateType.Loading, sceneLoadMode);
                m_QueueLoad.Enqueue(new SceneLoadRequest()
                {
                    sceneName = sceneName,
                    sceneLoadMode = sceneLoadMode,
                    loadingCallback = loadingCallback,
                    loadedCallback = loadedCallback,
                    isPreload = true
                });
                ProcessLoadQueue().Forget();
            }
        }
        public void UnloadSceneAsync(string sceneName, Action<float> unloadingCallback = null, Action unloadedCallback = null)
        {
            ProcessUnloadQueue(sceneName, unloadingCallback, unloadedCallback).Forget();
        }


        public void ReloadCurrentScene(LoadSceneMode sceneLoadMode = LoadSceneMode.Single, Action<float> loadingCallback = null, Action loadedCallback = null, float minLoadTime = 1)
        {
            if (m_DicPreloadedCache?.Count > 0)
            {
                Debugger.LogError("TODO 当有其他场景预加载后，无法重新加载当前场景");
                return;
                //for (int i = 0; i < m_DicPreloadedCache.Count; i++)
                //{
                //    UnloadSceneAsync(m_DicPreloadedCache.ElementAt(i).Key);
                //}
            }
            Scene curScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (m_DicSceneStateData.TryGetValue(curScene.name, out SceneStateData sceneState))
            {
                sceneState.state = ESceneStateType.None;
                LoadSceneAsync(curScene.name, sceneLoadMode, loadingCallback, loadedCallback);
            }
            else
            {
                Debugger.LogError($"ReloadCurrentScene fail ，sceneName:{curScene.name}, state:{sceneState.state}", LogType.FrameCore);
            }
        }


        public void Shutdown()
        {
            m_DicSceneStateData.Clear();
            m_DicPreloadedCache.Clear();
            m_QueueLoad.Clear();
            m_QueueUnload.Clear();
            Debugger.Log("Shutdown ResourcesManager", LogType.FrameCore);
        }

        #region Load
        private async UniTaskVoid ProcessLoadQueue()
        {
            if (m_IsLoadingScene) return;

            m_IsLoadingScene = true;

            try
            {
                while (m_QueueLoad.Count > 0)
                {
                    var request = m_QueueLoad.Dequeue();
                    await ProcessLoadRequest(request);
                }
            }
            finally
            {
                m_IsLoadingScene = false;
            }
        }

        private async UniTask ProcessLoadRequest(SceneLoadRequest request)
        {
            string sceneName = request.sceneName;

            try
            {
                Debugger.Log($"Load scene start: {sceneName} [{request.sceneLoadMode}]", LogType.FrameNormal);

                float startTime = Time.time;

                // 检查是否预加载过
                AsyncOperation asyncOp = null;
                if (m_DicPreloadedCache.TryGetValue(sceneName, out var preloadedOp))
                {
                    asyncOp = preloadedOp;
                    m_DicPreloadedCache.Remove(sceneName);
                    Debugger.Log($"Using preloaded scene: {sceneName}", LogType.FrameNormal);
                }
                else
                {
                    LoadSceneMode unityLoadMode = request.sceneLoadMode == LoadSceneMode.Single ?
                        LoadSceneMode.Single : LoadSceneMode.Additive;
                    asyncOp = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, unityLoadMode);
                }
                asyncOp.allowSceneActivation = false;

                while (asyncOp.progress < 0.9f)
                {
                    float progress = Mathf.Clamp01(asyncOp.progress / 0.9f);
                    request.loadingCallback?.Invoke(progress);
                    await UniTask.Yield();
                }

                // 如果不是预加载的Single模式，需要卸载其他场景
                if (!request.isPreload
                    && request.sceneLoadMode == LoadSceneMode.Single
                    && !string.IsNullOrEmpty(m_CurrentScene))
                {
                    await UnloadOtherAllSceneAsync(sceneName, request.unloadedCallback);
                }

                // 确保最小加载时间
                float elapsedTime = Time.time - startTime;
                if (elapsedTime < request.minLoadTime)
                {
                    float remainingTime = request.minLoadTime - elapsedTime;
                    float timer = 0f;

                    while (timer < remainingTime)
                    {
                        timer += Time.deltaTime;
                        float fakeProgress = Mathf.Lerp(1.0f, 1.0f, timer / remainingTime);
                        request.loadingCallback?.Invoke(fakeProgress);
                        await UniTask.Yield();
                    }
                }

                Debugger.Log($"Load scene end: {sceneName} [{request.sceneLoadMode}]", LogType.FrameNormal);

                request.loadingCallback?.Invoke(1.0f);

                // 场景加载完成后的处理
                if (request.isPreload)
                {
                    UpdateSceneState(sceneName, ESceneStateType.Preloaded, request.sceneLoadMode);
                    m_DicPreloadedCache.Add(sceneName, asyncOp);
                    Debugger.Log($"Preload scene successfully: {sceneName}", LogType.FrameNormal);
                }
                else
                {
                    Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName);
                    if (scene.IsValid())
                    {
                        asyncOp.allowSceneActivation = true;
                        int temp = 0;
                        await UniTask.WaitUntil(() =>
                            {
                                if (++temp > 1000)//TODO避免卡死
                                {
                                    return true;
                                }
                                return asyncOp.isDone;
                            });//接着更新后续10%
                        // 更新当前场景
                        if (request.sceneLoadMode == LoadSceneMode.Single && m_CurrentScene != sceneName)
                        {
                            m_CurrentScene = sceneName;
                            UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);
                        }
                        UpdateSceneState(sceneName, ESceneStateType.Loaded, request.sceneLoadMode);
                        if (m_AutoUnloadUnusedAssets)
                        {
                            await Resources.UnloadUnusedAssets();
                        }
                        Debugger.Log($"Load scene successfully: {sceneName}", LogType.FrameNormal);
                    }
                    else
                    {
                        Debugger.LogError($"Failed to load scene: {sceneName}");
                        UpdateSceneState(sceneName, ESceneStateType.None);
                    }
                }
                m_IsLoadingScene = false;
                request.loadedCallback?.Invoke();
            }
            catch (Exception e)
            {
                Debugger.LogError($"Error loading scene {sceneName}: {e.Message}");
                UpdateSceneState(sceneName, ESceneStateType.None);
                throw;
            }
        }

        #endregion


        #region Unload
        private async UniTask UnloadOtherAllSceneAsync(string sceneName, Action unloadedCallback)
        {
            //for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            //{
            //    Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
            //    if (scene.name != sceneName)
            //    {
            //        await UnloadSceneAsync(scene.name);
            //    }
            //}

            for (int i = 0; i < m_DicSceneStateData.Count; i++)
            {
                string name = m_DicSceneStateData.ElementAt(i).Key;
                var state = m_DicSceneStateData.ElementAt(i).Value.state;
                if (name != sceneName && state == ESceneStateType.Loaded)
                {
                    await ProcessUnloadQueue(name);
                }
            }
            unloadedCallback?.Invoke();
        }


        private async UniTask ProcessUnloadQueue(string sceneName, Action<float> unloadingCallback = null, Action unloadedCallback = null)
        {
            if (m_QueueUnload.Where(p => p.sceneName == sceneName).Count() > 0)
            {
                Debugger.LogError($"正在卸载场景：" + sceneName + "，请勿重复卸载");
                return;
            }
            m_QueueUnload.Enqueue(new SceneUnloadRequest()
            {
                sceneName = sceneName,
                unloadingCallback = unloadingCallback,
                unloadedCallback = unloadedCallback
            });

            if (m_IsUnloadingScene) return;

            m_IsUnloadingScene = true;

            try
            {
                while (m_QueueUnload.Count > 0)
                {
                    var request = m_QueueUnload.Dequeue();
                    await ProcessUnloadRequest(request);
                }
            }
            finally
            {
                m_IsUnloadingScene = false;
            }
        }

        private async UniTask ProcessUnloadRequest(SceneUnloadRequest request)
        {
            string sceneName = request.sceneName;

            try
            {
                Debugger.Log($"Unload scene start: {sceneName}", LogType.FrameNormal);

                //是否卸载唯一单一场景
                int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCount;
                if (UnityEngine.SceneManagement.SceneManager.sceneCount == 1
                    && UnityEngine.SceneManagement.SceneManager.GetSceneAt(0).name == sceneName)
                {
                    Debugger.LogError($"当前场景为唯一，无法卸载 sceneName:{sceneName}");
                    return;
                }

                AsyncOperation asyncOp = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName);
                if (asyncOp != null)
                {
                    // 更新卸载进度
                    while (!asyncOp.isDone)
                    {
                        float progress = Mathf.Clamp01(asyncOp.progress / 0.9f);
                        request.unloadingCallback?.Invoke(progress);
                        await UniTask.Yield();
                    }
                }
                UpdateSceneState(sceneName, ESceneStateType.Unloaded);
                Debugger.Log($"Unload scene end: {sceneName}", LogType.FrameNormal);

                // 卸载完成回调
                request.unloadingCallback?.Invoke(1.0f);

                // 清理场景状态
                m_DicSceneStateData.Remove(sceneName);

                // 如果卸载的是当前场景，需要重新设置当前场景
                if (sceneName == m_CurrentScene)
                {
                    UpdateCurrentSceneAfterUnload();
                }
                
                m_IsUnloadingScene = false;
                // 卸载完成回调
                request.unloadedCallback?.Invoke();

            }
            catch (Exception e)
            {
                Debugger.LogError($"Error unloading scene {sceneName}: {e.Message}");
                UpdateSceneState(sceneName, ESceneStateType.None);
                throw;
            }
        }
        #endregion

        #region Other
        private void UpdateSceneState(string sceneName, ESceneStateType newState,
            LoadSceneMode? loadMode = null, bool? isActive = null)
        {
            if (!m_DicSceneStateData.ContainsKey(sceneName))
            {
                m_DicSceneStateData[sceneName] = new SceneStateData
                {
                    sceneName = sceneName,
                    state = newState,
                    loadMode = loadMode ?? LoadSceneMode.Single,
                    isActive = isActive ?? false
                };
            }
            else
            {
                var state = m_DicSceneStateData[sceneName];
                state.state = newState;

                if (loadMode.HasValue)
                    state.loadMode = loadMode.Value;

                if (isActive.HasValue)
                    state.isActive = isActive.Value;

                m_DicSceneStateData[sceneName] = state;
            }
        }
        private void UpdateCurrentSceneAfterUnload()
        {
            // 查找第一个Loaded状态的Single模式场景
            foreach (var kvp in m_DicSceneStateData)
            {
                if (kvp.Value.state == ESceneStateType.Loaded &&
                    kvp.Value.loadMode == LoadSceneMode.Single)
                {
                    m_CurrentScene = kvp.Key;
                    return;
                }
            }

            // 如果没有Single模式场景，查找第一个Loaded场景
            foreach (var kvp in m_DicSceneStateData)
            {
                if (kvp.Value.state == ESceneStateType.Loaded)
                {
                    m_CurrentScene = kvp.Key;
                    return;
                }
            }

            // 如果没有加载任何场景
            m_CurrentScene = null;
        }
        #endregion


        public bool IsSceneLoaded(string sceneName)
        {
            return m_DicSceneStateData.TryGetValue(sceneName ,out SceneStateData stateData ) && stateData.state == ESceneStateType.Loaded;
        }

        public ESceneStateType GetSceneStateType(string sceneName)
        {
            if (m_DicSceneStateData.TryGetValue(sceneName, out SceneStateData stateData))
            {
                return stateData.state;
            }
            return  ESceneStateType.None;
        }

        public List<string> GetAllLoadedScenes()
        {
           return m_DicSceneStateData.Values.Where(kvp => kvp.state == ESceneStateType.Loaded).Select(kvp => kvp.sceneName).ToList();
        }

        public List<string> GetAllLoadingScenes()
        {
            return m_DicSceneStateData.Values.Where(kvp => kvp.state == ESceneStateType.Loading).Select(kvp => kvp.sceneName).ToList();
        }

        // 内部数据结构
        private class SceneStateData
        {
            public string sceneName;
            public ESceneStateType state;
            public LoadSceneMode loadMode;
            public bool isActive;
        }

        private struct SceneLoadRequest
        {
            public string sceneName;
            public LoadSceneMode sceneLoadMode;
            public Action<float> loadingCallback;
            public Action loadedCallback;
            public Action unloadedCallback;
            public float minLoadTime;
            public bool isPreload;
        }

        private struct SceneUnloadRequest
        {
            public string sceneName;
            public Action<float> unloadingCallback;
            public Action unloadedCallback;
        }
    }
}