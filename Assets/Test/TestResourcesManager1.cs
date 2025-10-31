//using MFramework.Runtime;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using UnityEngine;

//public class TestResourcesManager : MonoBehaviour
//{
//    private string m_ResoureceRootDir = "TestRes";
//    private async void Start()
//    {
//        //延时200毫秒
//        await System.Threading.Tasks.Task.Delay(500);

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

//        string resName = string.Empty;

//        Debugger.Log($"--------------------测试异步加载Resources资源,GameEntry.Resource.LoadTextAsync()-------------------", LogType.Test);
//        Debugger.Log($"测试异步加载.txt文本资源", LogType.Test);
//        resName = $"{m_ResoureceRootDir}/resFileTxt";
//        var resTxtFile = await GameEntry.Resource.LoadTextAsync(resName, ResourceSource.Resources, CallbackLoadProgress);
//        Debugger.Log($"ResAsset：{resTxtFile}", LogType.Test);

//        Debugger.Log($"测试异步加载.json文本资源", LogType.Test);
//        resName = $"{m_ResoureceRootDir}/resFileJson";
//        var resJsonFile = await GameEntry.Resource.LoadTextAsync(resName, ResourceSource.Resources, CallbackLoadProgress);
//        Debugger.Log($"ResAsset：{resJsonFile}", LogType.Test);

//        Debugger.Log($"测试异步加载.bytes文本资源", LogType.Test);
//        resName = $"{m_ResoureceRootDir}/resFileBytes";
//        var resByteFile = await GameEntry.Resource.LoadBytesAsync(resName, ResourceSource.Resources, CallbackLoadProgress);
//        Debugger.Log($"ResAsset：{resByteFile?.Length}", LogType.Test);


//        Debugger.Log($"--------------------测试异步加载Resources资源(通用接口) GameEntry.Resource.LoadAsync<T>-------------------", LogType.Test);
//        resName = $"{m_ResoureceRootDir}/resFileTxt";
//        Debugger.Log($"测试异步加载.txt文本资源", LogType.Test);
//        var resTxtFileNormal = await GameEntry.Resource.LoadAsync<TextAsset>(resName, ResourceSource.Resources, CallbackLoadProgress);
//        Debugger.Log($"ResAsset：{resTxtFileNormal}", LogType.Test);


//        resName = $"{m_ResoureceRootDir}/resFileJson";
//        Debugger.Log($"测试异步加载.json文本资源", LogType.Test);
//        var resJsonNormal = await GameEntry.Resource.LoadAsync<TextAsset>(resName, ResourceSource.Resources, CallbackLoadProgress);
//        Debugger.Log($"ResAsset：{resJsonNormal}", LogType.Test);

//        resName = $"{m_ResoureceRootDir}/resFileBytes";
//        Debugger.Log($"测试异步加载.bytes文本资源", LogType.Test);
//        var resBytesFileNormal = await GameEntry.Resource.LoadAsync<TextAsset>(resName, ResourceSource.Resources, CallbackLoadProgress);
//        Debugger.Log($"ResAsset：{resBytesFileNormal.bytes.Length}", LogType.Test);



//        return;


//        // 2. 带进度回调的网络资源加载
//        var networkTexture = await GameEntry.Resource.LoadFromNetworkAsync<Texture2D>(
//            "https://example.com/avatar.png",
//            progress =>
//            {
//                Debug.Log($"下载进度: {progress.Progress:P} - {progress.LoadedBytes}/{progress.TotalBytes} bytes");
//            }
//        );

//        // 3. 带进度回调的文本加载
//        var configText = await GameEntry.Resource.LoadTextAsync(
//            "Config/game.json",
//            ResourceSource.StreamingAssets,
//            progress =>
//            {
//                Debug.Log($"配置文件加载: {progress.Status}");
//            }
//        );

//        // 4. 预加载多个资源
//        var preloadList = new List<string> { "Prefabs/Enemy1", "Prefabs/Enemy2", "Textures/UI/Icons" };
//        await GameEntry.Resource.PreloadAsync(preloadList, progress =>
//        {
//            Debug.Log($"预加载进度: {progress.Progress:P}");
//        });
//    }

//    //public T LoadAssetAsync<T>(string assetPath , ResourceSourceType resourceSourceType, ResourceFileType resourceFileType)
//    //{

//    //}
//    public enum ResourceFileType
//    {
//        Txt,
//        Json,
//        Bytes,
//        AB
//    }
//    public enum ResourceSourceType
//    {
//        Resources,
//        /// <summary>
//        /// AssetBundle包 - 热更新资源，可从服务器下载
//        /// </summary>
//        AssetBundle,
//        /// <summary>
//        /// StreamingAssets文件夹 - 只读数据，配置文件和初始资源
//        /// </summary>
//        StreamingAssets,
//        /// <summary>
//        /// PersistentDataPath文件夹 - 可读写数据，用户数据和缓存
//        /// </summary>
//        PersistentData,
//        /// <summary>
//        /// 网络资源 - 实时下载的图片、音频、配置文件等
//        /// </summary>
//        Network,
//        /// <summary>
//        /// Addressable系统 - Unity的可寻址资源系统（可选）
//        /// </summary>
//        Addressables
//    }
    


//    private void CallbackLoadProgress(LoadProgress result)
//    {
//        Debugger.Log($"----- path：{result.Path}，progress：{result.Progress}，status：{result.Status}，isDone：{result.IsDone}，Asset：{result.Asset}，error：{result.Error}");
//    }

//    private async void Start11()
//    {
//        //// 1. 从Resources加载
//        var localPrefab = await GameEntry.Resource.LoadFromResourcesAsync<GameObject>("Prefabs/LocalCharacter");

//        //// 2. 从AssetBundle加载
//        //var abPrefab = await GameEntry.Resource.LoadFromAssetBundleAsync<GameObject>("characters", "prefabs/hero");

//        //// 3. 从网络加载图片
//        //var networkTexture = await GameEntry.Resource.LoadFromNetworkAsync<Texture2D>("https://example.com/avatar.png");

//        //// 4. 从StreamingAssets加载配置
//        //var configText = await GameEntry.Resource.LoadTextAsync("config/game.json", ResourceSource.StreamingAssets);

//        //// 5. 通用接口，自动判断来源
//        //var genericResource = await GameEntry.Resource.LoadAsync<GameObject>("prefabs/enemy", ResourceSource.Resources);

//        // 6. 下载文件到本地
//        await GameEntry.Resource.DownloadFileAsync("https://pic.rmb.bdstatic.com/bjh/news/d784e0991477b5a84f4ed1de4f1693c7.png", "downloads/update.zip");

//        // 7. 实例化网络下载的预制体
//        var networkInstance = await GameEntry.Resource.InstantiateAsync(
//            "https://example.com/prefabs/special.prefab",
//            ResourceSource.Network
//        );
//    }

//    private void OnDestroy()
//    {
//        // 卸载资源
//        GameEntry.Resource.Unload("Prefabs/LocalCharacter", ResourceSource.Resources);
//        GameEntry.Resource.UnloadAssetBundle("characters", true);
//    }
//}