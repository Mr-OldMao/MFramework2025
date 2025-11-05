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
        await Task.Delay(500);

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

        Debugger.Log($"--------------------测试异步加载Resources资源,GameEntry.Resource.LoadTextAsync()-------------------", LogType.Test);
        await TestLoadTextAsset();

        Debugger.Log($"--------------------测试异步加载.ab 资源--------------------", LogType.Test);
        await TestLoadAssetBundle();

        Debugger.Log($"--------------------测试异步加载网络资源--------------------", LogType.Test);
        await TestLoadNetworkAsset();


        Debugger.Log($"--------------------测试预加载多个资源--------------------", LogType.Test);
        await TestReloadAsset();

        return;


        
    }

    private async Task TestLoadTextAsset()
    {
        string resName = string.Empty;
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
        //var resAbFile = await GameEntry.Resource.LoadAsync<AssetBundle>(resName, ResourceSource.Resources, CallbackLoadProgress);
        AssetBundle resAbFile = await GameEntry.Resource.LoadFromAssetBundleAsync<AssetBundle>(resName, "AB", ResourceSource.Resources, CallbackLoadProgress);
        Debugger.Log($"ResAsset：{resAbFile}", LogType.Test);

        AssetBundleManifest abManifest = resAbFile.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        var allBunlds = abManifest.GetAllAssetBundles();

        foreach (var bundleName in allBunlds)
        {
            Debugger.Log($"AssetBundle资源 bundleName1：{bundleName}", LogType.Test);

            var bundle = await GameEntry.Resource.LoadFromAssetBundleAsync<GameObject>($"{m_ResoureceRootDir}/AB", bundleName, ResourceSource.Resources, CallbackLoadProgress);
            Debugger.Log($"AssetBundle资源 bundle2：{bundle}", LogType.Test);

        }
    }

    private async Task TestLoadNetworkAsset()
    {

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
        //var preloadList = new List<string> { "Prefabs/Enemy1", "Prefabs/Enemy2", "Textures/UI/Icons" };
        //await GameEntry.Resource.PreloadAsync(preloadList, progress =>
        //{
        //    Debug.Log($"预加载进度: {progress.Progress:P}");
        //});
        await Task.CompletedTask;
    }

    private void TestUnloadAsset()
    {
        // 卸载资源
        GameEntry.Resource.Unload("Prefabs/LocalCharacter", ResourceSource.Resources);
        GameEntry.Resource.UnloadAssetBundle("characters", true);
    }

    private void CallbackLoadProgress(LoadProgress result)
    {
        Debugger.Log($"----- path：{result.Path}，progress：{result.Progress}，status：{result.Status}，isDone：{result.IsDone}，Asset：{result.Asset}，error：{result.Error}");
    }
}