using MFramework.Runtime;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 使用示例
/// </summary>
public class TestResourcesManager : MonoBehaviour
{
    private async void Start()
    {
        await Task.Delay(500);

        await DemoResourceLoading();

        //todo 异步加载时回调加载的进度
    }

    private async Task DemoResourceLoading()
    {
        var resourceManager = GameEntry.Resource;

        Debugger.Log("加载单个资源", LogType.Test);
        var characterPrefab = await resourceManager.LoadAssetAsync<GameObject>($"Assets/Download/prefab/ui/UIPanelMain");
        if (characterPrefab != null)
        {
            Instantiate(characterPrefab, transform.position, Quaternion.identity);
        }

        Debugger.Log("加载多个资源", LogType.Test);
        var uiAssets = new List<string>
        {
            $"Assets/Download/prefab/ui/battle/UIPanelModule1",
            $"Assets/Download/prefab/ui/common/UIPanelCommon",
            $"Assets/Download/prefab/ui/UIPanelMain",
            $"Assets/Download/prefab/entity/Entity_Cube",
        };
        var uiPrefabs = await resourceManager.LoadAssetsAsync<GameObject>(uiAssets);
        foreach (var prefab in uiPrefabs)
        {
            // 处理加载的UI预制体
            Instantiate(prefab, transform.position, Quaternion.identity);

        }

        Debugger.Log("预加载多个资源", LogType.Test);
        var preloadList = new List<string>
        {
            $"Assets/Download/prefab/ui/battle/UIPanelModule1",
            $"Assets/Download/prefab/ui/battle/UIPanelModule2",
            $"Assets/Download/prefab/ui/battle/UIPanelModule3",
            $"Assets/Download/prefab/ui/common/UIPanelCommon",
            $"Assets/Download/prefab/ui/UIPanelMain",
            $"Assets/Download/prefab/entity/Entity_Cube",
            $"Assets/Download/prefab/entity/Entity_Sphere",
        };
        await resourceManager.PreloadAssetsAsync<GameObject>(preloadList);

        Debugger.Log("卸载资源", LogType.Test);
        resourceManager.ReleaseAsset($"Assets/Download/prefab/ui/UIPanelMain");

        Debugger.Log("获取统计信息", LogType.Test);
        resourceManager.GetResourceStats();

        Debugger.Log("3秒后切换场景", LogType.Test);
        await Task.Delay(3000);
        LoadSceneExample();
    }


    private async void LoadSceneExample()
    {
        await GameEntry.Resource.LoadSceneAsync("Assets/Download/scene/testScene");
    }

    private void OnDestroy()
    {
        // 清理资源
        GameEntry.Resource.ReleaseAsset("Characters/Hero");
    }
}