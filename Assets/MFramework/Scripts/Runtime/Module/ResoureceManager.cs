using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace MFramework.Runtime
{
    /// <summary>
    /// 统一资源管理器 - 完整实现
    /// </summary>
    public class ResourceManager : GameModuleBase, IResourceManager, IUpdatableModule
    {
        #region 内部类定义
        private class ResourceHandle
        {
            public object Asset { get; set; }
            public int ReferenceCount { get; set; }
            public DateTime LastAccessTime { get; set; }
            public string Path { get; set; }
            public ResourceSourceType SourceType { get; set; }
            public ResourceFileType FileType { get; set; }
            public long MemorySize { get; set; }
            public AssetBundle AssetBundle { get; set; }
        }

        private class AssetBundleHandle
        {
            public AssetBundle Bundle { get; set; }
            public int ReferenceCount { get; set; }
            public DateTime LastAccessTime { get; set; }
            public string BundlePath { get; set; }
            public Dictionary<string, ResourceHandle> LoadedAssets { get; set; } = new Dictionary<string, ResourceHandle>();
        }

        /// <summary>
        /// 资源加载进度信息
        /// </summary>
        public class LoadProgress
        {
            /// <summary>
            /// 资源路径或URL
            /// </summary>
            public string Path { get; set; }

            /// <summary>
            /// 加载进度（0-1）
            /// </summary>
            public float Progress { get; set; }

            /// <summary>
            /// 是否加载完成
            /// </summary>
            public bool IsDone { get; set; }

            /// <summary>
            /// 加载的资源对象（仅对UnityEngine.Object资源有效）
            /// </summary>
            public UnityEngine.Object Asset { get; set; }

            /// <summary>
            /// 加载的字节数据（仅对字节加载有效）
            /// </summary>
            public byte[] Bytes { get; set; }

            /// <summary>
            /// 加载的文本数据（仅对文本加载有效）
            /// </summary>
            public string Text { get; set; }

            /// <summary>
            /// 加载错误信息
            /// </summary>
            public Exception Error { get; set; }

            /// <summary>
            /// 当前加载状态描述
            /// </summary>
            public string Status { get; set; }

            /// <summary>
            /// 已加载字节数（对网络加载有效）
            /// </summary>
            public long LoadedBytes { get; set; }

            /// <summary>
            /// 总字节数（对网络加载有效）
            /// </summary>
            public long TotalBytes { get; set; }
        }
        #endregion

        #region 字段定义
        private readonly Dictionary<string, ResourceHandle> _resourceHandles = new Dictionary<string, ResourceHandle>();
        private readonly Dictionary<string, AssetBundleHandle> _assetBundleHandles = new Dictionary<string, AssetBundleHandle>();
        private readonly Dictionary<GameObject, (string path, ResourceSourceType sourceType)> _instanceToSourceMap = new Dictionary<GameObject, (string, ResourceSourceType)>();

        private string _networkCacheDirectory;
        private readonly object _lockObject = new object();
        private readonly int _maxCacheCount = 100;
        private readonly long _maxMemorySize = 512 * 1024 * 1024; // 512MB
        private readonly float _cacheCleanupInterval = 30f;
        private float _lastCleanupTime;
        #endregion

        #region 初始化
        /// <summary>
        /// 初始化资源管理器
        /// </summary>
        protected override async Task OnInitialize()
        {
            _networkCacheDirectory = Path.Combine(Application.persistentDataPath, "NetworkCache");
            if (!Directory.Exists(_networkCacheDirectory))
            {
                Directory.CreateDirectory(_networkCacheDirectory);
            }

            Debugger.Log($"资源管理器初始化完成，网络缓存目录: {_networkCacheDirectory}", LogType.FrameCore);
            await Task.CompletedTask;
        }

        /// <summary>
        /// 关闭资源管理器，清理所有资源
        /// </summary>
        protected override void OnShutdown()
        {
            lock (_lockObject)
            {
                foreach (var handle in _resourceHandles.Values)
                {
                    if (handle.Asset is UnityEngine.Object unityObj && handle.AssetBundle == null)
                    {
                        if (Application.isPlaying)
                        {
                            UnityEngine.Object.Destroy(unityObj);
                        }
                        else
                        {
                            UnityEngine.Object.DestroyImmediate(unityObj);
                        }
                    }
                }
                _resourceHandles.Clear();

                foreach (var bundleHandle in _assetBundleHandles.Values)
                {
                    bundleHandle.Bundle.Unload(true);
                }
                _assetBundleHandles.Clear();
            }

            _instanceToSourceMap.Clear();
            Resources.UnloadUnusedAssets();

            Debugger.Log("资源管理器已关闭", LogType.FrameNormal);
        }
        #endregion

        #region 核心加载接口实现
        /// <summary>
        /// 通用异步资源加载方法
        /// </summary>
        public async Task<T> LoadAssetAsync<T>(
            string assetPath,
            ResourceSourceType resourceSourceType,
            ResourceFileType resourceFileType = ResourceFileType.UnityAsset,
            Action<LoadProgress> onProgress = null)
        {
            try
            {
                // 根据资源文件类型选择不同的加载逻辑
                switch (resourceFileType)
                {
                    case ResourceFileType.UnityAsset:
                        return await LoadUnityAssetInternal<T>(assetPath, resourceSourceType, onProgress);

                    case ResourceFileType.Txt:
                        if (typeof(T) != typeof(string))
                            throw new InvalidCastException($"Text资源必须使用string类型，当前类型: {typeof(T)}");
                        var text = await LoadTextInternal(assetPath, resourceSourceType, onProgress);
                        return (T)(object)text;

                    case ResourceFileType.Json:
                        var jsonText = await LoadTextInternal(assetPath, resourceSourceType, onProgress);
                        return JsonUtility.FromJson<T>(jsonText);

                    case ResourceFileType.Bytes:
                        if (typeof(T) != typeof(byte[]))
                            throw new InvalidCastException($"Bytes资源必须使用byte[]类型，当前类型: {typeof(T)}");
                        var bytes = await LoadBytesInternal(assetPath, resourceSourceType, onProgress);
                        return (T)(object)bytes;

                    case ResourceFileType.AB:
                        throw new ArgumentException("AB类型资源请使用LoadBundleAssetAsync方法");

                    default:
                        throw new ArgumentException($"不支持的资源文件类型: {resourceFileType}");
                }
            }
            catch (Exception e)
            {
                Debugger.LogError($"资源加载失败: {assetPath}, 来源: {resourceSourceType}, 类型: {resourceFileType}, 错误: {e.Message}", LogType.FrameCore);
                return default(T);
            }
        }

        /// <summary>
        /// 同步资源加载方法
        /// </summary>
        public T LoadAssetSync<T>(
            string assetPath,
            ResourceSourceType resourceSourceType,
            ResourceFileType resourceFileType = ResourceFileType.UnityAsset)
        {
            try
            {
                // 同步加载只支持部分类型
                switch (resourceFileType)
                {
                    case ResourceFileType.UnityAsset when resourceSourceType == ResourceSourceType.Resources:
                        if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(T)))
                        {
                            var asset = Resources.Load(assetPath, typeof(T));
                            return (T)(object)asset;
                        }
                        break;

                    case ResourceFileType.Txt when resourceSourceType == ResourceSourceType.Resources:
                        var textAsset = Resources.Load<TextAsset>(assetPath);
                        if (textAsset != null && typeof(T) == typeof(string))
                            return (T)(object)textAsset.text;
                        break;

                    case ResourceFileType.Bytes when resourceSourceType == ResourceSourceType.Resources:
                        var bytesAsset = Resources.Load<TextAsset>(assetPath);
                        if (bytesAsset != null && typeof(T) == typeof(byte[]))
                            return (T)(object)bytesAsset.bytes;
                        break;
                }

                throw new NotSupportedException($"同步加载不支持此组合: 来源={resourceSourceType}, 类型={resourceFileType}");
            }
            catch (Exception e)
            {
                Debugger.LogError($"同步资源加载失败: {assetPath}, 错误: {e.Message}", LogType.FrameCore);
                return default(T);
            }
        }
        #endregion

        #region 智能快捷方法实现
        /// <summary>
        /// 智能加载Unity资源（自动推断文件类型）
        /// </summary>
        public async Task<T> LoadAsync<T>(string assetPath, ResourceSourceType resourceSourceType) where T : UnityEngine.Object
        {
            return await LoadAssetAsync<T>(assetPath, resourceSourceType, ResourceFileType.UnityAsset);
        }

        /// <summary>
        /// 智能加载文本
        /// </summary>
        public async Task<string> LoadTextAsync(string assetPath, ResourceSourceType resourceSourceType)
        {
            return await LoadAssetAsync<string>(assetPath, resourceSourceType, ResourceFileType.Txt);
        }

        /// <summary>
        /// 智能加载JSON
        /// </summary>
        public async Task<T> LoadJsonAsync<T>(string assetPath, ResourceSourceType resourceSourceType)
        {
            return await LoadAssetAsync<T>(assetPath, resourceSourceType, ResourceFileType.Json);
        }

        /// <summary>
        /// 智能加载二进制
        /// </summary>
        public async Task<byte[]> LoadBytesAsync(string assetPath, ResourceSourceType resourceSourceType)
        {
            return await LoadAssetAsync<byte[]>(assetPath, resourceSourceType, ResourceFileType.Bytes);
        }
        #endregion

        #region 极简快捷方法实现
        /// <summary>
        /// 从Resources加载资源（最常用）
        /// </summary>
        public async Task<T> LoadResourceAsync<T>(string assetPath) where T : UnityEngine.Object
        {
            return await LoadAsync<T>(assetPath, ResourceSourceType.Resources);
        }

        /// <summary>
        /// 从StreamingAssets加载文本配置
        /// </summary>
        public async Task<string> LoadConfigTextAsync(string configPath)
        {
            return await LoadTextAsync(configPath, ResourceSourceType.StreamingAssets);
        }

        /// <summary>
        /// 从PersistentData加载用户数据
        /// </summary>
        public async Task<T> LoadUserDataAsync<T>(string dataPath)
        {
            return await LoadJsonAsync<T>(dataPath, ResourceSourceType.PersistentData);
        }

        /// <summary>
        /// 从网络加载图片
        /// </summary>
        public async Task<Texture2D> LoadNetImageAsync(string url)
        {
            return await LoadAsync<Texture2D>(url, ResourceSourceType.Network);
        }

        /// <summary>
        /// 从AssetBundle加载资源
        /// </summary>
        public async Task<T> LoadBundleAssetAsync<T>(string bundleName, string assetPath) where T : UnityEngine.Object
        {
            return await LoadAssetBundleAssetAsync<T>(bundleName, assetPath);
        }
        #endregion

        #region 实例化快捷方法实现
        /// <summary>
        /// 实例化预制体（从Resources）
        /// </summary>
        public async Task<GameObject> InstantiateAsync(string assetPath, Transform parent = null)
        {
            var prefab = await LoadResourceAsync<GameObject>(assetPath);
            if (prefab == null)
            {
                Debugger.LogError($"实例化失败，预制体加载失败: {assetPath}", LogType.FrameCore);
                return null;
            }

            var instance = UnityEngine.Object.Instantiate(prefab, parent);
            _instanceToSourceMap[instance] = (assetPath, ResourceSourceType.Resources);

            Debugger.Log($"实例化成功: {assetPath}", LogType.FrameNormal);
            return instance;
        }

        /// <summary>
        /// 实例化预制体到指定位置
        /// </summary>
        public async Task<GameObject> InstantiateAsync(string assetPath, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            var prefab = await LoadResourceAsync<GameObject>(assetPath);
            if (prefab == null)
            {
                Debugger.LogError($"实例化失败，预制体加载失败: {assetPath}", LogType.FrameCore);
                return null;
            }

            var instance = UnityEngine.Object.Instantiate(prefab, position, rotation, parent);
            _instanceToSourceMap[instance] = (assetPath, ResourceSourceType.Resources);

            Debugger.Log($"实例化成功: {assetPath} 位置: {position}", LogType.FrameNormal);
            return instance;
        }
        #endregion

        #region AssetBundle管理实现
        /// <summary>
        /// 加载AssetBundle资源
        /// </summary>
        public async Task<T> LoadAssetBundleAssetAsync<T>(
            string bundlePath,
            string assetPath,
            Action<LoadProgress> onProgress = null) where T : UnityEngine.Object
        {
            var progress = new LoadProgress
            {
                Path = $"{bundlePath}/{assetPath}",
                Status = "开始加载AssetBundle资源"
            };

            var bundle = await LoadAssetBundleAsync(bundlePath, true, (bundleProgress) =>
            {
                progress.Progress = bundleProgress.Progress * 0.5f;
                progress.Status = $"加载AssetBundle: {bundleProgress.Progress:P}";
                onProgress?.Invoke(progress);
            });

            if (bundle == null) return null;

            var cacheKey = $"assetbundle://{bundlePath}/{assetPath}";

            lock (_lockObject)
            {
                if (_assetBundleHandles.TryGetValue(bundlePath, out var bundleHandle) &&
                    bundleHandle.LoadedAssets.TryGetValue(assetPath, out var assetHandle))
                {
                    assetHandle.ReferenceCount++;
                    assetHandle.LastAccessTime = DateTime.Now;
                    return assetHandle.Asset as T;
                }
            }

            try
            {
                var assetRequest = bundle.LoadAssetAsync<T>(assetPath);

                while (!assetRequest.isDone)
                {
                    progress.Progress = 0.5f + assetRequest.progress * 0.5f;
                    progress.Status = $"加载AssetBundle资源: {assetRequest.progress:P}";
                    onProgress?.Invoke(progress);
                    await Task.Yield();
                }

                progress.Progress = 1f;
                progress.IsDone = true;
                progress.Asset = assetRequest.asset;
                progress.Status = "AssetBundle资源加载完成";
                onProgress?.Invoke(progress);

                if (assetRequest.asset == null)
                {
                    throw new Exception($"AssetBundle资源不存在: {assetPath} in {bundlePath}");
                }

                var newAssetHandle = new ResourceHandle
                {
                    Asset = assetRequest.asset,
                    ReferenceCount = 1,
                    LastAccessTime = DateTime.Now,
                    Path = cacheKey,
                    SourceType = ResourceSourceType.AssetBundle,
                    FileType = ResourceFileType.UnityAsset,
                    MemorySize = EstimateMemorySize(assetRequest.asset),
                    AssetBundle = bundle
                };

                lock (_lockObject)
                {
                    if (_assetBundleHandles.TryGetValue(bundlePath, out var bundleHandle))
                    {
                        bundleHandle.LoadedAssets[assetPath] = newAssetHandle;
                    }
                    _resourceHandles[cacheKey] = newAssetHandle;
                }

                return assetRequest.asset as T;
            }
            catch (Exception e)
            {
                progress.Error = e;
                progress.IsDone = true;
                progress.Status = $"AssetBundle资源加载失败: {e.Message}";
                onProgress?.Invoke(progress);

                Debugger.LogError($"AssetBundle资源加载异常 {assetPath}: {e.Message}", LogType.FrameCore);
                return null;
            }
        }

        /// <summary>
        /// 加载AssetBundle
        /// </summary>
        public async Task<AssetBundle> LoadAssetBundleAsync(
            string bundlePath,
            bool fromStreamingAssets = true,
            Action<LoadProgress> onProgress = null)
        {
            var fullPath = fromStreamingAssets ?
                Path.Combine(Application.streamingAssetsPath, bundlePath) :
                bundlePath;

            var progress = new LoadProgress
            {
                Path = bundlePath,
                Status = "开始加载AssetBundle"
            };

            lock (_lockObject)
            {
                if (_assetBundleHandles.TryGetValue(bundlePath, out var handle))
                {
                    handle.ReferenceCount++;
                    handle.LastAccessTime = DateTime.Now;
                    return handle.Bundle;
                }
            }

            try
            {
                var bundleRequest = AssetBundle.LoadFromFileAsync(fullPath);

                while (!bundleRequest.isDone)
                {
                    progress.Progress = bundleRequest.progress;
                    progress.Status = $"加载AssetBundle: {progress.Progress:P}";
                    onProgress?.Invoke(progress);
                    await Task.Yield();
                }

                progress.Progress = 1f;
                progress.IsDone = true;
                progress.Status = "AssetBundle加载完成";
                onProgress?.Invoke(progress);

                if (bundleRequest.assetBundle == null)
                {
                    throw new Exception($"AssetBundle加载失败: {bundlePath}");
                }

                var newHandle = new AssetBundleHandle
                {
                    Bundle = bundleRequest.assetBundle,
                    ReferenceCount = 1,
                    LastAccessTime = DateTime.Now,
                    BundlePath = bundlePath
                };

                lock (_lockObject)
                {
                    _assetBundleHandles[bundlePath] = newHandle;
                }

                Debugger.Log($"AssetBundle加载成功: {bundlePath}", LogType.FrameNormal);
                return bundleRequest.assetBundle;
            }
            catch (Exception e)
            {
                progress.Error = e;
                progress.IsDone = true;
                progress.Status = $"AssetBundle加载失败: {e.Message}";
                onProgress?.Invoke(progress);

                Debugger.LogError($"AssetBundle加载异常 {bundlePath}: {e.Message}", LogType.FrameCore);
                return null;
            }
        }

        /// <summary>
        /// 卸载AssetBundle
        /// </summary>
        public void UnloadAssetBundle(string bundlePath, bool unloadAllObjects = false)
        {
            lock (_lockObject)
            {
                if (_assetBundleHandles.TryGetValue(bundlePath, out var handle))
                {
                    handle.ReferenceCount--;

                    if (handle.ReferenceCount <= 0)
                    {
                        handle.Bundle.Unload(unloadAllObjects);
                        _assetBundleHandles.Remove(bundlePath);
                        Debugger.Log($"AssetBundle已卸载: {bundlePath}", LogType.FrameNormal);
                    }
                }
            }
        }
        #endregion

        #region 网络资源管理实现
        /// <summary>
        /// 下载文件到本地
        /// </summary>
        public async Task DownloadFileAsync(
            string url,
            string localPath,
            Action<LoadProgress> onProgress = null)
        {
            var progress = new LoadProgress
            {
                Path = url,
                Status = "开始下载文件"
            };

            try
            {
                using (var webRequest = UnityEngine.Networking.UnityWebRequest.Get(url))
                {
                    var operation = webRequest.SendWebRequest();

                    while (!operation.isDone)
                    {
                        progress.Progress = operation.progress;
                        progress.Status = $"下载文件: {progress.Progress:P}";

                        if (webRequest.downloadHandler != null)
                        {
                            progress.LoadedBytes = (long)webRequest.downloadedBytes;
                            progress.TotalBytes = (long)webRequest.downloadHandler.data.Length;
                        }

                        onProgress?.Invoke(progress);
                        await Task.Yield();
                    }

                    progress.Progress = 1f;
                    progress.IsDone = true;

                    if (webRequest.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
                    {
                        throw new Exception($"文件下载失败: {webRequest.error}");
                    }

                    var directory = Path.GetDirectoryName(localPath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    await File.WriteAllBytesAsync(localPath, webRequest.downloadHandler.data);

                    progress.Status = "文件下载完成";
                    progress.Bytes = webRequest.downloadHandler.data;
                    onProgress?.Invoke(progress);

                    Debugger.Log($"文件下载完成: {url} -> {localPath}", LogType.FrameNormal);
                }
            }
            catch (Exception e)
            {
                progress.Error = e;
                progress.IsDone = true;
                progress.Status = $"文件下载失败: {e.Message}";
                onProgress?.Invoke(progress);

                Debugger.LogError($"文件下载异常 {url}: {e.Message}", LogType.FrameCore);
            }
        }

        /// <summary>
        /// 检查文件是否已缓存
        /// </summary>
        public Task<bool> IsFileCached(string url)
        {
            var cachedPath = GetCachedFilePath(url);
            return Task.FromResult(File.Exists(cachedPath));
        }
        #endregion

        #region 资源卸载管理实现
        /// <summary>
        /// 卸载资源
        /// </summary>
        public void Unload(string assetPath, ResourceSourceType resourceSourceType)
        {
            var cacheKey = $"{resourceSourceType.ToString().ToLower()}://{assetPath}";

            lock (_lockObject)
            {
                if (_resourceHandles.TryGetValue(cacheKey, out var handle))
                {
                    handle.ReferenceCount--;
                    handle.LastAccessTime = DateTime.Now;

                    if (handle.ReferenceCount <= 0)
                    {
                        Debugger.Log($"资源引用计数为0，等待自动清理: {assetPath}", LogType.FrameNormal);
                    }
                    else
                    {
                        Debugger.Log($"资源引用计数减少: {assetPath} -> {handle.ReferenceCount}", LogType.FrameNormal);
                    }
                }
                else
                {
                    Debugger.LogWarning($"尝试卸载未加载的资源: {assetPath}", LogType.FrameNormal);
                }
            }
        }

        /// <summary>
        /// 卸载游戏对象实例
        /// </summary>
        public void Unload(GameObject instance)
        {
            if (_instanceToSourceMap.TryGetValue(instance, out var sourceInfo))
            {
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(instance);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(instance);
                }
                _instanceToSourceMap.Remove(instance);
                Unload(sourceInfo.path, sourceInfo.sourceType);

                Debugger.Log($"游戏对象实例已卸载: {sourceInfo.path}", LogType.FrameNormal);
            }
            else
            {
                Debugger.LogWarning($"尝试卸载未注册的游戏对象实例", LogType.FrameNormal);
            }
        }

        /// <summary>
        /// 强制清理所有未使用的资源
        /// </summary>
        public void UnloadUnusedAssets()
        {
            int unloadedCount = 0;

            lock (_lockObject)
            {
                // 清理未使用的资源
                var toRemove = _resourceHandles.Where(kvp => kvp.Value.ReferenceCount <= 0).ToList();
                foreach (var kvp in toRemove)
                {
                    if (kvp.Value.Asset is UnityEngine.Object unityObj && kvp.Value.AssetBundle == null)
                    {
                        if (Application.isPlaying)
                        {
                            UnityEngine.Object.Destroy(unityObj);
                        }
                        else
                        {
                            UnityEngine.Object.DestroyImmediate(unityObj);
                        }
                    }
                    _resourceHandles.Remove(kvp.Key);
                    unloadedCount++;
                }

                // 清理未使用的AssetBundle
                var bundlesToRemove = _assetBundleHandles.Where(kvp => kvp.Value.ReferenceCount <= 0).ToList();
                foreach (var kvp in bundlesToRemove)
                {
                    kvp.Value.Bundle.Unload(true);
                    _assetBundleHandles.Remove(kvp.Key);
                    unloadedCount++;
                }
            }

            Resources.UnloadUnusedAssets();

            if (unloadedCount > 0)
            {
                Debugger.Log($"强制清理完成，卸载了 {unloadedCount} 个未使用资源", LogType.FrameNormal);
            }
            else
            {
                Debugger.Log("没有需要清理的未使用资源", LogType.FrameNormal);
            }
        }
        #endregion

        #region 预加载和批量操作实现
        /// <summary>
        /// 预加载多个资源
        /// </summary>
        public async Task PreloadAsync(
            List<string> assetPaths,
            ResourceSourceType resourceSourceType = ResourceSourceType.Resources,
            Action<LoadProgress> onProgress = null)
        {
            var progress = new LoadProgress
            {
                Status = "开始预加载资源"
            };

            Debugger.Log($"开始预加载 {assetPaths.Count} 个资源", LogType.FrameNormal);

            var loadTasks = new List<Task>();
            int completedCount = 0;

            foreach (var path in assetPaths)
            {
                var task = LoadAssetAsync<UnityEngine.Object>(
                    path,
                    resourceSourceType,
                    ResourceFileType.UnityAsset,
                    (itemProgress) =>
                    {
                        // 计算总体进度
                        float individualProgress = itemProgress.IsDone ? 1f : itemProgress.Progress;
                        float totalProgress = (float)(completedCount + individualProgress) / assetPaths.Count;

                        progress.Progress = totalProgress;
                        progress.Status = $"预加载进度: {completedCount}/{assetPaths.Count} ({totalProgress:P}) - {path}";
                        onProgress?.Invoke(progress);
                    }).ContinueWith(t =>
                    {
                        completedCount++;
                        return t;
                    });

                loadTasks.Add(task);
            }

            await Task.WhenAll(loadTasks);

            progress.Progress = 1f;
            progress.IsDone = true;
            progress.Status = "预加载完成";
            onProgress?.Invoke(progress);

            Debugger.Log($"预加载完成: {assetPaths.Count} 个资源", LogType.FrameNormal);
        }
        #endregion

        #region 缓存管理实现
        /// <summary>
        /// 清理指定来源的缓存
        /// </summary>
        public void ClearCache(ResourceSourceType resourceSourceType)
        {
            int clearedCount = 0;

            lock (_lockObject)
            {
                var toRemove = _resourceHandles.Where(kvp => kvp.Value.SourceType == resourceSourceType).ToList();
                foreach (var kvp in toRemove)
                {
                    if (kvp.Value.Asset is UnityEngine.Object unityObj && kvp.Value.AssetBundle == null)
                    {
                        if (Application.isPlaying)
                        {
                            UnityEngine.Object.Destroy(unityObj);
                        }
                        else
                        {
                            UnityEngine.Object.DestroyImmediate(unityObj);
                        }
                    }
                    _resourceHandles.Remove(kvp.Key);
                    clearedCount++;
                }
            }

            if (resourceSourceType == ResourceSourceType.Network)
            {
                // 清理网络缓存文件
                if (Directory.Exists(_networkCacheDirectory))
                {
                    var files = Directory.GetFiles(_networkCacheDirectory);
                    foreach (var file in files)
                    {
                        File.Delete(file);
                    }
                    clearedCount += files.Length;
                    Debugger.Log($"清理了 {files.Length} 个网络缓存文件", LogType.FrameNormal);
                }
            }

            Debugger.Log($"清理了 {clearedCount} 个 {resourceSourceType} 缓存资源", LogType.FrameNormal);
        }

        /// <summary>
        /// 获取缓存大小
        /// </summary>
        public long GetCacheSize(ResourceSourceType resourceSourceType)
        {
            lock (_lockObject)
            {
                return _resourceHandles.Values
                    .Where(h => h.SourceType == resourceSourceType)
                    .Sum(h => h.MemorySize);
            }
        }
        #endregion

        #region 内部加载实现
        private async Task<T> LoadUnityAssetInternal<T>(
            string assetPath,
            ResourceSourceType resourceSourceType,
            Action<LoadProgress> onProgress = null)
        {
            var cacheKey = $"{resourceSourceType.ToString().ToLower()}://{assetPath}";

            lock (_lockObject)
            {
                if (_resourceHandles.TryGetValue(cacheKey, out var handle) && handle.Asset is T)
                {
                    handle.ReferenceCount++;
                    handle.LastAccessTime = DateTime.Now;
                    return (T)handle.Asset;
                }
            }

            var progress = new LoadProgress
            {
                Path = assetPath,
                Status = $"开始加载{resourceSourceType}资源"
            };

            try
            {
                object result = null;

                switch (resourceSourceType)
                {
                    case ResourceSourceType.Resources:
                        result = await LoadFromResourcesAsync<T>(assetPath, progress, onProgress);
                        break;

                    case ResourceSourceType.StreamingAssets:
                        result = await LoadFromStreamingAssetsAsync<T>(assetPath, progress, onProgress);
                        break;

                    case ResourceSourceType.PersistentData:
                        result = await LoadFromPersistentDataAsync<T>(assetPath, progress, onProgress);
                        break;

                    case ResourceSourceType.Network:
                        result = await LoadFromNetworkAsync<T>(assetPath, progress, onProgress);
                        break;

                    case ResourceSourceType.AssetBundle:
                        throw new ArgumentException("AssetBundle资源请使用LoadBundleAssetAsync方法");

                    case ResourceSourceType.Addressables:
                        throw new NotSupportedException("Addressables暂未实现");

                    default:
                        throw new ArgumentException($"不支持的资源来源类型: {resourceSourceType}");
                }

                if (result != null)
                {
                    var newHandle = new ResourceHandle
                    {
                        Asset = result,
                        ReferenceCount = 1,
                        LastAccessTime = DateTime.Now,
                        Path = cacheKey,
                        SourceType = resourceSourceType,
                        FileType = ResourceFileType.UnityAsset,
                        MemorySize = EstimateMemorySize(result as UnityEngine.Object)
                    };

                    lock (_lockObject)
                    {
                        _resourceHandles[cacheKey] = newHandle;
                    }
                }

                return (T)result;
            }
            catch (Exception e)
            {
                progress.Error = e;
                progress.IsDone = true;
                progress.Status = $"资源加载失败: {e.Message}";
                onProgress?.Invoke(progress);

                Debugger.LogError($"{resourceSourceType}资源加载失败 {assetPath}: {e.Message}", LogType.FrameCore);
                return default(T);
            }
        }

        private async Task<string> LoadTextInternal(
            string assetPath,
            ResourceSourceType resourceSourceType,
            Action<LoadProgress> onProgress = null)
        {
            var progress = new LoadProgress
            {
                Path = assetPath,
                Status = $"开始加载{resourceSourceType}文本"
            };

            try
            {
                string result = null;

                switch (resourceSourceType)
                {
                    case ResourceSourceType.Resources:
                        var textAsset = await LoadFromResourcesAsync<TextAsset>(assetPath, progress, onProgress);
                        result = textAsset?.text;
                        break;

                    case ResourceSourceType.StreamingAssets:
                        var streamingPath = Path.Combine(Application.streamingAssetsPath, assetPath);
                        if (File.Exists(streamingPath))
                            result = await File.ReadAllTextAsync(streamingPath);
                        break;

                    case ResourceSourceType.PersistentData:
                        var persistentPath = Path.Combine(Application.persistentDataPath, assetPath);
                        if (File.Exists(persistentPath))
                            result = await File.ReadAllTextAsync(persistentPath);
                        break;

                    case ResourceSourceType.Network:
                        var bytes = await LoadBytesFromNetworkAsync(assetPath, progress, onProgress);
                        result = bytes != null ? System.Text.Encoding.UTF8.GetString(bytes) : null;
                        break;

                    default:
                        throw new ArgumentException($"文本加载不支持此来源: {resourceSourceType}");
                }

                progress.Progress = 1f;
                progress.IsDone = true;
                progress.Status = "文本加载完成";
                progress.Text = result;
                onProgress?.Invoke(progress);

                return result;
            }
            catch (Exception e)
            {
                progress.Error = e;
                progress.IsDone = true;
                progress.Status = $"文本加载失败: {e.Message}";
                onProgress?.Invoke(progress);

                Debugger.LogError($"文本加载失败 {assetPath}: {e.Message}", LogType.FrameCore);
                return null;
            }
        }

        private async Task<byte[]> LoadBytesInternal(
            string assetPath,
            ResourceSourceType resourceSourceType,
            Action<LoadProgress> onProgress = null)
        {
            var progress = new LoadProgress
            {
                Path = assetPath,
                Status = $"开始加载{resourceSourceType}字节数据"
            };

            try
            {
                byte[] result = null;

                switch (resourceSourceType)
                {
                    case ResourceSourceType.Resources:
                        var textAsset = await LoadFromResourcesAsync<TextAsset>(assetPath, progress, onProgress);
                        result = textAsset?.bytes;
                        break;

                    case ResourceSourceType.StreamingAssets:
                        var streamingPath = Path.Combine(Application.streamingAssetsPath, assetPath);
                        if (File.Exists(streamingPath))
                            result = await File.ReadAllBytesAsync(streamingPath);
                        break;

                    case ResourceSourceType.PersistentData:
                        var persistentPath = Path.Combine(Application.persistentDataPath, assetPath);
                        if (File.Exists(persistentPath))
                            result = await File.ReadAllBytesAsync(persistentPath);
                        break;

                    case ResourceSourceType.Network:
                        result = await LoadBytesFromNetworkAsync(assetPath, progress, onProgress);
                        break;

                    default:
                        throw new ArgumentException($"字节加载不支持此来源: {resourceSourceType}");
                }

                progress.Progress = 1f;
                progress.IsDone = true;
                progress.Status = "字节数据加载完成";
                progress.Bytes = result;
                onProgress?.Invoke(progress);

                return result;
            }
            catch (Exception e)
            {
                progress.Error = e;
                progress.IsDone = true;
                progress.Status = $"字节数据加载失败: {e.Message}";
                onProgress?.Invoke(progress);

                Debugger.LogError($"字节数据加载失败 {assetPath}: {e.Message}", LogType.FrameCore);
                return null;
            }
        }
        #endregion

        #region 具体来源加载实现
        private async Task<T> LoadFromResourcesAsync<T>(
            string assetPath,
            LoadProgress progress,
            Action<LoadProgress> onProgress) where T : UnityEngine.Object
        {
            var resourceRequest = Resources.LoadAsync<T>(assetPath);

            while (!resourceRequest.isDone)
            {
                progress.Progress = resourceRequest.progress;
                progress.Status = $"加载Resources资源: {progress.Progress:P}";
                onProgress?.Invoke(progress);
                await Task.Yield();
            }

            progress.Progress = 1f;
            progress.IsDone = true;
            progress.Asset = resourceRequest.asset;
            progress.Status = "Resources资源加载完成";
            onProgress?.Invoke(progress);

            if (resourceRequest.asset == null)
            {
                throw new Exception($"Resources资源不存在: {assetPath}");
            }

            return resourceRequest.asset as T;
        }

        private async Task<T> LoadFromStreamingAssetsAsync<T>(
            string assetPath,
            LoadProgress progress,
            Action<LoadProgress> onProgress) where T : UnityEngine.Object
        {
            var fullPath = Path.Combine(Application.streamingAssetsPath, assetPath);
            return await LoadFromFileAsync<T>(fullPath, progress, onProgress);
        }

        private async Task<T> LoadFromPersistentDataAsync<T>(
            string assetPath,
            LoadProgress progress,
            Action<LoadProgress> onProgress) where T : UnityEngine.Object
        {
            var fullPath = Path.Combine(Application.persistentDataPath, assetPath);
            return await LoadFromFileAsync<T>(fullPath, progress, onProgress);
        }

        private async Task<T> LoadFromFileAsync<T>(
            string fullPath,
            LoadProgress progress,
            Action<LoadProgress> onProgress) where T : UnityEngine.Object
        {
            if (!File.Exists(fullPath))
            {
                throw new Exception($"文件不存在: {fullPath}");
            }

            progress.Status = "读取文件数据";
            onProgress?.Invoke(progress);

            var bytes = await File.ReadAllBytesAsync(fullPath);

            progress.Progress = 0.5f;
            progress.Status = "处理文件数据";
            onProgress?.Invoke(progress);

            // 根据类型创建Unity对象
            if (typeof(T) == typeof(Texture2D))
            {
                var texture = new Texture2D(2, 2);
                texture.LoadImage(bytes);
                return texture as T;
            }
            else if (typeof(T) == typeof(AudioClip))
            {
                // 简化实现，实际项目需要根据音频格式处理
                return null;
            }
            else if (typeof(T) == typeof(TextAsset))
            {
                var text = System.Text.Encoding.UTF8.GetString(bytes);
                var textAsset = new TextAsset(text);
                return textAsset as T;
            }

            throw new Exception($"不支持的Unity资源类型: {typeof(T)}");
        }

        private async Task<T> LoadFromNetworkAsync<T>(
            string url,
            LoadProgress progress,
            Action<LoadProgress> onProgress) where T : UnityEngine.Object
        {
            // 检查缓存
            var cachedPath = GetCachedFilePath(url);
            if (File.Exists(cachedPath))
            {
                Debugger.Log($"使用缓存资源: {url}", LogType.FrameNormal);
                progress.Status = "使用缓存资源";
                onProgress?.Invoke(progress);
                return await LoadFromPersistentDataAsync<T>(cachedPath, progress, onProgress);
            }

            var bytes = await LoadBytesFromNetworkAsync(url, progress, onProgress);
            if (bytes == null) return null;

            // 根据类型创建Unity对象
            if (typeof(T) == typeof(Texture2D))
            {
                var texture = new Texture2D(2, 2);
                texture.LoadImage(bytes);
                return texture as T;
            }
            else if (typeof(T) == typeof(AudioClip))
            {
                // 简化实现
                return null;
            }
            else if (typeof(T) == typeof(TextAsset))
            {
                var text = System.Text.Encoding.UTF8.GetString(bytes);
                var textAsset = new TextAsset(text);
                return textAsset as T;
            }

            throw new Exception($"不支持的Unity资源类型: {typeof(T)}");
        }

        private async Task<byte[]> LoadBytesFromNetworkAsync(
            string url,
            LoadProgress progress,
            Action<LoadProgress> onProgress)
        {
            using (var webRequest = UnityEngine.Networking.UnityWebRequest.Get(url))
            {
                var operation = webRequest.SendWebRequest();

                while (!operation.isDone)
                {
                    progress.Progress = operation.progress;
                    progress.Status = $"下载资源: {progress.Progress:P}";

                    if (webRequest.downloadHandler != null)
                    {
                        progress.LoadedBytes = (long)webRequest.downloadedBytes;
                        progress.TotalBytes = (long)webRequest.downloadHandler.data.Length;
                    }

                    onProgress?.Invoke(progress);
                    await Task.Yield();
                }

                progress.Progress = 1f;
                progress.IsDone = true;

                if (webRequest.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    throw new Exception($"网络资源加载失败: {webRequest.error}");
                }

                var data = webRequest.downloadHandler.data;

                // 保存到缓存
                var cachedPath = GetCachedFilePath(url);
                await File.WriteAllBytesAsync(cachedPath, data);

                progress.Status = "网络资源下载完成";
                progress.Bytes = data;
                onProgress?.Invoke(progress);

                return data;
            }
        }
        #endregion

        #region 工具方法和生命周期
        /// <summary>
        /// 每帧更新
        /// </summary>
        public void OnUpdate(float deltaTime)
        {
            _lastCleanupTime += deltaTime;
            if (_lastCleanupTime >= _cacheCleanupInterval)
            {
                _lastCleanupTime = 0f;
                CleanupUnusedResources();
            }
        }

        private void CleanupUnusedResources()
        {
            lock (_lockObject)
            {
                var toRemove = new List<string>();
                var currentTime = DateTime.Now;
                long totalMemory = _resourceHandles.Values.Sum(h => h.MemorySize);

                // 内存超限，强制清理
                if (totalMemory > _maxMemorySize)
                {
                    Debugger.LogWarning($"内存使用超限: {totalMemory}/{_maxMemorySize}, 强制清理", LogType.FrameNormal);
                    ForceCleanup();
                    return;
                }

                // 常规清理：引用为0且长时间未访问的资源
                foreach (var kvp in _resourceHandles)
                {
                    var handle = kvp.Value;

                    if (handle.ReferenceCount <= 0 &&
                        (currentTime - handle.LastAccessTime).TotalMinutes > 5)
                    {
                        toRemove.Add(kvp.Key);
                    }
                }

                foreach (var path in toRemove)
                {
                    if (_resourceHandles.TryGetValue(path, out var handle))
                    {
                        if (handle.Asset is UnityEngine.Object unityObj && handle.AssetBundle == null)
                        {
                            if (Application.isPlaying)
                            {
                                UnityEngine.Object.Destroy(unityObj);
                            }
                            else
                            {
                                UnityEngine.Object.DestroyImmediate(unityObj);
                            }
                        }
                        _resourceHandles.Remove(path);
                    }
                }

                if (toRemove.Count > 0)
                {
                    Debugger.Log($"自动清理了 {toRemove.Count} 个未使用资源", LogType.FrameNormal);
                }
            }
        }

        private void ForceCleanup()
        {
            lock (_lockObject)
            {
                var toRemove = _resourceHandles
                    .Where(kvp => kvp.Value.ReferenceCount <= 0)
                    .OrderBy(kvp => kvp.Value.LastAccessTime)
                    .Take(10) // 一次清理10个
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var path in toRemove)
                {
                    if (_resourceHandles.TryGetValue(path, out var handle))
                    {
                        if (handle.Asset is UnityEngine.Object unityObj && handle.AssetBundle == null)
                        {
                            if (Application.isPlaying)
                            {
                                UnityEngine.Object.Destroy(unityObj);
                            }
                            else
                            {
                                UnityEngine.Object.DestroyImmediate(unityObj);
                            }
                        }
                        _resourceHandles.Remove(path);
                    }
                }
            }
        }

        private long EstimateMemorySize(UnityEngine.Object obj)
        {
            if (obj == null) return 0;

            if (obj is Texture2D texture) return texture.width * texture.height * 4;
            if (obj is Mesh mesh) return (mesh.vertices.Length * 12) + (mesh.triangles.Length * 4);
            if (obj is AudioClip audio) return (long)(audio.samples * audio.channels * 4);
            return 1024; // 默认1KB
        }

        private string GetCachedFilePath(string url)
        {
            var fileName = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(url))
                .Replace("/", "_")
                .Replace("+", "-")
                .Replace("=", "");
            return Path.Combine(_networkCacheDirectory, fileName);
        }

        /// <summary>
        /// 打印资源状态信息
        /// </summary>
        public void PrintResourceStatus()
        {
            int totalResources = _resourceHandles.Count;
            int usedResources = _resourceHandles.Count(kvp => kvp.Value.ReferenceCount > 0);
            long totalMemory = _resourceHandles.Values.Sum(h => h.MemorySize);

            Debugger.Log($"资源状态: 总数{totalResources}, 使用中{usedResources}, 内存{totalMemory / 1024 / 1024}MB", LogType.FrameNormal);
        }
        #endregion
    }
}