using MFramework.Runtime;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// 资源管理器实现
/// </summary>
public class ResourcesManager : GameModuleBase, IUpdatableModule, IResourcesManager
{

    // 资源缓存字典
    private Dictionary<string, AsyncOperationHandle> _assetHandles;
    // 场景缓存字典
    private Dictionary<string, AsyncOperationHandle> _sceneHandles;

    // 初始化状态
    private bool _isInitialized = false;
    // 默认后缀名映射
    private Dictionary<Type, string> _defaultSuffixes;
    /// <summary>
    /// 初始化资源管理器
    /// </summary>
    protected override Task OnInitialize()
    {
        if (_isInitialized) return Task.CompletedTask;

        _assetHandles = new Dictionary<string, AsyncOperationHandle>();
        _sceneHandles = new Dictionary<string, AsyncOperationHandle>();
        InitializeDefaultSuffixes();
        Debug.Log("ResourcesManager Initialized");
        _isInitialized = true;
        return Task.CompletedTask;
    }

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
            Debug.Log($"Auto added suffix: {address} -> {res}");
            return res;
        }

        // 没有找到对应后缀名，返回原地址
        return address;
    }

    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="address"></param>
    /// <param name="isAutoAddSuffix">是否自动加后缀名</param>
    /// <returns></returns>
    public async Task<T> LoadAssetAsync<T>(string address, bool isAutoAddSuffix = true) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(address))
        {
            Debug.LogError("LoadAssetAsync: Address is null or empty");
            return null;
        }

        // 处理地址，自动添加后缀名  ---TODO需要验证性能如何
        string processedAddress = address;
        if (isAutoAddSuffix)
        {
            processedAddress = ProcessAssetAddress<T>(address);
        }
        try
        {
            // 如果资源已加载，直接返回
            if (_assetHandles.ContainsKey(processedAddress) && _assetHandles[processedAddress].IsValid())
            {
                var asset = _assetHandles[processedAddress].Result as T;
                if (asset != null)
                {
                    return asset;
                }
            }

            // 异步加载资源
            var handle = Addressables.LoadAssetAsync<T>(processedAddress);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _assetHandles[processedAddress] = handle;
                return handle.Result;
            }
            else
            {
                Debug.LogError($"LoadAssetAsync Failed: {processedAddress}, Status: {handle.Status}");
                Addressables.Release(handle);
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"LoadAssetAsync Exception: {processedAddress}, Error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 异步加载多个资源
    /// </summary>
    public async Task<List<T>> LoadAssetsAsync<T>(List<string> addresses, bool isAutoAddSuffix = true) where T : UnityEngine.Object
    {
        var results = new List<T>();

        if (addresses == null || addresses.Count == 0)
            return results;

        try
        {
            var tasks = new List<Task<T>>();

            foreach (var address in addresses)
            {
                tasks.Add(LoadAssetAsync<T>(address, isAutoAddSuffix));
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
            Debug.LogError($"LoadAssetsAsync Exception: {ex.Message}");
            return results;
        }
    }

    /// <summary>
    /// 异步加载场景
    /// </summary>
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
            if (_sceneHandles.ContainsKey(sceneAddress) && _sceneHandles[sceneAddress].IsValid())
            {
                Debug.Log($"Scene already loaded: {sceneAddress}");
                return;
            }

            var handle = Addressables.LoadSceneAsync(sceneAddress);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _sceneHandles[sceneAddress] = handle;
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

    /// <summary>
    /// 卸载资源
    /// </summary>
    public void ReleaseAsset(UnityEngine.Object asset)
    {
        if (asset == null) return;

        try
        {
            Addressables.Release(asset);

            // 从缓存中移除
            string keyToRemove = null;
            foreach (var kvp in _assetHandles)
            {
                if (kvp.Value.Result == asset)
                {
                    keyToRemove = kvp.Key;
                    break;
                }
            }

            if (keyToRemove != null)
            {
                _assetHandles.Remove(keyToRemove);
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
            if (_assetHandles.ContainsKey(address))
            {
                var handle = _assetHandles[address];
                Addressables.Release(handle);
                _assetHandles.Remove(address);
            }
            else
            {
                Debug.LogWarning($"ReleaseAsset: Asset not found in cache: {address}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"ReleaseAsset Exception: {address}, Error: {ex.Message}");
        }
    }

    /// <summary>
    /// 预加载资源
    /// </summary>
    public async Task PreloadAssetsAsync<T>(List<string> addresses, bool isAutoAddSuffix = true) where T : UnityEngine.Object
    {
        if (addresses == null || addresses.Count == 0) return;

        try
        {
            var tasks = new List<Task>();
            foreach (var address in addresses)
            {
                string processedAddress = address;
                if (isAutoAddSuffix)
                {
                    processedAddress = ProcessAssetAddress<T>(address);
                }
                // 只预加载未缓存的资源
                if (!_assetHandles.ContainsKey(processedAddress) || !_assetHandles[processedAddress].IsValid())
                {
                    tasks.Add(LoadAssetAsync<T>(processedAddress, false));
                }
            }

            Debug.Log($"预加载资源目标个数： {addresses.Count},实际加载个数：" + tasks.Count);

            if (tasks.Count > 0)
            {
                await Task.WhenAll(tasks);
                Debug.Log($"Preload completed: {tasks.Count} assets loaded");
            }
            else
            {
                Debug.Log("All assets are already preloaded");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"PreloadAssetsAsync Exception: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取资源加载状态
    /// </summary>
    public bool IsAssetLoaded(string address)
    {
        return _assetHandles.ContainsKey(address) &&
               _assetHandles[address].IsValid() &&
               _assetHandles[address].IsDone;
    }

    /// <summary>
    /// 清理所有资源
    /// </summary>
    public void Clear()
    {
        try
        {
            // 释放所有资源句柄
            foreach (var handle in _assetHandles.Values)
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }

            // 释放所有场景句柄
            foreach (var handle in _sceneHandles.Values)
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }

            _assetHandles.Clear();
            _sceneHandles.Clear();

            Debug.Log("ResourcesManager cleared all resources");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Clear Exception: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取资源统计信息
    /// </summary>
    public void GetResourceStats()
    {
        Debug.Log($"Resource Stats - Assets: {_assetHandles.Count}, Scenes: {_sceneHandles.Count}");
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Clear();
    }

    public void OnUpdate(float deltaTime)
    {

    }


    protected override void OnShutdown()
    {
    }
}