//using MFramework.Runtime;
//using System.IO;
//using System.Threading.Tasks;
//using UnityEngine;

//public class TestResourcesManager : MonoBehaviour
//{
//    private string m_ResoureceRootDir = "TestRes";

//    private async void Start()
//    {
//        await Task.Delay(500);

//        //资源所在位置
//        //Resources,
//        //AssetBundle,
//        //StreamingAssets,
//        //PersistentData,
//        //Network,
//        //Addressables

//        //读取的文件资源类型
//        //.txt文件
//        //.json文件
//        //.bytes文件
//        //.ab文件

//        //

//        Debugger.Log($"--------------------测试异步加载文本资源(Resources目录下)-------------------", LogType.Test);
//        await TestLoadTextAsset(ResourceSource.Resources);
//        Debugger.Log($"--------------------测试异步加载文本资源(StreamingAssets目录下)-------------------", LogType.Test);
//        await TestLoadTextAsset(ResourceSource.StreamingAssets);
//        Debugger.Log($"--------------------测试异步加载文本资源(PersistentData目录下)-------------------", LogType.Test);
//        await TestLoadTextAsset(ResourceSource.PersistentData);


//        Debugger.Log($"--------------------测试异步加载Unity资源(Resources目录下)-------------------", LogType.Test);
//        await TestLoadUnityAsset(ResourceSource.Resources);
//        Debugger.Log($"--------------------测试异步加载Unity资源(StreamingAssets目录下,PS:不支持加载部分Unity资源，包括但是不限于.audioClip,.prefab)-------------------", LogType.Test);
//        await TestLoadUnityAsset(ResourceSource.StreamingAssets);
//        Debugger.Log($"--------------------测试异步加载Unity资源(PersistentData目录下,PS:不支持加载部分Unity资源，包括但是不限于.audioClip,.prefab)-------------------", LogType.Test);
//        await TestLoadUnityAsset(ResourceSource.PersistentData);


//        Debugger.Log($"--------------------测试异步加载.ab 资源(Resources目录下)--------------------", LogType.Test);
//        await TestLoadAssetBundle(ResourceSource.Resources);
//        Debugger.Log($"--------------------测试异步加载.ab 资源(StreamingAssets目录下)--------------------", LogType.Test);
//        await TestLoadAssetBundle(ResourceSource.StreamingAssets);
//        Debugger.Log($"--------------------测试异步加载.ab 资源(PersistentData目录下)--------------------", LogType.Test);
//        await TestLoadAssetBundle(ResourceSource.PersistentData);


//        Debugger.Log($"--------------------测试异步加载网络资源--------------------", LogType.Test);
//        await TestLoadNetworkAsset();


//        //todo 测试预加载多个资源
//        //Debugger.Log($"--------------------测试预加载多个资源--------------------", LogType.Test);
//        //await TestReloadAsset();

//        //todo 测试资源卸载

//        return;
//    }

//    private async Task TestLoadTextAsset(ResourceSource resSource)
//    {
//        bool isFileExtension = resSource != ResourceSource.Resources;

//        string resName = string.Empty;
//        Debugger.Log($"测试异步加载.txt文本资源", LogType.Test);
//        resName = isFileExtension ? $"{m_ResoureceRootDir}/resFileTxt.txt" : $"{m_ResoureceRootDir}/resFileTxt";
//        var resTxtFile = await GameEntry.Resource.LoadTextAsync(resName, resSource, CallbackLoadProgress);

//        Debugger.Log($"测试异步加载.json文本资源", LogType.Test);
//        resName = isFileExtension ? $"{m_ResoureceRootDir}/resFileJson.json" : $"{m_ResoureceRootDir}/resFileJson";
//        var resJsonFile = await GameEntry.Resource.LoadTextAsync(resName, resSource, CallbackLoadProgress);

//        Debugger.Log($"测试异步加载.bytes文本资源", LogType.Test);
//        resName = isFileExtension ? $"{m_ResoureceRootDir}/resFileBytes.bytes" : $"{m_ResoureceRootDir}/resFileBytes";
//        var resByteFile = await GameEntry.Resource.LoadBytesAsync(resName, resSource, CallbackLoadProgress);
//    }


//    private async Task TestLoadUnityAsset(ResourceSource resSource)
//    {
//        bool isFileExtension = resSource != ResourceSource.Resources;

//        string resName = string.Empty;
//        resName = isFileExtension ? $"{m_ResoureceRootDir}/resFileTxt.txt" : $"{m_ResoureceRootDir}/resFileTxt";
//        Debugger.Log($"测试异步加载.txt文本资源", LogType.Test);
//        var resTxtFileNormal = await GameEntry.Resource.LoadAsync<TextAsset>(resName, resSource, CallbackLoadProgress);

//        resName = isFileExtension ? $"{m_ResoureceRootDir}/resFileJson.json" : $"{m_ResoureceRootDir}/resFileJson";
//        Debugger.Log($"测试异步加载.json文本资源", LogType.Test);
//        var resJsonNormal = await GameEntry.Resource.LoadAsync<TextAsset>(resName, resSource, CallbackLoadProgress);

//        resName = isFileExtension ? $"{m_ResoureceRootDir}/resFileBytes.bytes" : $"{m_ResoureceRootDir}/resFileBytes";
//        Debugger.Log($"测试异步加载.bytes文本资源", LogType.Test);
//        var resBytesFileNormal = await GameEntry.Resource.LoadAsync<TextAsset>(resName, resSource, CallbackLoadProgress);

//        Debugger.Log($"测试异步加载.img图片资源", LogType.Test);
//        resName = isFileExtension ? $"{m_ResoureceRootDir}/resFileImg.png" : $"{m_ResoureceRootDir}/resFileImg";
//        var resImgFile = await GameEntry.Resource.LoadAsync<Texture>(resName, resSource, CallbackLoadProgress);

//        Debugger.Log($"测试异步加载.audioClip音频资源", LogType.Test);
//        resName = isFileExtension ? $"{m_ResoureceRootDir}/resFileImg.png" : $"{m_ResoureceRootDir}/resFileAudioClip";
//        var resAudioFile = await GameEntry.Resource.LoadAsync<AudioClip>(resName, resSource, CallbackLoadProgress);
        
//        Debugger.Log($"测试异步加载.prefab预制体资源", LogType.Test);
//        resName = isFileExtension ? $"{m_ResoureceRootDir}/resFilePrefab.prefab" : $"{m_ResoureceRootDir}/resFilePrefab";
//        var resPrefabFile = await GameEntry.Resource.LoadAsync<GameObject>(resName, resSource, CallbackLoadProgress);
//    }

//    private async Task TestLoadAssetBundle(ResourceSource resSource)
//    {
//        //1.加载ab包，T：AssetBundle，bundlePath为ab文件路径，assetPath为空
//        //2.加载ab包内资源，T:Gameobject，bundlePath为ab文件路径，assetPath为ab包内资源路径


//        //TODO 把加载ab包，和加载ab包内资源分开，因为加载ab包内资源需要ab包的依赖，所以需要先加载ab包，再加载ab包内资源

//        string resDir = string.Empty;
//        resDir = $"{m_ResoureceRootDir}/AB";
//        AssetBundle resAbFile = await GameEntry.Resource.LoadFromAssetBundleAsync<AssetBundle>(resDir + "/AB", "", resSource, CallbackLoadProgress);
//        Debugger.Log($"ResAsset：{resAbFile}", LogType.Test);

//        AssetBundleManifest abManifest = resAbFile.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
//        var allBunlds = abManifest.GetAllAssetBundles();

//        foreach (var subBundleName in allBunlds)
//        {
//            Debugger.Log($"开始加载AssetBundle资源，bundleName：{subBundleName}", LogType.Test);

//            var bundle = await GameEntry.Resource.LoadFromAssetBundleAsync<AssetBundle>($"{m_ResoureceRootDir}/AB/{subBundleName}", "", resSource, CallbackLoadProgress);
//            Debugger.Log($"加载AssetBundle资源完成，bundleName：{bundle}", LogType.Test);
//        }

//        //异步加载ab资源
//        Debugger.Log($"加载Prefab资源开始", LogType.Test);

//        string resName = "UIPanelTestSub1";

//        var go = await GameEntry.Resource.LoadFromAssetBundleAsync<GameObject>($"{m_ResoureceRootDir}/AB/folder_uiprefab", resName, resSource, CallbackLoadProgress);
//        Instantiate(go);
//        Debugger.Log($"加载Prefab资源完成：go：{go}", LogType.Test);

//    }

//    private async Task TestLoadNetworkAsset()
//    {
//        Debugger.Log($"测试网络资源下载到本地", LogType.Test);
//        string localPath = "downloads/test.png";
//        await GameEntry.Resource.DownloadFileAsync("https://pic.rmb.bdstatic.com/bjh/news/d784e0991477b5a84f4ed1de4f1693c7.png", localPath, (progress) =>
//        {
//            Debug.Log($"网络资源下载中,下载进度: {progress.Progress:P} - {progress.LoadedBytes}/{progress.TotalBytes} bytes");
//            if (progress.IsDone)
//            {
//                Debugger.Log($"网络资源下载完成,directory:{Path.GetDirectoryName(localPath)}/{localPath}", LogType.Test);
//            }
//        });

//        Debugger.Log($"测试加载网络资源", LogType.Test);
//        var testNeworkRes = await GameEntry.Resource.LoadAsync<Texture2D>("https://pic.rmb.bdstatic.com/bjh/news/d784e0991477b5a84f4ed1de4f1693c7.png",
//            ResourceSource.Network, CallbackLoadProgress);
//        Debugger.Log($"网络资源加载完成,testNeworkRes：{testNeworkRes}", LogType.Test);
//    }

//    /// <summary>
//    /// 预加载多个资源
//    /// </summary>
//    /// <returns></returns>
//    private async Task TestReloadAsset()
//    {
//        //var preloadList = new List<string> { "Prefabs/Enemy1", "Prefabs/Enemy2", "Textures/UI/Icons" };
//        //await GameEntry.Resource.PreloadAsync(preloadList, progress =>
//        //{
//        //    Debug.Log($"预加载进度: {progress.Progress:P}");
//        //});
//        await Task.CompletedTask;
//    }

//    private void TestUnloadAsset()
//    {
//        // 卸载资源
//        GameEntry.Resource.Unload("Prefabs/LocalCharacter", ResourceSource.Resources);

//        string resDir = string.Empty;
//        resDir = $"{m_ResoureceRootDir}/AB";
//        GameEntry.Resource.UnloadAssetBundle(resDir, "test/uipaneltestmain", true);
//    }

//    private void CallbackLoadProgress(LoadProgress result)
//    {
//        //Debugger.Log($"-------- Progress：{result.Progress:P}，Status：{result.Status}，IsDone：{result.IsDone}，Path：{result.Path}");
//        if (result.IsDone)
//        {
//            if (result.Asset != null)
//            {
//                Debugger.Log($"-------- Load Complete，Asset：{result.Asset}", LogType.Test);
//            }
//            else if (!string.IsNullOrEmpty(result.Text))
//            {
//                Debugger.Log($"-------- Load Complete，TextAsset：{result.Text}", LogType.Test);
//            }
//            else if (result.Bytes != null)
//            {
//                Debugger.Log($"-------- Load Complete，BytesAsset.Length：{result.Bytes.Length}", LogType.Test);
//            }
//            else
//            {
//                Debugger.Log($"-------- Load Complete，error：{result.Error}", LogType.Test);
//            }
//        }
//    }
//}