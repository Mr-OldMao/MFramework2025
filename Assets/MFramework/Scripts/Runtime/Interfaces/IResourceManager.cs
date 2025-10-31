using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace MFramework.Runtime
{
    /// <summary>
    /// 统一资源管理器接口 - 简洁API设计
    /// </summary>
    public interface IResourceManager : IGameModule
    {
        #region 核心加载接口（最简设计）
        /// <summary>
        /// 通用异步资源加载方法
        /// </summary>
        Task<T> LoadAssetAsync<T>(
            string assetPath,
            ResourceSourceType resourceSourceType,
            ResourceFileType resourceFileType = ResourceFileType.UnityAsset,
            Action<LoadProgress> onProgress = null);

        /// <summary>
        /// 同步资源加载方法（谨慎使用）
        /// </summary>
        T LoadAssetSync<T>(
            string assetPath,
            ResourceSourceType resourceSourceType,
            ResourceFileType resourceFileType = ResourceFileType.UnityAsset);
        #endregion

        #region 智能快捷方法（无需指定文件类型）
        /// <summary>
        /// 智能加载Unity资源（自动推断文件类型）
        /// </summary>
        Task<T> LoadAsync<T>(string assetPath, ResourceSourceType resourceSourceType) where T : UnityEngine.Object;

        /// <summary>
        /// 智能加载文本
        /// </summary>
        Task<string> LoadTextAsync(string assetPath, ResourceSourceType resourceSourceType);

        /// <summary>
        /// 智能加载JSON
        /// </summary>
        Task<T> LoadJsonAsync<T>(string assetPath, ResourceSourceType resourceSourceType);

        /// <summary>
        /// 智能加载二进制
        /// </summary>
        Task<byte[]> LoadBytesAsync(string assetPath, ResourceSourceType resourceSourceType);
        #endregion

        #region 极简快捷方法（预设常用来源）
        /// <summary>
        /// 从Resources加载资源（最常用）
        /// </summary>
        Task<T> LoadResourceAsync<T>(string assetPath) where T : UnityEngine.Object;

        /// <summary>
        /// 从StreamingAssets加载文本配置
        /// </summary>
        Task<string> LoadConfigTextAsync(string configPath);

        /// <summary>
        /// 从PersistentData加载用户数据
        /// </summary>
        Task<T> LoadUserDataAsync<T>(string dataPath);

        /// <summary>
        /// 从网络加载图片
        /// </summary>
        Task<Texture2D> LoadNetImageAsync(string url);

        /// <summary>
        /// 从AssetBundle加载资源
        /// </summary>
        Task<T> LoadBundleAssetAsync<T>(string bundleName, string assetPath) where T : UnityEngine.Object;
        #endregion

        #region 实例化快捷方法
        /// <summary>
        /// 实例化预制体（从Resources）
        /// </summary>
        Task<GameObject> InstantiateAsync(string assetPath, Transform parent = null);

        /// <summary>
        /// 实例化预制体到指定位置
        /// </summary>
        Task<GameObject> InstantiateAsync(string assetPath, Vector3 position, Quaternion rotation, Transform parent = null);
        #endregion

        #region 资源管理
        void Unload(string assetPath, ResourceSourceType resourceSourceType);
        void Unload(GameObject instance);
        void UnloadUnusedAssets();
        #endregion
    }

    /// <summary>
    /// 资源文件类型
    /// </summary>
    public enum ResourceFileType
    {
        /// <summary>Unity资源（预制体、纹理、音频等）</summary>
        UnityAsset,
        /// <summary>文本文件</summary>
        Txt,
        /// <summary>JSON文件</summary>
        Json,
        /// <summary>二进制文件</summary>
        Bytes,
        /// <summary>AssetBundle文件</summary>
        AB
    }

    /// <summary>
    /// 资源来源类型
    /// </summary>
    public enum ResourceSourceType
    {
        /// <summary>Unity Resources文件夹</summary>
        Resources,
        /// <summary>AssetBundle包 - 热更新资源</summary>
        AssetBundle,
        /// <summary>StreamingAssets文件夹 - 只读配置数据</summary>
        StreamingAssets,
        /// <summary>PersistentDataPath文件夹 - 可读写用户数据</summary>
        PersistentData,
        /// <summary>网络资源 - 实时下载资源</summary>
        Network,
        /// <summary>Addressable系统</summary>
        Addressables
    }

    /// <summary>
    /// 加载进度信息
    /// </summary>
    public class LoadProgress
    {
        public string Path { get; set; }
        public float Progress { get; set; }
        public bool IsDone { get; set; }
        public UnityEngine.Object Asset { get; set; }
        public byte[] Bytes { get; set; }
        public string Text { get; set; }
        public Exception Error { get; set; }
        public string Status { get; set; }
        public long LoadedBytes { get; set; }
        public long TotalBytes { get; set; }
    }
}