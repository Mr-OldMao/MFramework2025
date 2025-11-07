using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

namespace MFramework.Runtime
{
    /// <summary>
    /// 资源管理器实现
    /// </summary>
    public class ResourcesManager : GameModuleBase, IUpdatableModule, IResourcesManager
    {
        // 资源缓存字典
        private Dictionary<string, AsyncOperationHandle> m_AssetHandles;
        // 场景缓存字典
        private Dictionary<string, AsyncOperationHandle> m_SceneHandles;

        // 默认后缀名映射
        private Dictionary<Type, string> _defaultSuffixes;
        /// <summary>
        /// 初始化资源管理器
        /// </summary>
        protected override Task OnInitialize()
        {
            m_AssetHandles = new Dictionary<string, AsyncOperationHandle>();
            m_SceneHandles = new Dictionary<string, AsyncOperationHandle>();
            InitializeDefaultSuffixes();
            return Task.CompletedTask;
        }

        #region 对外接口

        #region 资源加载

        public async void LoadAssetAsync<T>(string address, Action<T> callback, bool isAutoAddSuffix = true) where T : Object
        {
            var res = await LoadAssetAsync<T>(address, null, null, isAutoAddSuffix);
            callback?.Invoke(res);
        }

        public async Task<T> LoadAssetAsync<T>(string address, bool isAutoAddSuffix = true) where T : Object
        {
            return await LoadAssetAsync<T>(address, null, null, isAutoAddSuffix);
        }

        public async Task<T> LoadAssetAsync<T>(string address, Action<AsyncOperationHandle<T>> completedCallback,
            Action<AsyncOperationHandle> destroyedCallback, bool isAutoAddSuffix = true) where T : Object
        {
            if (string.IsNullOrEmpty(address))
            {
                Debugger.LogError("LoadAssetAsync: Address is null or empty", LogType.FrameNormal);
                return null;
            }
            if (isAutoAddSuffix)
            {
                address = ProcessAssetAddress<T>(address);
            }
            try
            {
                if (m_AssetHandles.ContainsKey(address) && m_AssetHandles[address].IsValid())
                {
                    var asset = m_AssetHandles[address].Result as T;
                    if (asset != null)
                    {
                        return asset;
                    }
                }
                var handle = Addressables.LoadAssetAsync<T>(address);
                if (completedCallback != null)
                {
                    handle.Completed += completedCallback;
                }
                if (destroyedCallback != null)
                {
                    handle.Destroyed += destroyedCallback;
                }
                await handle.Task;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    m_AssetHandles[address] = handle;
                    return handle.Result;
                }
                else
                {
                    Debugger.LogError($"LoadAssetAsync Failed: {address}, Status: {handle.Status}", LogType.FrameCore);
                    Addressables.Release(handle);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debugger.LogError($"LoadAssetAsync Exception: {address}, Error: {ex.Message}", LogType.FrameCore);
                return null;
            }
        }


        public async Task<List<T>> LoadAssetsAsync<T>(List<string> addresses, bool isAutoAddSuffix = true) where T : Object
        {
            return await LoadAssetsAsync<T>(addresses, null, null, isAutoAddSuffix);
        }

        public async Task<List<T>> LoadAssetsAsync<T>(List<string> addresses, Action<AsyncOperationHandle<T>> completedCallback,
            Action<AsyncOperationHandle> destroyedCallback = null, bool isAutoAddSuffix = true) where T : Object
        {
            var results = new List<T>();

            if (addresses == null || addresses.Count == 0)
                return results;

            try
            {
                var tasks = new List<Task<T>>();

                foreach (var address in addresses)
                {
                    tasks.Add(LoadAssetAsync<T>(address, completedCallback, destroyedCallback, isAutoAddSuffix));
                }

                await Task.WhenAll(tasks);

                foreach (var task in tasks)
                {
                    if (task.IsCompletedSuccessfully && task.Result != null)
                    {
                        results.Add(task.Result);
                    }
                }

                return results;
            }

            catch (Exception ex)
            {
                Debugger.LogError($"LoadAssetsAsync Exception: {ex.Message}", LogType.FrameCore);
                return results;
            }
        }


        /// <summary>
        /// 预加载资源
        /// </summary>
        public async Task<List<T>> PreloadAssetsAsync<T>(List<string> addresses, bool isAutoAddSuffix = true) where T : Object
        {
            return await LoadAssetsAsync<T>(addresses, null, null, isAutoAddSuffix);
        }

        public async void PreloadAssetsAsync<T>(List<string> addresses, Action<List<T>> allCompletedCallback, bool isAutoAddSuffix = true) where T : Object
        {
            var resList = await PreloadAssetsAsync<T>(addresses, isAutoAddSuffix);
            allCompletedCallback?.Invoke(resList);
        }

        public async void PreloadAssetsAsync<T>(List<string> addresses, Action<List<T>> allCompletedCallback, Action<float> loadPregressCallback, bool isAutoAddSuffix = true) where T : Object
        {
            float loadProgress = 0;
            List<T> resList = new List<T>();
            for (int i = 0; i < addresses?.Count; i++)
            {
                var res = await LoadAssetAsync<T>(addresses[i], null, null, isAutoAddSuffix);
                resList.Add(res);
                loadProgress = (float)(i + 1) / addresses.Count;
                loadPregressCallback?.Invoke(loadProgress);
            }
            allCompletedCallback?.Invoke(resList);
        }



        public async Task LoadSceneAsync(string sceneAddress)
        {
            if (string.IsNullOrEmpty(sceneAddress))
            {
                Debug.LogError("LoadSceneAsync: Scene address is null or empty");
                return;
            }

            try
            {
                if (!sceneAddress.Contains("."))
                {
                    sceneAddress += ".unity";
                }
                // 如果场景已加载，直接返回
                if (m_SceneHandles.ContainsKey(sceneAddress) && m_SceneHandles[sceneAddress].IsValid())
                {
                    Debug.Log($"Scene already loaded: {sceneAddress}");
                    return;
                }

                var handle = Addressables.LoadSceneAsync(sceneAddress);
                await handle.Task;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    m_SceneHandles[sceneAddress] = handle;
                    Debug.Log($"Scene loaded successfully: {sceneAddress}");
                }
                else
                {
                    Debug.LogError($"LoadSceneAsync Failed: {sceneAddress}, Status: {handle.Status}");
                    Addressables.Release(handle);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"LoadSceneAsync Exception: {sceneAddress}, Error: {ex.Message}");
            }
        }

        #endregion

        #region 实例化
        public async Task<GameObject> InstantiateAsset(string address, bool isAutoAddSuffix = true)
        {
            if (isAutoAddSuffix)
            {
                address = ProcessAssetAddress<GameObject>(address);
            }
            var res = Addressables.InstantiateAsync(address);
            await res.Task;

            //if (m_AssetHandles.ContainsKey(address))
            //{
            //    m_AssetHandles.Remove(address);
            //}
            //m_AssetHandles.Add(address, res);
            return res.Result;
        }

        public bool ReleaseInstance(GameObject go)
        {
            return Addressables.ReleaseInstance(go);
        }

        public bool ReleaseInstance(AsyncOperationHandle handle)
        {
            return Addressables.ReleaseInstance(handle);
        }
        #endregion


        #region 卸载资源
        /// <summary>
        /// 卸载资源
        /// </summary>
        public void ReleaseAsset(Object asset)
        {
            if (asset == null) return;

            try
            {
                Addressables.Release(asset);

                // 从缓存中移除
                string keyToRemove = null;
                foreach (var kvp in m_AssetHandles)
                {
                    if (kvp.Value.Result == asset)
                    {
                        keyToRemove = kvp.Key;
                        break;
                    }
                }

                if (keyToRemove != null)
                {
                    m_AssetHandles.Remove(keyToRemove);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"ReleaseAsset Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// 卸载资源（通过地址）
        /// </summary>
        public void ReleaseAsset(string address)
        {
            if (string.IsNullOrEmpty(address)) return;

            try
            {
                if (m_AssetHandles.ContainsKey(address))
                {
                    var handle = m_AssetHandles[address];
                    Addressables.Release(handle);
                    m_AssetHandles.Remove(address);
                }
                else
                {
                    Debug.LogError($"ReleaseAsset: Asset not found in cache: {address}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"ReleaseAsset Exception: {address}, Error: {ex.Message}");
            }
        }

        /// <summary>
        /// 卸载资源（通过地址）
        /// </summary>
        public void ReleaseAsset<T>(string address, bool isAutoAddSuffix = true) where T : Object
        {
            if (string.IsNullOrEmpty(address)) return;

            if (isAutoAddSuffix)
            {
                address = ProcessAssetAddress<T>(address);
            }
            ReleaseAsset(address);
        }

        /// <summary>
        /// 清理所有资源
        /// </summary>
        public void ReleaseAllAssets()
        {
            try
            {
                // 释放所有资源句柄
                foreach (var handle in m_AssetHandles.Values)
                {
                    if (handle.IsValid())
                    {
                        Addressables.Release(handle);
                    }
                }

                // 释放所有场景句柄
                foreach (var handle in m_SceneHandles.Values)
                {
                    if (handle.IsValid())
                    {
                        Addressables.Release(handle);
                    }
                }

                m_AssetHandles.Clear();
                m_SceneHandles.Clear();

                Debug.Log("ResourcesManager cleared all resources");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Clear Exception: {ex.Message}");
            }
        }
        #endregion


        #region 其他
        public AsyncOperationHandle GetAssetHandle<T>(string address, bool isAutoAddSuffix = true) where T : Object
        {
            if (isAutoAddSuffix)
            {
                address = ProcessAssetAddress<T>(address);
            }
            if (m_AssetHandles.ContainsKey(address))
            {
                return m_AssetHandles[address];
            }
            else
            {
                Debugger.LogError($"Addressable handle not found for address: {address}", LogType.FrameCore);
                return default;
            }
        }

        /// <summary>
        /// 获取资源加载状态
        /// </summary>
        public bool IsAssetLoaded(string address)
        {
            return m_AssetHandles.ContainsKey(address) &&
                   m_AssetHandles[address].IsValid() &&
                   m_AssetHandles[address].IsDone;
        }

        /// <summary>
        /// 获取资源加载状态
        /// </summary>
        public bool IsAssetLoaded<T>(string address, bool isAutoAddSuffix = true) where T : UnityEngine.Object
        {
            if (isAutoAddSuffix)
            {
                address = ProcessAssetAddress<T>(address);
            }
            return m_AssetHandles.ContainsKey(address) &&
                   m_AssetHandles[address].IsValid() &&
                   m_AssetHandles[address].IsDone;
        }

        /// <summary>
        /// 添加自定义后缀名映射
        /// </summary>
        public void AddSuffixMapping(Type assetType, string suffix)
        {
            if (_defaultSuffixes.ContainsKey(assetType))
            {
                _defaultSuffixes[assetType] = suffix;
            }
            else
            {
                _defaultSuffixes.Add(assetType, suffix);
            }
        }


        /// <summary>
        /// 获取资源统计信息
        /// </summary>
        public void GetResourceStats()
        {
            Debug.Log($"Resource Stats - Assets: {m_AssetHandles.Count}, Scenes: {m_SceneHandles.Count}");
        }

        #endregion

        #endregion

        #region 后缀名
        /// <summary>
        /// 初始化默认后缀名映射
        /// </summary>
        private void InitializeDefaultSuffixes()
        {
            _defaultSuffixes = new Dictionary<Type, string>
        {
            // 预制体
            { typeof(GameObject), ".prefab" },
            
            // 纹理
            { typeof(Texture2D), ".png" },
            { typeof(Sprite), ".png" },
            { typeof(RenderTexture), ".renderTexture" },
            { typeof(SpriteAtlas), ".spriteatlas" },

            // 音频
            { typeof(AudioClip), ".mp3" },
            
            // 材质和着色器
            { typeof(Material), ".mat" },
            { typeof(Shader), ".shader" },
            { typeof(ComputeShader), ".compute" },
            
            // 动画
            { typeof(AnimationClip), ".anim" },
            //{ typeof(AnimatorController), ".controller" },
            
            // 字体
            { typeof(Font), ".ttf" },
            { typeof(TMP_FontAsset), ".asset" },
            
            // 配置和数据
            { typeof(TextAsset), ".bytes" },
            { typeof(ScriptableObject), ".asset" },
            
            
            // 其他
            { typeof(Cubemap), ".cubemap" },
            { typeof(Avatar), ".avatar" },
            { typeof(Flare), ".flare" },
            { typeof(LensFlare), ".flare" },
            { typeof(PhysicMaterial), ".physicMaterial" }
        };
        }

        /// <summary>
        /// 处理资源地址，自动添加后缀名
        /// </summary>
        private string ProcessAssetAddress<T>(string address) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(address))
                return address;

            // 如果地址已经包含后缀名，直接返回
            if (address.Contains("."))
                return address;

            // 获取资源类型
            Type assetType = typeof(T);

            // 如果是泛型或者Object类型，不添加后缀
            if (assetType == typeof(UnityEngine.Object) || assetType.IsGenericType)
                return address;

            // 查找对应的后缀名
            if (_defaultSuffixes.TryGetValue(assetType, out string suffix))
            {
                var res = address + suffix;
                //Debug.Log($"Auto added suffix: {address} -> {res}");
                return res;
            }

            // 没有找到对应后缀名，返回原地址
            return address;
        }
        #endregion


        #region 定时自动释放资源 TODO

        #endregion

        #region Frame

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            ReleaseAllAssets();
        }

        public void OnUpdate(float deltaTime)
        {

        }

        protected override void OnShutdown()
        {
            ReleaseAllAssets();
        }
        #endregion
    }
}