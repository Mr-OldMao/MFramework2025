using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace MFramework.Runtime
{
    /// <summary>
    /// 多功能资源管理器
    /// 支持从Resources、AssetBundle、StreamingAssets、PersistentData和网络加载资源
    /// 提供引用计数、缓存管理、进度回调和内存优化功能
    /// </summary>
    public class ResourceManager : GameModuleBase, IUpdatableModule, IResourceManager
    {
        #region 对外接口

        /// <summary>
        /// 异步加载Unity资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">资源相对路径。Resources不用加后缀名，StreamingAssets、PersistentData需要加后缀名</param>
        /// <param name="source">不支持从StreamingAssets、PersistentData加载部分Unity资源，包括但是不限于.audioClip,.prefab</param>
        /// <param name="onProgress"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<T> LoadAsync<T>(string path, ResourceSource source, Action<LoadProgress> onProgress = null) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(path))
            {
                Debugger.LogError("资源路径为空", LogType.FrameCore);
                return null;
            }
            if (source == ResourceSource.StreamingAssets || source == ResourceSource.PersistentData)
            {
                if (path.Split('.').Length == 1)
                {
                    Debugger.LogError($"source:{source},需要加后缀名", LogType.FrameCore);
                    return null;
                }
            }


            return source switch
            {
                ResourceSource.Resources => await LoadFromResourcesAsync<T>(path, onProgress),
                ResourceSource.StreamingAssets => await LoadFromStreamingAssetsAsync<T>(path, onProgress),
                ResourceSource.PersistentData => await LoadFromPersistentDataAsync<T>(path, onProgress),
                ResourceSource.Network => await LoadFromNetworkAsync<T>(path, onProgress),
                _ => throw new ArgumentException($"不支持的资源来源: {source}")
            };
        }

        #region 文本与字节流加载

        /// <summary>
        /// 异步加载字节数据
        /// </summary>
        public async Task<byte[]> LoadBytesAsync(string path, ResourceSource source, Action<LoadProgress> onProgress = null)
        {
            if (source == ResourceSource.StreamingAssets || source == ResourceSource.PersistentData)
            {
                if (path.Split('.').Length == 1)
                {
                    Debugger.LogError($"source:{source},需要加后缀名", LogType.FrameCore);
                    return null;
                }
            }

            var progress = new LoadProgress
            {
                Path = path,
                Status = "开始加载字节数据"
            };

            try
            {
                byte[] result = null;

                switch (source)
                {
                    case ResourceSource.StreamingAssets:
                        var streamingPath = Path.Combine(Application.streamingAssetsPath, path);
                        result = await ReadFileBytesAsync(streamingPath, progress, onProgress);
                        break;

                    case ResourceSource.PersistentData:
                        var persistentPath = Path.Combine(Application.persistentDataPath, path);
                        result = await ReadFileBytesAsync(persistentPath, progress, onProgress);
                        break;

                    case ResourceSource.Network:
                        result = await DownloadBytesAsync(path, progress, onProgress);
                        break;

                    case ResourceSource.Resources:
                        result = await LoadBytesFromResourcesAsync(path, progress, onProgress);
                        break;

                    default:
                        throw new ArgumentException($"不支持的资源来源: {source}");
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

                Debugger.LogError($"LoadBytesAsync 失败: {path}, 来源: {source}, 错误: {e.Message}", LogType.FrameCore);
                return null;
            }
        }

        /// <summary>
        /// 异步加载文本数据
        /// </summary>
        public async Task<string> LoadTextAsync(string path, ResourceSource source, Action<LoadProgress> onProgress = null)
        {
            if (source == ResourceSource.StreamingAssets || source == ResourceSource.PersistentData)
            {
                if (path.Split('.').Length == 1)
                {
                    Debugger.LogError($"source:{source},需要加后缀名", LogType.FrameCore);
                    return null;
                }
            }

            var progress = new LoadProgress
            {
                Path = path,
                Status = "开始加载文本数据"
            };

            try
            {
                var bytes = await LoadBytesAsync(path, source, (byteProgress) =>
                {
                    // 转换字节加载进度为文本加载进度
                    progress.Progress = byteProgress.Progress;
                    progress.Status = byteProgress.Status;
                    onProgress?.Invoke(progress);
                });

                if (bytes == null) return null;

                progress.Progress = 1f;
                progress.IsDone = true;
                progress.Status = "文本数据加载完成";

                var text = System.Text.Encoding.UTF8.GetString(bytes);
                progress.Text = text;
                onProgress?.Invoke(progress);

                return text;
            }
            catch (Exception e)
            {
                progress.Error = e;
                progress.IsDone = true;
                progress.Status = $"文本数据加载失败: {e.Message}";
                onProgress?.Invoke(progress);

                Debugger.LogError($"LoadTextAsync 失败: {path}, 来源: {source}, 错误: {e.Message}", LogType.FrameCore);
                return null;
            }
        }
        #endregion

        #region AssetBundle资源加载



        /// <summary>
        /// 从AssetBundle异步加载资源
        /// </summary>
        public async Task<T> LoadFromAssetBundleAsync<T>(string bundlePath, string assetPath, ResourceSource resourceSource, Action<LoadProgress> onProgress = null) where T : UnityEngine.Object
        {
            var progress = new LoadProgress
            {
                Path = $"{bundlePath}/{assetPath}",
                Status = "开始加载AssetBundle资源"
            };

            var bundle = await LoadAssetBundleAsync<T>(bundlePath, assetPath, resourceSource, (bundleProgress) =>
            {
                // 转换AssetBundle加载进度为资源加载进度
                progress.Progress = bundleProgress.Progress * 0.5f; // AssetBundle加载占50%
                progress.Status = $"加载AssetBundle: {bundleProgress.Progress:P}";
                onProgress?.Invoke(progress);
            });

            if (bundle == null) return null;

            var cacheKey = $"assetbundle://{bundlePath}/{assetPath}";

            lock (_lockObject)
            {
                // 检查AB包内资源的缓存
                if (_assetBundleHandles.TryGetValue(bundlePath, out var bundleHandle))
                {
                    if (typeof(T) == typeof(AssetBundle))
                    {
                        return bundleHandle.Bundle as T;
                    }
                    else
                    {
                        if (bundleHandle.LoadedAssets.TryGetValue(assetPath, out var assetHandle))
                        {

                            assetHandle.ReferenceCount++;
                            assetHandle.LastAccessTime = DateTime.Now;
                            return assetHandle.Asset as T;
                        }
                    }
                }


                //// 检查AB包内资源的缓存
                //if (_assetBundleHandles.TryGetValue(bundlePath, out var bundleHandle) &&
                //    bundleHandle.LoadedAssets.TryGetValue(assetPath, out var assetHandle))
                //{
                //    assetHandle.ReferenceCount++;
                //    assetHandle.LastAccessTime = DateTime.Now;
                //    if (typeof(T) == typeof(AssetBundle))
                //    {
                //        return assetHandle.AssetBundle as T;
                //    }
                //    else
                //    {
                //        return assetHandle.Asset as T;
                //    }
                //}
            }

            try
            {
                var assetRequest = bundle.LoadAssetAsync<T>(assetPath);

                // 资源加载进度（占50%）
                while (!assetRequest.isDone)
                {
                    progress.Progress = 0.5f + assetRequest.progress * 0.5f;
                    progress.Status = $"加载AssetBundle资源1: {assetRequest.progress:P}";
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
                    Source = resourceSource,
                    MemorySize = EstimateMemorySize(assetRequest.asset),
                    AssetBundle = bundle
                };

                lock (_lockObject)
                {
                    if (_assetBundleHandles.TryGetValue(bundlePath, out var bundleHandle))
                    {
                        bundleHandle.LoadedAssets[bundlePath] = newAssetHandle;
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
        /// 异步加载AssetBundle
        /// </summary>
        public async Task<AssetBundle> LoadAssetBundleAsync<T>(string bundlePath, string assetPath, ResourceSource resourceSource, Action<LoadProgress> onProgress = null) where T : UnityEngine.Object
        {
            string fullPath = string.Empty;
            switch (resourceSource)
            {
                case ResourceSource.Resources:
                    fullPath = Path.Combine(Application.dataPath, $"Resources/{bundlePath}");
                    break;
                case ResourceSource.StreamingAssets:
                    fullPath = Path.Combine(Application.streamingAssetsPath, $"{bundlePath}");
                    break;
                case ResourceSource.PersistentData:
                    fullPath = Path.Combine(Application.persistentDataPath, $"{bundlePath}");
                    break;
                default:
                    Debugger.LogError($"不支持的资源源: {resourceSource}", LogType.FrameCore);
                    break;
            }
            var progress = new LoadProgress
            {
                Path = fullPath,
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
                    throw new Exception($"AssetBundle加载失败: {fullPath}");
                }

                var newHandle = new AssetBundleHandle
                {
                    Bundle = bundleRequest.assetBundle,
                    ReferenceCount = 1,
                    LastAccessTime = DateTime.Now,
                    BundlePath = bundlePath,
                    FilePath = $"{bundlePath}/{assetPath}",
                    LoadedAssets = new Dictionary<string, ResourceHandle>()
                };
                ResourceHandle resourceHandle = new ResourceHandle
                {
                    Asset = null,
                    ReferenceCount = 0,
                    LastAccessTime = DateTime.Now,
                    Path = assetPath,
                    AssetBundle = bundleRequest.assetBundle,
                    MemorySize = EstimateMemorySize(bundleRequest.assetBundle),
                    Source = resourceSource
                };
                if (!string.IsNullOrEmpty(assetPath))
                {
                    var assetRequest = bundleRequest.assetBundle.LoadAssetAsync<T>(assetPath);
                    //等待异步加载资源完成
                    while (!assetRequest.isDone)
                    {
                        progress.Progress = assetRequest.progress;
                        await Task.Yield();
                    }
                    resourceHandle.Asset = assetRequest.asset;
                }
                newHandle.LoadedAssets.Add(bundlePath, resourceHandle);


                lock (_lockObject)
                {
                    _assetBundleHandles[bundlePath] = newHandle;
                }

                Debugger.Log($"AssetBundle加载成功: {fullPath}", LogType.FrameNormal);
                return bundleRequest.assetBundle;
            }
            catch (Exception e)
            {
                progress.Error = e;
                progress.IsDone = true;
                progress.Status = $"AssetBundle加载失败: {e.Message}";
                onProgress?.Invoke(progress);

                Debugger.LogError($"AssetBundle加载异常 {fullPath}: {e.Message}", LogType.FrameCore);
                return null;
            }
        }

        /// <summary>
        /// 卸载AssetBundle
        /// </summary>
        public void UnloadAssetBundle(string bundlePath, string assetPath, bool unloadAllObjects = false)
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

        #region 网络资源加载

        /// <summary>
        /// 从网络异步加载资源
        /// </summary>
        private async Task<T> LoadFromNetworkAsync<T>(string url, Action<LoadProgress> onProgress = null) where T : UnityEngine.Object
        {
            var progress = new LoadProgress
            {
                Path = url,
                Status = "开始下载网络资源"
            };

            // 检查本地缓存
            var cachedPath = GetCachedFilePath(url);
            if (File.Exists(cachedPath))
            {
                Debugger.Log($"使用缓存资源: {url}", LogType.FrameNormal);
                progress.Status = "使用缓存资源";
                onProgress?.Invoke(progress);
                return await LoadFromPersistentDataAsync<T>(cachedPath, onProgress);
            }

            try
            {
                using (var webRequest = UnityWebRequest.Get(url))
                {
                    var type = typeof(T);
                    #region 判定T 资源类型
                    //图片文件
                    if (type == typeof(Texture2D) || type == typeof(Sprite))
                    {
                        webRequest.downloadHandler = new DownloadHandlerTexture(true);
                    }
                    //音频文件
                    else if (type == typeof(DownloadHandlerAudioClip) || type == typeof(AudioClip))
                    {
                        //Debug.Log("解析URL音频资源 默认音频类型为WAV，如需更改需在此设置");
                        //request.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.MPEG);
                        webRequest.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.WAV);
                    }
                    //本地文本文件
                    else if (type == typeof(string))
                    {

                    }

                    #endregion
                    var operation = webRequest.SendWebRequest();

                    while (!operation.isDone)
                    {
                        progress.Progress = operation.progress;
                        progress.Status = $"下载资源: {progress.Progress:P}";

                        // 更新字节信息
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

                    // 保存到缓存
                    var data = webRequest.downloadHandler.data;
                    await File.WriteAllBytesAsync(cachedPath, data);

                    progress.Status = "网络资源下载完成";
                    progress.Bytes = data;
                    onProgress?.Invoke(progress);


                    // 根据类型加载资源
                    // return await LoadDownloadedResource<T>(data, url, onProgress);
                    T changeType = null;
                    #region 获取资源实体
                    //判定T 资源类型
                    if (type == typeof(Texture2D) || type == typeof(Sprite))//图片资源
                    {
                        //获取资源
                        DownloadHandlerTexture downloadHandlerTexture = (DownloadHandlerTexture)webRequest.downloadHandler;
                        if (type == typeof(Texture2D))
                        {
                            //强转为资源类型
                            changeType = downloadHandlerTexture.texture as T;
                        }
                        else if (type == typeof(Sprite))
                        {
                            Sprite sprite = Sprite.Create(downloadHandlerTexture.texture, new Rect(0, 0, downloadHandlerTexture.texture.width, downloadHandlerTexture.texture.height), new Vector2(0.5f, 0.5f));
                            changeType = sprite as T;
                        }

                    }
                    else if (type == typeof(DownloadHandlerAudioClip) || type == typeof(AudioClip))//音频资源
                    {
                        DownloadHandlerAudioClip audioClip = (DownloadHandlerAudioClip)webRequest.downloadHandler;
                        if (type == typeof(DownloadHandlerAudioClip))
                        {
                            changeType = audioClip as T;
                        }
                        else if (type == typeof(AudioClip))
                        {
                            changeType = audioClip.audioClip as T;
                        }
                    }
                    else if (type == typeof(string))//文本资源
                    {
                        string txt = webRequest.downloadHandler.text;
                        changeType = txt as T;
                    }
                    Debugger.Log($"网络资源下载完成: {url}", LogType.FrameNormal);
                    return changeType;
                    #endregion
                }
            }
            catch (Exception e)
            {
                progress.Error = e;
                progress.IsDone = true;
                progress.Status = $"网络资源加载失败: {e.Message}";
                onProgress?.Invoke(progress);

                Debugger.LogError($"网络资源加载异常 {url}: {e.Message}", LogType.FrameCore);
                return null;
            }
        }

        /// <summary>
        /// 异步下载文件到本地
        /// </summary>
        public async Task DownloadFileAsync(string url, string localPath, Action<LoadProgress> onProgress = null)
        {
            var progress = new LoadProgress
            {
                Path = url,
                Status = "开始下载文件"
            };

            try
            {
                using (var webRequest = UnityWebRequest.Get(url))
                {
                    var operation = webRequest.SendWebRequest();
                    while (!operation.isDone)
                    {
                        progress.Progress = operation.progress;
                        progress.Status = $"Download，Progress: {progress.Progress:P}";

                        if (webRequest.downloadHandler != null)
                        {
                            progress.LoadedBytes = (long)webRequest.downloadedBytes;
                            if (webRequest.downloadHandler.data != null)
                            {
                                progress.TotalBytes = (long)webRequest.downloadHandler.data.Length;
                            }
                        }
                        onProgress?.Invoke(progress);
                        await Task.Yield();
                    }
                    progress.Progress = 1f;
                    progress.IsDone = true;

                    if (webRequest.result != UnityWebRequest.Result.Success)
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

                    Debugger.Log($"文件下载完成，url: {url} ，localPath： {localPath}", LogType.FrameNormal);
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

        #region 实例化管理
        /// <summary>
        /// 异步实例化游戏对象
        /// </summary>
        public async Task<GameObject> InstantiateAsync(string path, ResourceSource source = ResourceSource.Resources, Transform parent = null, Action<LoadProgress> onProgress = null)
        {
            var prefab = await LoadAsync<GameObject>(path, source, onProgress);
            if (prefab == null)
            {
                Debugger.LogError($"实例化失败，预制体加载失败: {path}", LogType.FrameCore);
                return null;
            }

            var instance = UnityEngine.Object.Instantiate(prefab, parent);
            _instanceToSourceMap[instance] = (path, source);
            return instance;
        }

        /// <summary>
        /// 异步实例化游戏对象到指定位置和旋转
        /// </summary>
        public async Task<GameObject> InstantiateAsync(string path, Vector3 position, Quaternion rotation, Transform parent = null, Action<LoadProgress> onProgress = null)
        {
            var prefab = await LoadAsync<GameObject>(path, ResourceSource.Resources, onProgress);
            if (prefab == null)
            {
                Debugger.LogError($"实例化失败，预制体加载失败: {path}", LogType.FrameCore);
                return null;
            }

            var instance = UnityEngine.Object.Instantiate(prefab, position, rotation, parent);
            _instanceToSourceMap[instance] = (path, ResourceSource.Resources);
            return instance;
        }
        #endregion

        #region 资源卸载管理
        /// <summary>
        /// 卸载指定资源
        /// </summary>
        public void Unload(string path, ResourceSource source)
        {
            var cacheKey = $"{source.ToString().ToLower()}://{path}";

            lock (_lockObject)
            {
                if (_resourceHandles.TryGetValue(cacheKey, out var handle))
                {
                    handle.ReferenceCount--;
                    handle.LastAccessTime = DateTime.Now;

                    if (handle.ReferenceCount <= 0)
                    {
                        // 延迟卸载，避免频繁加载卸载
                        // 实际卸载在清理时进行
                    }
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
                UnityEngine.Object.Destroy(instance);
                _instanceToSourceMap.Remove(instance);
                Unload(sourceInfo.path, sourceInfo.source);
            }
        }

        /// <summary>
        /// 强制清理所有未使用的资源
        /// </summary>
        public void UnloadUnusedAssets()
        {
            lock (_lockObject)
            {
                // 清理未使用的资源
                var toRemove = _resourceHandles.Where(kvp => kvp.Value.ReferenceCount <= 0).ToList();
                foreach (var kvp in toRemove)
                {
                    if (kvp.Value.AssetBundle == null)
                    {
                        UnityEngine.Object.Destroy(kvp.Value.Asset);
                    }
                    _resourceHandles.Remove(kvp.Key);
                }

                // 清理未使用的AssetBundle
                var bundlesToRemove = _assetBundleHandles.Where(kvp => kvp.Value.ReferenceCount <= 0).ToList();
                foreach (var kvp in bundlesToRemove)
                {
                    kvp.Value.Bundle.Unload(true);
                    _assetBundleHandles.Remove(kvp.Key);
                }
            }

            Resources.UnloadUnusedAssets();
            Debugger.Log("未使用资源清理完成", LogType.FrameNormal);
        }
        #endregion

        #region 预加载和批量操作
        /// <summary>
        /// 预加载多个资源
        /// </summary>
        public async Task PreloadAsync(List<string> paths, Action<LoadProgress> onProgress = null)
        {
            var progress = new LoadProgress
            {
                Status = "开始预加载资源"
            };

            Debugger.Log($"开始预加载 {paths.Count} 个资源", LogType.FrameNormal);

            var loadTasks = new List<Task>();
            int completedCount = 0;

            foreach (var path in paths)
            {
                var task = LoadAsync<UnityEngine.Object>(path, ResourceSource.Resources, (itemProgress) =>
                {
                    // 计算总体进度
                    float individualProgress = itemProgress.IsDone ? 1f : itemProgress.Progress;
                    float totalProgress = (float)(completedCount + individualProgress) / paths.Count;

                    progress.Progress = totalProgress;
                    progress.Status = $"预加载进度: {completedCount}/{paths.Count} ({totalProgress:P}) - {path}";
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

            Debugger.Log("预加载完成", LogType.FrameNormal);
        }
        #endregion

        #region 缓存管理
        /// <summary>
        /// 清理指定来源的缓存
        /// </summary>
        public void ClearCache(ResourceSource source)
        {
            lock (_lockObject)
            {
                var toRemove = _resourceHandles.Where(kvp => kvp.Value.Source == source).ToList();
                foreach (var kvp in toRemove)
                {
                    if (kvp.Value.AssetBundle == null)
                    {
                        UnityEngine.Object.Destroy(kvp.Value.Asset);
                    }
                    _resourceHandles.Remove(kvp.Key);
                }
            }

            if (source == ResourceSource.Network)
            {
                // 清理网络缓存文件
                if (Directory.Exists(_networkCacheDirectory))
                {
                    Directory.Delete(_networkCacheDirectory, true);
                    Directory.CreateDirectory(_networkCacheDirectory);
                }
            }
        }

        /// <summary>
        /// 获取指定来源的缓存大小
        /// </summary>
        public long GetCacheSize(ResourceSource source)
        {
            lock (_lockObject)
            {
                return _resourceHandles.Values
                    .Where(h => h.Source == source)
                    .Sum(h => h.MemorySize);
            }
        }
        #endregion

        #region Log

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

        #endregion


        #region 内部类定义
        /// <summary>
        /// 资源句柄，用于资源引用计数和生命周期管理
        /// </summary>
        private class ResourceHandle
        {
            public UnityEngine.Object Asset { get; set; }
            public int ReferenceCount { get; set; }
            public DateTime LastAccessTime { get; set; }
            public string Path { get; set; }
            public ResourceSource Source { get; set; }
            public long MemorySize { get; set; }
            public AssetBundle AssetBundle { get; set; }
        }

        /// <summary>
        /// AssetBundle句柄，管理AssetBundle的加载和引用
        /// </summary>
        private class AssetBundleHandle
        {
            public AssetBundle Bundle { get; set; }
            public int ReferenceCount { get; set; }
            public DateTime LastAccessTime { get; set; }
            public string BundlePath { get; set; }
            public string FilePath { get; set; }
            public Dictionary<string, ResourceHandle> LoadedAssets { get; set; } = new();
        }
        #endregion

        #region 字段定义
        private readonly Dictionary<string, ResourceHandle> _resourceHandles = new();
        /// <summary>
        /// key: FilePath
        /// </summary>
        private readonly Dictionary<string, AssetBundleHandle> _assetBundleHandles = new();
        private readonly Dictionary<GameObject, (string path, ResourceSource source)> _instanceToSourceMap = new();

        private string _networkCacheDirectory;
        private readonly object _lockObject = new object();
        //private readonly int _maxCacheCount = 100;
        private readonly long _maxMemorySize = 512 * 1024 * 1024;
        private readonly float _cacheCleanupInterval = 30f;
        private float _lastCleanupTime;
        #endregion

        #region 初始化
        /// <summary>
        /// 初始化资源管理器
        /// </summary>
        protected override async Task OnInitialize()
        {
            // 创建网络缓存目录
            _networkCacheDirectory = Path.Combine(Application.persistentDataPath, "NetworkCache");
            if (!Directory.Exists(_networkCacheDirectory))
            {
                Directory.CreateDirectory(_networkCacheDirectory);
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// 关闭资源管理器，清理所有资源
        /// </summary>
        protected override void OnShutdown()
        {
            // 清理所有资源
            foreach (var handle in _resourceHandles.Values)
            {
                if (handle.AssetBundle == null)
                {
                    UnityEngine.Object.Destroy(handle.Asset);
                }
            }
            _resourceHandles.Clear();

            // 清理AssetBundle
            foreach (var bundleHandle in _assetBundleHandles.Values)
            {
                bundleHandle.Bundle.Unload(true);
            }
            _assetBundleHandles.Clear();

            _instanceToSourceMap.Clear();

            Resources.UnloadUnusedAssets();
            Debugger.Log("资源管理器已关闭", LogType.FrameNormal);
        }
        #endregion


        #region Resources文件夹资源加载
        /// <summary>
        /// 从Resources文件夹异步加载资源
        /// </summary>
        public async Task<T> LoadFromResourcesAsync<T>(string path, Action<LoadProgress> onProgress = null) where T : UnityEngine.Object
        {
            var cacheKey = $"resources://{path}";

            lock (_lockObject)
            {
                if (_resourceHandles.TryGetValue(cacheKey, out var handle))
                {
                    handle.ReferenceCount++;
                    handle.LastAccessTime = DateTime.Now;
                    return handle.Asset as T;
                }
            }

            var progress = new LoadProgress
            {
                Path = path,
                Status = "开始加载Resources资源"
            };

            try
            {
                var resourceRequest = Resources.LoadAsync<T>(path);
                // 进度更新循环
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
                    throw new Exception($"Resources资源不存在: {path}");
                }

                var newHandle = new ResourceHandle
                {
                    Asset = resourceRequest.asset,
                    ReferenceCount = 1,
                    LastAccessTime = DateTime.Now,
                    Path = cacheKey,
                    Source = ResourceSource.Resources,
                    MemorySize = EstimateMemorySize(resourceRequest.asset)
                };

                lock (_lockObject)
                {
                    _resourceHandles[cacheKey] = newHandle;
                }

                return resourceRequest.asset as T;
            }
            catch (Exception e)
            {
                progress.Error = e;
                progress.IsDone = true;
                progress.Status = $"Resources资源加载失败: {e.Message}";
                onProgress?.Invoke(progress);

                Debugger.LogError($"Resources资源加载失败 {path}: {e.Message}", LogType.FrameCore);
                return null;
            }
        }
        #endregion

        #region 网络资源加载

        /// <summary>
        ///  TODO后面改为UnityWebReques 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="url"></param>
        /// <param name="onProgress"></param>
        /// <returns></returns>
        private async Task<T> LoadDownloadedResource<T>(byte[] data, string url, Action<LoadProgress> onProgress = null) where T : UnityEngine.Object
        {
            var progress = new LoadProgress
            {
                Path = url,
                Status = "处理下载的资源"
            };

            try
            {
                // 根据类型创建资源
                if (typeof(T) == typeof(Texture2D) || typeof(T) == typeof(Texture))
                {
                    progress.Status = "创建Texture2D";
                    onProgress?.Invoke(progress);

                    var texture = new Texture2D(2, 2);
                    texture.LoadImage(data);
                    return texture as T;
                }
                else if (typeof(T) == typeof(TextAsset))
                {
                    progress.Status = "创建TextAsset";
                    onProgress?.Invoke(progress);

                    var text = System.Text.Encoding.UTF8.GetString(data);
                    var textAsset = new TextAsset(text);
                    return textAsset as T;
                }
                //else if (typeof(T) == typeof(AudioClip))
                //{
                //    return null;
                //}
                //else if (typeof(T) == typeof(GameObject))
                //{
                //    progress.Status = "创建GameObject";
                //    onProgress?.Invoke(progress);

                //    // 这里需要根据Prefab格式处理，简化实现
                //    return null;
                //}
                else
                {
                    Debugger.LogError($"暂不支持的下载资源类型,{typeof(T)}", LogType.FrameCore);
                    throw new Exception($"不支持的下载资源类型，{typeof(T)}");
                }
            }
            catch (Exception e)
            {
                progress.Error = e;
                progress.IsDone = true;
                progress.Status = $"资源处理失败: {e.Message}";
                onProgress?.Invoke(progress);

                Debugger.LogError($"下载资源处理异常: {e.Message}", LogType.FrameCore);
                await Task.CompletedTask;

                return null;
            }
        }

        private string GetCachedFilePath(string url)
        {
            var fileName = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(url))
                .Replace("/", "_")
                .Replace("+", "-")
                .Replace("=", "");
            return Path.Combine(_networkCacheDirectory, fileName);
        }
        #endregion

        #region StreamingAssets和PersistentData资源加载
        private async Task<T> LoadFromStreamingAssetsAsync<T>(string path, Action<LoadProgress> onProgress = null) where T : UnityEngine.Object
        {
            var fullPath = Path.Combine(Application.streamingAssetsPath, path);
            return await LoadFromFileAsync<T>(fullPath, ResourceSource.StreamingAssets, onProgress);
        }

        private async Task<T> LoadFromPersistentDataAsync<T>(string path, Action<LoadProgress> onProgress = null) where T : UnityEngine.Object
        {
            var fullPath = Path.Combine(Application.persistentDataPath, path);
            return await LoadFromFileAsync<T>(fullPath, ResourceSource.PersistentData, onProgress);
        }

        private async Task<T> LoadFromFileAsync<T>(string fullPath, ResourceSource source, Action<LoadProgress> onProgress = null) where T : UnityEngine.Object
        {
            var progress = new LoadProgress
            {
                Path = fullPath,
                Status = "开始读取文件"
            };

            if (!File.Exists(fullPath))
            {
                progress.Error = new Exception($"文件不存在: {fullPath}");
                progress.IsDone = true;
                progress.Status = "文件不存在";
                onProgress?.Invoke(progress);

                Debugger.LogError($"文件不存在: {fullPath}", LogType.FrameCore);
                return null;
            }

            var cacheKey = $"{source.ToString().ToLower()}://{fullPath}";

            lock (_lockObject)
            {
                if (_resourceHandles.TryGetValue(cacheKey, out var handle))
                {
                    handle.ReferenceCount++;
                    handle.LastAccessTime = DateTime.Now;
                    return handle.Asset as T;
                }
            }

            try
            {
                progress.Status = "读取文件数据";
                onProgress?.Invoke(progress);

                var bytes = await File.ReadAllBytesAsync(fullPath);

                progress.Progress = 0.5f;
                progress.Status = "处理文件数据";
                onProgress?.Invoke(progress);

                var resource = await LoadDownloadedResource<T>(bytes, fullPath, (resourceProgress) =>
                {
                    // 合并进度：文件读取50% + 资源处理50%
                    progress.Progress = 0.5f + resourceProgress.Progress * 0.5f;
                    progress.Status = resourceProgress.Status;
                    onProgress?.Invoke(progress);
                });

                progress.Progress = 1f;
                progress.IsDone = true;
                progress.Status = "文件加载完成";
                progress.Asset = resource;
                onProgress?.Invoke(progress);

                if (resource != null)
                {
                    var newHandle = new ResourceHandle
                    {
                        Asset = resource,
                        ReferenceCount = 1,
                        LastAccessTime = DateTime.Now,
                        Path = cacheKey,
                        Source = source,
                        MemorySize = EstimateMemorySize(resource)
                    };

                    lock (_lockObject)
                    {
                        _resourceHandles[cacheKey] = newHandle;
                    }
                }

                return resource;
            }
            catch (Exception e)
            {
                progress.Error = e;
                progress.IsDone = true;
                progress.Status = $"文件加载失败: {e.Message}";
                onProgress?.Invoke(progress);

                Debugger.LogError($"文件加载异常 {fullPath}: {e.Message}", LogType.FrameCore);
                return null;
            }
        }
        #endregion

        #region 字节和文本加载

        private async Task<byte[]> ReadFileBytesAsync(string filePath, LoadProgress progress, Action<LoadProgress> onProgress)
        {
            if (!File.Exists(filePath))
            {
                throw new Exception($"文件不存在: {filePath}");
            }

            progress.Status = "读取文件字节";
            onProgress?.Invoke(progress);

            var bytes = await File.ReadAllBytesAsync(filePath);

            progress.Progress = 1f;
            progress.Status = "文件读取完成";
            onProgress?.Invoke(progress);

            return bytes;
        }

        private async Task<byte[]> LoadBytesFromResourcesAsync(string path, LoadProgress progress, Action<LoadProgress> onProgress)
        {
            progress.Status = "加载Resources字节资源";
            onProgress?.Invoke(progress);

            var resourceRequest = Resources.LoadAsync<TextAsset>(path);

            while (!resourceRequest.isDone)
            {
                progress.Progress = resourceRequest.progress;
                progress.Status = $"加载Resources资源: {progress.Progress:P}";
                onProgress?.Invoke(progress);
                await Task.Yield();
            }

            if (resourceRequest.asset == null)
            {
                throw new Exception($"Resources字节资源不存在: {path}");
            }

            var textAsset = resourceRequest.asset as TextAsset;
            if (textAsset == null)
            {
                throw new Exception($"Resources资源不是TextAsset: {path}");
            }

            progress.Progress = 1f;
            progress.Status = "Resources字节资源加载完成";
            onProgress?.Invoke(progress);

            return textAsset.bytes;
        }

        private async Task<byte[]> DownloadBytesAsync(string url, LoadProgress progress, Action<LoadProgress> onProgress)
        {
            progress.Status = "开始下载字节数据";
            onProgress?.Invoke(progress);

            using (var webRequest = UnityEngine.Networking.UnityWebRequest.Get(url))
            {
                var operation = webRequest.SendWebRequest();

                while (!operation.isDone)
                {
                    progress.Progress = operation.progress;
                    progress.Status = $"下载字节数据: {progress.Progress:P}";

                    if (webRequest.downloadHandler != null)
                    {
                        progress.LoadedBytes = (long)webRequest.downloadedBytes;
                        progress.TotalBytes = (long)webRequest.downloadHandler.data.Length;
                    }

                    onProgress?.Invoke(progress);
                    await Task.Yield();
                }

                if (webRequest.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    throw new Exception($"字节下载失败: {webRequest.error}");
                }

                progress.Progress = 1f;
                progress.Status = "字节数据下载完成";
                onProgress?.Invoke(progress);

                return webRequest.downloadHandler.data;
            }
        }
        #endregion


        #region 生命周期管理
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
                        if (handle.AssetBundle == null)
                        {
                            UnityEngine.Object.Destroy(handle.Asset);
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
                        if (handle.AssetBundle == null)
                        {
                            UnityEngine.Object.Destroy(handle.Asset);
                        }
                        _resourceHandles.Remove(path);
                    }
                }
            }
        }
        #endregion

        #region 工具方法
        private long EstimateMemorySize(UnityEngine.Object obj)
        {
            if (obj is Texture2D texture) return texture.width * texture.height * 4;
            if (obj is Mesh mesh) return (mesh.vertices.Length * 12) + (mesh.triangles.Length * 4);
            if (obj is AudioClip audio) return (long)(audio.samples * audio.channels * 4);
            return 1024; // 默认1KB
        }
        #endregion
    }
}