using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 资源管理接口
/// </summary>
public interface IResourcesManager
{
    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="address">资源地址</param>
    /// <returns>资源对象</returns>
    Task<T> LoadAssetAsync<T>(string address, bool isAutoAddSuffix = true) where T : UnityEngine.Object;

    /// <summary>
    /// 异步加载多个资源
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="addresses">资源地址列表</param>
    /// <returns>资源对象列表</returns>
    Task<List<T>> LoadAssetsAsync<T>(List<string> addresses, bool isAutoAddSuffix = true) where T : UnityEngine.Object;

    /// <summary>
    /// 异步加载场景
    /// </summary>
    /// <param name="sceneAddress">场景地址</param>
    /// <returns>场景加载操作</returns>
    Task LoadSceneAsync(string sceneAddress);

    /// <summary>
    /// 卸载资源
    /// </summary>
    /// <param name="asset">资源对象</param>
    void ReleaseAsset(UnityEngine.Object asset);

    /// <summary>
    /// 卸载资源（通过地址）
    /// </summary>
    /// <param name="address">资源地址</param>
    void ReleaseAsset(string address);

    /// <summary>
    /// 预加载资源
    /// </summary>
    /// <param name="addresses">预加载地址列表</param>
    /// <returns>预加载任务</returns>
    Task PreloadAssetsAsync<T>(List<string> addresses, bool isAutoAddSuffix = true) where T : UnityEngine.Object;

    /// <summary>
    /// 获取资源加载状态
    /// </summary>
    /// <param name="address">资源地址</param>
    /// <returns>是否已加载</returns>
    bool IsAssetLoaded(string address);

    /// <summary>
    /// 清理所有资源
    /// </summary>
    void Clear();
}