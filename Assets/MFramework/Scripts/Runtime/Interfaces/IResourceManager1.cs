//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using UnityEngine;
//namespace MFramework.Runtime
//{
//    /// <summary>
//    /// 资源管理器接口
//    /// 提供多来源资源加载、卸载和管理功能
//    /// </summary>
//    public interface IResourceManager : IGameModule
//    {

//        #region 通用异步加载接口
//        /// <summary>
//        /// 通用异步资源加载方法
//        /// </summary>
//        /// <typeparam name="T">资源类型（GameObject, Texture2D, AudioClip等）</typeparam>
//        /// <param name="path">资源路径</param>
//        /// <param name="source">资源来源</param>
//        /// <param name="onProgress">加载进度回调（可选）</param>
//        /// <returns>加载的资源对象，失败返回null</returns>
//        Task<T> LoadAsync<T>(string path, ResourceSource source = ResourceSource.Resources, Action<LoadProgress> onProgress = null) where T : UnityEngine.Object;
//        #endregion

//        #region 指定来源的加载接口
//        /// <summary>
//        /// 从Resources文件夹异步加载资源
//        /// </summary>
//        /// <typeparam name="T">资源类型</typeparam>
//        /// <param name="path">Resources文件夹下的相对路径</param>
//        /// <param name="onProgress">加载进度回调（可选）</param>
//        /// <returns>加载的资源对象</returns>
//        Task<T> LoadFromResourcesAsync<T>(string path, Action<LoadProgress> onProgress = null) where T : UnityEngine.Object;

//        /// <summary>
//        /// 从AssetBundle异步加载资源
//        /// </summary>
//        /// <typeparam name="T">资源类型</typeparam>
//        /// <param name="bundlePath">AssetBundle文件路径</param>
//        /// <param name="assetPath">AssetBundle内的资源路径</param>
//        /// <param name="onProgress">加载进度回调（可选）</param>
//        /// <returns>加载的资源对象</returns>
//        Task<T> LoadFromAssetBundleAsync<T>(string bundlePath, string assetPath, Action<LoadProgress> onProgress = null) where T : UnityEngine.Object;

//        /// <summary>
//        /// 从网络异步加载资源
//        /// </summary>
//        /// <typeparam name="T">资源类型（Texture2D, AudioClip, TextAsset）</typeparam>
//        /// <param name="url">资源URL地址</param>
//        /// <param name="onProgress">加载进度回调（可选）</param>
//        /// <returns>加载的资源对象</returns>
//        Task<T> LoadFromNetworkAsync<T>(string url, Action<LoadProgress> onProgress = null) where T : UnityEngine.Object;
//        #endregion

//        #region 字节和文本加载接口
//        /// <summary>
//        /// 异步加载字节数据
//        /// </summary>
//        /// <param name="path">资源路径或URL</param>
//        /// <param name="source">资源来源</param>
//        /// <param name="onProgress">加载进度回调（可选）</param>
//        /// <returns>字节数组，失败返回null</returns>
//        Task<byte[]> LoadBytesAsync(string path, ResourceSource source, Action<LoadProgress> onProgress = null);

//        /// <summary>
//        /// 异步加载文本数据
//        /// </summary>
//        /// <param name="path">资源路径或URL</param>
//        /// <param name="source">资源来源</param>
//        /// <param name="onProgress">加载进度回调（可选）</param>
//        /// <returns>文本内容，失败返回null</returns>
//        Task<string> LoadTextAsync(string path, ResourceSource source, Action<LoadProgress> onProgress = null);
//        #endregion

//        #region AssetBundle管理
//        /// <summary>
//        /// 异步加载AssetBundle
//        /// </summary>
//        /// <param name="bundlePath">AssetBundle文件路径</param>
//        /// <param name="fromStreamingAssets">是否从StreamingAssets加载</param>
//        /// <param name="onProgress">加载进度回调（可选）</param>
//        /// <returns>AssetBundle对象，失败返回null</returns>
//        Task<AssetBundle> LoadAssetBundleAsync(string bundlePath, bool fromStreamingAssets = true, Action<LoadProgress> onProgress = null);

//        /// <summary>
//        /// 卸载AssetBundle
//        /// </summary>
//        /// <param name="bundlePath">AssetBundle文件路径</param>
//        /// <param name="unloadAllObjects">是否卸载所有包含的对象</param>
//        void UnloadAssetBundle(string bundlePath, bool unloadAllObjects = false);
//        #endregion

//        #region 网络资源管理
//        /// <summary>
//        /// 异步下载文件到本地
//        /// </summary>
//        /// <param name="url">文件URL地址</param>
//        /// <param name="localPath">本地保存路径</param>
//        /// <param name="onProgress">下载进度回调（可选）</param>
//        /// <returns>下载任务</returns>
//        Task DownloadFileAsync(string url, string localPath, Action<LoadProgress> onProgress = null);

//        /// <summary>
//        /// 检查文件是否已缓存
//        /// </summary>
//        /// <param name="url">文件URL地址</param>
//        /// <returns>true表示文件已缓存</returns>
//        Task<bool> IsFileCached(string url);
//        #endregion

//        #region 实例化管理
//        /// <summary>
//        /// 异步实例化游戏对象
//        /// </summary>
//        /// <param name="path">预制体路径</param>
//        /// <param name="source">资源来源</param>
//        /// <param name="parent">父级变换（可选）</param>
//        /// <param name="onProgress">加载进度回调（可选）</param>
//        /// <returns>实例化的游戏对象</returns>
//        Task<GameObject> InstantiateAsync(string path, ResourceSource source = ResourceSource.Resources, Transform parent = null, Action<LoadProgress> onProgress = null);

//        /// <summary>
//        /// 异步实例化游戏对象到指定位置和旋转
//        /// </summary>
//        /// <param name="path">预制体路径</param>
//        /// <param name="position">世界坐标位置</param>
//        /// <param name="rotation">世界坐标旋转</param>
//        /// <param name="parent">父级变换（可选）</param>
//        /// <param name="onProgress">加载进度回调（可选）</param>
//        /// <returns>实例化的游戏对象</returns>
//        Task<GameObject> InstantiateAsync(string path, Vector3 position, Quaternion rotation, Transform parent = null, Action<LoadProgress> onProgress = null);

//        /// <summary>
//        /// 同步实例化游戏对象
//        /// </summary>
//        /// <param name="path">预制体路径</param>
//        /// <param name="parent">父级变换（可选）</param>
//        /// <returns>实例化的游戏对象</returns>
//        GameObject InstantiateSync(string path, Transform parent = null);
//        #endregion

//        #region 资源卸载管理
//        /// <summary>
//        /// 卸载指定资源
//        /// </summary>
//        /// <param name="path">资源路径</param>
//        /// <param name="source">资源来源</param>
//        void Unload(string path, ResourceSource source);

//        /// <summary>
//        /// 卸载游戏对象实例
//        /// </summary>
//        /// <param name="instance">要卸载的游戏对象实例</param>
//        void Unload(GameObject instance);

//        /// <summary>
//        /// 强制清理所有未使用的资源
//        /// </summary>
//        void UnloadUnusedAssets();
//        #endregion

//        #region 预加载和批量操作
//        /// <summary>
//        /// 预加载多个资源
//        /// </summary>
//        /// <param name="paths">资源路径列表</param>
//        /// <param name="onProgress">总体加载进度回调（可选）</param>
//        /// <returns>预加载任务</returns>
//        Task PreloadAsync(List<string> paths, Action<LoadProgress> onProgress = null);
//        #endregion

//        #region 缓存管理
//        /// <summary>
//        /// 清理指定来源的缓存
//        /// </summary>
//        /// <param name="source">要清理的缓存来源</param>
//        void ClearCache(ResourceSource source);

//        /// <summary>
//        /// 获取指定来源的缓存大小（字节）
//        /// </summary>
//        /// <param name="source">缓存来源</param>
//        /// <returns>缓存大小（字节）</returns>
//        long GetCacheSize(ResourceSource source);
//        #endregion
//    }

//    /// <summary>
//    /// 资源加载进度信息
//    /// </summary>
//    public class LoadProgress
//    {
//        /// <summary>
//        /// 资源路径或URL
//        /// </summary>
//        public string Path { get; set; }

//        /// <summary>
//        /// 加载进度（0-1）
//        /// </summary>
//        public float Progress { get; set; }

//        /// <summary>
//        /// 是否加载完成
//        /// </summary>
//        public bool IsDone { get; set; }

//        /// <summary>
//        /// 加载的资源对象（仅对UnityEngine.Object资源有效）
//        /// </summary>
//        public UnityEngine.Object Asset { get; set; }

//        /// <summary>
//        /// 加载的字节数据（仅对字节加载有效）
//        /// </summary>
//        public byte[] Bytes { get; set; }

//        /// <summary>
//        /// 加载的文本数据（仅对文本加载有效）
//        /// </summary>
//        public string Text { get; set; }

//        /// <summary>
//        /// 加载错误信息
//        /// </summary>
//        public Exception Error { get; set; }

//        /// <summary>
//        /// 当前加载状态描述
//        /// </summary>
//        public string Status { get; set; }

//        /// <summary>
//        /// 已加载字节数（对网络加载有效）
//        /// </summary>
//        public long LoadedBytes { get; set; }

//        /// <summary>
//        /// 总字节数（对网络加载有效）
//        /// </summary>
//        public long TotalBytes { get; set; }
//    }



//}