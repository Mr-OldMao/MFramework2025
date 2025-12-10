using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace MFramework.Runtime
{
    public interface IResourcesManager : IGameBase
    {
        UniTask<T> LoadAssetAsync<T>(string address, bool isAutoAddSuffix = true) where T : Object;

        UniTask<T> LoadAssetAsync<T>(string address, Action<AsyncOperationHandle<T>> completedCallback,
            Action<AsyncOperationHandle> destroyedCallback, bool isAutoAddSuffix = true) where T : Object;

        UniTask<List<T>> LoadAssetsAsync<T>(List<string> addresses, bool isAutoAddSuffix = true) where T : Object;

        UniTask LoadSceneAsync(string sceneAddress);

        bool ReleaseAsset(Object asset);

        bool ReleaseAsset(string address);

        bool ReleaseAsset<T>(string address, bool isAutoAddSuffix = true) where T : Object;

        UniTask<List<T>> PreloadAssetsAsync<T>(List<string> addresses, bool isAutoAddSuffix = true) where T : Object;

        bool IsAssetLoaded(string address);

        bool ReleaseAllAssets();
    } 
}