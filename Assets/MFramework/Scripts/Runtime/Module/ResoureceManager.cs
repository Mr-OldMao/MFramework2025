//// 具体模块实现 - 资源管理器（不可更新）
//using UnityEngine;

//public class ResourceManager : GameModuleBase, IResourceManager
//{
//    public override int Priority => 20;

//    protected override async Task OnInitialize()
//    {
//        Debug.Log("资源管理器初始化中...");
//        // 初始化资源系统
//        await Task.Delay(200);
//        Debug.Log("资源管理器初始化完成");
//    }

//    protected override void OnShutdown()
//    {
//        Debug.Log("资源管理器已关闭");
//    }

//    public GameObject LoadPrefab(string path)
//    {
//        // 资源加载逻辑
//        return null;
//    }
//}