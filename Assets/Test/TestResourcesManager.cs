using MFramework.Runtime;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class TestResourcesManager : MonoBehaviour
{
    private string m_ResoureceRootDir = "TestRes";
    private async void Start()
    {
        await System.Threading.Tasks.Task.Delay(500);

        //资源所在位置
        //Resources,
        //AssetBundle,
        //StreamingAssets,
        //PersistentData,
        //Network,
        //Addressables

        //读取的文件资源类型
        //.txt文件
        //.json文件
        //.bytes文件
        //.ab文件
        await TestLoadTextAsset();

        await TestLoadAssetBundle();

        await TestLoadNetworkAsset();
        return;

        await TestReloadAsset();
    }

    private async Task TestLoadTextAsset()
    {
        string resName = string.Empty;
        Debugger.Log($"--------------------测试异步加载Resources资源,GameEntry.Resource.LoadTextAsync()-------------------", LogType.Test);
        Debugger.Log($"测试异步加载.txt文本资源", LogType.Test);
        resName = $"{m_ResoureceRootDir}/resFileTxt";
        var resTxtFile = await GameEntry.Resource.LoadTextAsync(resName, ResourceSource.Resources, CallbackLoadProgress);
        Debugger.Log($"ResAsset：{resTxtFile}", LogType.Test);

        Debugger.Log($"测试异步加载.json文本资源", LogType.Test);
        resName = $"{m_ResoureceRootDir}/resFileJson";
        var resJsonFile = await GameEntry.Resource.LoadTextAsync(resName, ResourceSource.Resources, CallbackLoadProgress);
        Debugger.Log($"ResAsset：{resJsonFile}", LogType.Test);

        Debugger.Log($"测试异步加载.bytes文本资源", LogType.Test);
        resName = $"{m_ResoureceRootDir}/resFileBytes";
        var resByteFile = await GameEntry.Resource.LoadBytesAsync(resName, ResourceSource.Resources, CallbackLoadProgress);
        Debugger.Log($"ResAsset：{resByteFile?.Length}", LogType.Test);


        Debugger.Log($"--------------------测试异步加载Resources资源(通用接口) GameEntry.Resource.LoadAsync<T>-------------------", LogType.Test);
        resName = $"{m_ResoureceRootDir}/resFileTxt";
        Debugger.Log($"测试异步加载.txt文本资源", LogType.Test);
        var resTxtFileNormal = await GameEntry.Resource.LoadAsync<TextAsset>(resName, ResourceSource.Resources, CallbackLoadProgress);
        Debugger.Log($"ResAsset：{resTxtFileNormal}", LogType.Test);


        resName = $"{m_ResoureceRootDir}/resFileJson";
        Debugger.Log($"测试异步加载.json文本资源", LogType.Test);
        var resJsonNormal = await GameEntry.Resource.LoadAsync<TextAsset>(resName, ResourceSource.Resources, CallbackLoadProgress);
        Debugger.Log($"ResAsset：{resJsonNormal}", LogType.Test);

        resName = $"{m_ResoureceRootDir}/resFileBytes";
        Debugger.Log($"测试异步加载.bytes文本资源", LogType.Test);
        var resBytesFileNormal = await GameEntry.Resource.LoadAsync<TextAsset>(resName, ResourceSource.Resources, CallbackLoadProgress);
        Debugger.Log($"ResAsset：{resBytesFileNormal.bytes.Length}", LogType.Test);
    }

    private async Task TestLoadAssetBundle()
    {
        string resName = string.Empty;
        resName = $"{m_ResoureceRootDir}/AB";
        Debugger.Log($"测试异步加载.ab 资源", LogType.Test);
        //var resAbFile = await GameEntry.Resource.LoadAsync<AssetBundle>(resName, ResourceSource.Resources, CallbackLoadProgress);
        AssetBundle resAbFile = await GameEntry.Resource.LoadFromAssetBundleAsync<AssetBundle>(resName, "AB", ResourceSource.Resources, CallbackLoadProgress);
        Debugger.Log($"ResAsset：{resAbFile}", LogType.Test);

        AssetBundleManifest abManifest = resAbFile.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        var allBunlds = abManifest.GetAllAssetBundles();

        foreach (var bundleName in allBunlds)
        {
            Debugger.Log($"AssetBundle资源 bundleName1：{bundleName}", LogType.Test);

            var bundle = await GameEntry.Resource.LoadFromAssetBundleAsync<GameObject>($"{m_ResoureceRootDir}/AB/Test", bundleName, ResourceSource.Resources, CallbackLoadProgress);
            Debugger.Log($"AssetBundle资源 bundle2：{bundle}", LogType.Test);

        }
    }

    private async Task TestLoadNetworkAsset()
    {
        Debugger.Log($"测试异步加载网络资源", LogType.Test);

        string localPath = "downloads/test.png";
        await GameEntry.Resource.DownloadFileAsync("https://pic.rmb.bdstatic.com/bjh/news/d784e0991477b5a84f4ed1de4f1693c7.png", localPath, (progress) =>
        {
            Debug.Log($"网络资源下载中,下载进度: {progress.Progress:P} - {progress.LoadedBytes}/{progress.TotalBytes} bytes");
            if (progress.IsDone)
            {
                Debugger.Log($"网络资源下载完成,directory:{Path.GetDirectoryName(localPath)}/{localPath}", LogType.Test);
            }
        });
    }

    /// <summary>
    /// 预加载多个资源
    /// </summary>
    /// <returns></returns>
    private async Task TestReloadAsset()
    {
        //// 4. 预加载多个资源
        //var preloadList = new List<string> { "Prefabs/Enemy1", "Prefabs/Enemy2", "Textures/UI/Icons" };
        //await GameEntry.Resource.PreloadAsync(preloadList, progress =>
        //{
        //    Debug.Log($"预加载进度: {progress.Progress:P}");
        //});
    }

    private void CallbackLoadProgress(LoadProgress result)
    {
        Debugger.Log($"----- path：{result.Path}，progress：{result.Progress}，status：{result.Status}，isDone：{result.IsDone}，Asset：{result.Asset}，error：{result.Error}");
    }

    private async void Start11()
    {
        //// 1. 从Resources加载
        //var localPrefab = await GameEntry.Resource.LoadFromResourcesAsync<GameObject>("Prefabs/LocalCharacter");

        //// 2. 从AssetBundle加载
        //var abPrefab = await GameEntry.Resource.LoadFromAssetBundleAsync<GameObject>("characters", "prefabs/hero");

        //// 3. 从网络加载图片
        //var networkTexture = await GameEntry.Resource.LoadFromNetworkAsync<Texture2D>("https://example.com/avatar.png");

        //// 4. 从StreamingAssets加载配置
        //var configText = await GameEntry.Resource.LoadTextAsync("config/game.json", ResourceSource.StreamingAssets);

        //// 5. 通用接口，自动判断来源
        //var genericResource = await GameEntry.Resource.LoadAsync<GameObject>("prefabs/enemy", ResourceSource.Resources);

        // 6. 下载文件到本地
        await GameEntry.Resource.DownloadFileAsync("https://pic.rmb.bdstatic.com/bjh/news/d784e0991477b5a84f4ed1de4f1693c7.png", "downloads/update.zip");

        // 7. 实例化网络下载的预制体
        var networkInstance = await GameEntry.Resource.InstantiateAsync(
            "https://example.com/prefabs/special.prefab",
            ResourceSource.Network
        );
    }

    private void OnDestroy()
    {
        // 卸载资源
        GameEntry.Resource.Unload("Prefabs/LocalCharacter", ResourceSource.Resources);
        GameEntry.Resource.UnloadAssetBundle("characters", true);
    }
}