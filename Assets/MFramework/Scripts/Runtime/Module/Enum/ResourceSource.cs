using System;

namespace MFramework.Runtime
{
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
    /// <summary>
    /// 资源文件类型枚举
    /// </summary>
    public enum ResourceFileType
    {
        /// <summary>
        /// 文本文件
        /// </summary>
        Txt,

        /// <summary>
        /// JSON文件
        /// </summary>
        Json,

        /// <summary>
        /// 二进制文件
        /// </summary>
        Bytes,

        /// <summary>
        /// AssetBundle包
        /// </summary>
        AB,

        /// <summary>
        /// Unity资源（预制体、纹理、音频等）
        /// </summary>
        UnityAsset
    }


    /// <summary>
    /// 资源来源枚举
    /// 定义资源加载的不同来源
    /// </summary>
    public enum ResourceSource
    {
        /// <summary>
        /// Resources文件夹 - 内置资源，打包时包含
        /// </summary>
        Resources,
        ///// <summary>
        ///// AssetBundle包 - 热更新资源，可从服务器下载
        ///// </summary>
        //AssetBundle,
        /// <summary>
        /// StreamingAssets文件夹 - 只读数据，配置文件和初始资源
        /// </summary>
        StreamingAssets,
        /// <summary>
        /// PersistentDataPath文件夹 - 可读写数据，用户数据和缓存
        /// </summary>
        PersistentData,
        /// <summary>
        /// 网络资源 - 实时下载的图片、音频、配置文件等
        /// </summary>
        Network,
        ///// <summary>
        ///// Addressable系统 - Unity的可寻址资源系统（可选）
        ///// </summary>
        //Addressables
    }
}
