using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace MFramework.Runtime
{
    public interface IResourcesManager
    {
        Task<T> LoadAssetAsync<T>(string address, bool isAutoAddSuffix = true) where T : Object;

        Task<T> LoadAssetAsync<T>(string address, Action<AsyncOperationHandle<T>> completedCallback,
            Action<AsyncOperationHandle> destroyedCallback, bool isAutoAddSuffix = true) where T : Object;

        Task<List<T>> LoadAssetsAsync<T>(List<string> addresses, bool isAutoAddSuffix = true) where T : Object;

        Task LoadSceneAsync(string sceneAddress);

        bool ReleaseAsset(Object asset);

        bool ReleaseAsset(string address);

        bool ReleaseAsset<T>(string address, bool isAutoAddSuffix = true) where T : Object;

        Task<List<T>> PreloadAssetsAsync<T>(List<string> addresses, bool isAutoAddSuffix = true) where T : Object;

        bool IsAssetLoaded(string address);

        bool ReleaseAllAssets();
    } 
}