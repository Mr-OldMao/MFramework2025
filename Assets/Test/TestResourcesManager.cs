using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MFramework.Runtime
{
    /// <summary>
    /// 使用示例
    /// </summary>
    public class TestResourcesManager : MonoBehaviour
    {
        private string resFileAudioClip = "Assets/Download/audio/bgm/resFileAudioClip.ogg";

        private string resFileBytes = "Assets/Download/config/data/resFileBytes.bytes";
        private string resFileTxt = $"Assets/Download/base/resFileTxt.txt";
        private string resFileJson = $"Assets/Download/config/language/resFileJson.json";

        private string mainFont = "Assets/Download/font/mainFont.ttf";
        private string mainFontSDF = "Assets/Download/font/mainFontSDF.asset";

        private string Entity_Cube = "Assets/Download/prefab/entity/Entity_Cube";
        private string Entity_Sphere = "Assets/Download/prefab/entity/Entity_Sphere.prefab";

        private string UIPanelItem = "Assets/Download/prefab/ui/battle/UIPanelItem.prefab";
        private string UIPanelModule1 = "Assets/Download/prefab/ui/battle/UIPanelModule1";
        private string UIPanelModule2 = "Assets/Download/prefab/ui/battle/UIPanelModule2";
        private string UIPanelCommon = "Assets/Download/prefab/ui/common/UIPanelCommon.prefab";
        private string UIPanelMain = "Assets/Download/prefab/ui/UIPanelMain.prefab";

        private string testScene = "Assets/Download/scene/testScene.unity";

        private string item = "Assets/Download/texture/atlas/item.spriteatlas";
        private string plane = "Assets/Download/texture/atlas/plane.spriteatlas";



        private async void Start()
        {
            await Task.Delay(3000);

            await TestLoadResType();

            await TestLoadAltas();

            //TODO 自动卸载资源
        }


        /// <summary>
        /// 测试加载不同资源类型
        /// </summary>
        /// <returns></returns>
        private async Task TestLoadResType()
        {
            Debugger.Log("测试加载不同资源类型", LogType.Test);
            var preloadList2 = new List<string>
        {
            resFileTxt,
            resFileJson,
            resFileBytes
        };
            GameEntry.Resource.PreloadAssetsAsync<TextAsset>(preloadList2, (List<TextAsset> textAssets) =>
            {
                foreach (var res in textAssets)
                {
                    Debugger.Log($"load complete TextAsset , txt: {res.text} , length:{res.bytes.Length}", LogType.Test);
                }
            });


            var res = await GameEntry.Resource.LoadAssetAsync<AudioClip>(resFileAudioClip);
            Log(res);

            var mainFont1 = await GameEntry.Resource.LoadAssetAsync<Font>(mainFont);
            Log(mainFont1);

            var mainFontSDF1 = await GameEntry.Resource.LoadAssetAsync<TMP_FontAsset>(mainFontSDF, false);
            Log(mainFontSDF1);

            var Entity_Cube1 = await GameEntry.Resource.LoadAssetAsync<GameObject>(Entity_Cube, true);
            Instantiate(Entity_Cube1, transform.position, Quaternion.identity);
            Log(Entity_Cube1);

            Debugger.Log("批量加载资源", LogType.Test);
            var uiAssets = new List<string>
        {
            UIPanelItem,
            UIPanelModule1,
            UIPanelModule2,
        };
            var uiPrefabs = await GameEntry.Resource.LoadAssetsAsync<GameObject>(uiAssets);
            foreach (var prefab in uiPrefabs)
            {
                // 处理加载的UI预制体
                Instantiate(prefab, transform.position, Quaternion.identity);
                Log(prefab);
            }


            var planeAtlas = await GameEntry.Resource.LoadAssetsAsync<SpriteAtlas>(new List<string> { plane, item },
                 (AsyncOperationHandle<SpriteAtlas> p) =>
                 {
                     Debugger.Log($"加载图集完成，res：{p.Result},spriteCount：{p.Result.spriteCount}", LogType.Test);
                 }, null);


            Debugger.Log("预加载多个资源", LogType.Test);
            var preloadList = new List<string>
        {
            UIPanelCommon,
            UIPanelMain,
            UIPanelItem,
            UIPanelModule1,
            UIPanelModule2,
            Entity_Sphere
        };
            GameEntry.Resource.PreloadAssetsAsync<GameObject>(preloadList, (List<GameObject> p) =>
            {
                foreach (var prefab in p)
                {
                    Debugger.Log($"预加载资源完成,res：{prefab}", LogType.Test);
                }

            }, (float loadPregressCallback) =>
            {
                Debugger.Log($"预加载资源进度，{loadPregressCallback}");
            });


            Debugger.Log($"-----------------------测试卸载资源，5秒后卸载单个资源{Entity_Cube}", LogType.Test);
            await Task.Delay(5000);
            Debugger.Log($"卸载单个资源{Entity_Cube}", LogType.Test);
            GameEntry.Resource.ReleaseAsset<GameObject>(Entity_Cube);

            Debugger.Log("获取统计信息", LogType.Test);
            GameEntry.Resource.GetResourceStats();

            Debugger.Log("\"-----------------------5秒后卸载所有资源", LogType.Test);
            await Task.Delay(5000);
            Debugger.Log("卸载所有资源", LogType.Test);
            GameEntry.Resource.ReleaseAllAssets();


            Debugger.Log($"-----------------------5秒后切换场景", LogType.Test);
            await Task.Delay(5000);
            await GameEntry.Resource.LoadSceneAsync(testScene);
        }

        private void Log<T>(T res) where T : Object
        {
            Debugger.Log($"load complete {res.GetType()} ，res:{res}", LogType.Test);
        }

        /// <summary>
        /// 测试加载图集
        /// </summary>
        /// <returns></returns>
        private async Task TestLoadAltas()
        {
            Debugger.Log("测试加载图集", LogType.Test);

            var UIPanelMain1 = await GameEntry.Resource.LoadAssetAsync<GameObject>(UIPanelMain);
            var planeAtlas = await GameEntry.Resource.LoadAssetAsync<SpriteAtlas>(plane);
            Debugger.Log($"测试加载图集 {planeAtlas.spriteCount} 个图集", LogType.Test);
            var UIPanelMainClone = Instantiate(UIPanelMain1);
            Debugger.Log($"运行时替换图集资源", LogType.Test);
            UIPanelMainClone.transform.GetChild(0).GetComponent<Image>().sprite = planeAtlas.GetSprite("resFileImgPlane5");


            var UIPanelItem1 = await GameEntry.Resource.LoadAssetAsync<GameObject>(UIPanelItem);
            Instantiate(UIPanelItem1);
        }
    } 
}