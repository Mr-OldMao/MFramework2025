using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
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

        private string TestScene = "Assets/Download/scene/TestScene.unity";

        private string item = "Assets/Download/texture/atlas/item.spriteatlas";
        private string temp = "Assets/Download/texture/atlas/temp.spriteatlas";


        private async Task ShowAPI()
        {
            string resAddress = string.Empty;
            string resSceneName = string.Empty;
            List<string> resAddressList = new List<string>();

            Object resultObj = null;
            List<Object> resultObjs = null;
            GameObject resultGo = null;
            AsyncOperationHandle handle= default;

            //加载单个资源
            resultObj = await GameEntry.Resource.LoadAssetAsync<Object>(resAddress, false);
            GameEntry.Resource.LoadAssetAsync<Object>(resAddress, (Object obj) => { resultObj = obj; }, false);
            resultObj = await GameEntry.Resource.LoadAssetAsync<Object>(resAddress,
               (AsyncOperationHandle<Object> p) => { },
               (AsyncOperationHandle p) => { },
               false);

            //加载多个资源
            resultObjs = await GameEntry.Resource.LoadAssetsAsync<Object>(resAddressList, false);
            resultObjs= await GameEntry.Resource.LoadAssetsAsync<Object>(resAddressList,
               (AsyncOperationHandle<Object> p) => { },
               (AsyncOperationHandle p) => { },
               false);

            //预加载资源
            resultObjs =await GameEntry.Resource.PreloadAssetsAsync<Object>(resAddressList, false);
            GameEntry.Resource.PreloadAssetsAsync<Object>(resAddressList, (List<Object> p) => { resultObjs = p; }, false);
            GameEntry.Resource.PreloadAssetsAsync<Object>(resAddressList, (List<Object> p) => { resultObjs = p; }, (float loadPregress) => { }, false);

            //加载场景
            await GameEntry.Resource.LoadSceneAsync(resSceneName);

            //加载并实例化
            resultGo = await GameEntry.Resource.InstantiateAsset(resAddress, false);
            GameEntry.Resource.ReleaseInstance(resultGo);
            GameEntry.Resource.ReleaseInstance(handle);

            //卸载资源
            GameEntry.Resource.ReleaseAsset(resultObj);
            GameEntry.Resource.ReleaseAsset(resAddress);
            GameEntry.Resource.ReleaseAsset<Object>(resAddress,false);
            GameEntry.Resource.ReleaseAllAssets();

            //其他
            GameEntry.Resource.GetAssetHandle<Object>(resAddress, false);
            GameEntry.Resource.IsAssetLoaded(resAddress);
            GameEntry.Resource.IsAssetLoaded<Object>(resAddress, false);
            GameEntry.Resource.AddSuffixMapping(typeof(Material),".mat");
            GameEntry.Resource.GetResourceStats();
        }

        private async void OnGUI()
        {
            int width = 600;
            int height = 100;
            int curWidth = 0;
            int curHeight = 0;

            GUI.Label(new Rect(curWidth, curHeight, 1000, 100), "需要导入测试资源TestResourcesManagerAssets.unitypackage，并标记Addressable", new GUIStyle("Label") { fontSize = 36 });
            curHeight += height;
            GUI.Label(new Rect(curWidth, curHeight, 600, 100), "点击按钮测试资源加载", new GUIStyle("Label") { fontSize = 36 });
            curHeight += height;

            GUIStyle style = new GUIStyle("Button");
            style.fontSize = 36;

            if (GUI.Button(new Rect(curWidth, curHeight, width, height), "加载并实例化预制体资源Entity_Cube", style))
            {
                var Entity_Cube1 = await GameEntry.Resource.LoadAssetAsync<GameObject>(Entity_Cube, true);
                Instantiate(Entity_Cube1, transform.position, Quaternion.identity);
                Log(Entity_Cube1);
            }
            curHeight += height;


            if (GUI.Button(new Rect(curWidth, curHeight, width, height), "加载文本资源", style))
            {
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
            }
            curHeight += height;

            if (GUI.Button(new Rect(curWidth, curHeight, width, height), "加载音频资源", style))
            {
                var res = await GameEntry.Resource.LoadAssetAsync<AudioClip>(resFileAudioClip);
                Log(res);
            }
            curHeight += height;

            if (GUI.Button(new Rect(curWidth, curHeight, width, height), "加载字体资源", style))
            {
                var mainFont1 = await GameEntry.Resource.LoadAssetAsync<Font>(mainFont);
                Log(mainFont1);
            }
            curHeight += height;

            if (GUI.Button(new Rect(curWidth, curHeight, width, height), "加载TMP字体资源", style))
            {
                var mainFontSDF1 = await GameEntry.Resource.LoadAssetAsync<TMP_FontAsset>(mainFontSDF, false);
                Log(mainFontSDF1);
            }
            curHeight += height;





            if (GUI.Button(new Rect(curWidth, curHeight, width, height), "批量加载预制体资源", style))
            {
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
            }
            curHeight += height;


            if (GUI.Button(new Rect(curWidth, curHeight, width, height), "批量加载图集资源", style))
            {
                var planeAtlas = await GameEntry.Resource.LoadAssetsAsync<SpriteAtlas>(new List<string> { temp, item },
                (AsyncOperationHandle<SpriteAtlas> p) =>
                {
                    Debugger.Log($"加载图集完成，res：{p.Result},spriteCount：{p.Result.spriteCount}", LogType.Test);
                }, null);
            }
            curHeight += height;

            if (GUI.Button(new Rect(curWidth, curHeight, width, height), "预加载多个资源", style))
            {
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
            }

            curHeight += height;
            if (GUI.Button(new Rect(curWidth, curHeight, width, height), "卸载单个资源Entity_Cube", style))
            {
                GameEntry.Resource.ReleaseAsset<GameObject>(Entity_Cube);
            }
            curHeight += height;

            if (GUI.Button(new Rect(curWidth, curHeight, width, height), "卸载所有资源", style))
            {
                GameEntry.Resource.ReleaseAllAssets();
            }
            curHeight += height;

            if (GUI.Button(new Rect(curWidth, curHeight, width, height), "加载并使用图集", style))
            {
                var UIPanelMain1 = await GameEntry.Resource.LoadAssetAsync<GameObject>(UIPanelMain);
                var planeAtlas = await GameEntry.Resource.LoadAssetAsync<SpriteAtlas>(temp);
                Debugger.Log($"测试加载图集 {planeAtlas.spriteCount} 个图集", LogType.Test);
                var UIPanelMainClone = Instantiate(UIPanelMain1);
                Debugger.Log($"运行时替换图集资源", LogType.Test);
                UIPanelMainClone.transform.GetChild(0).GetComponent<Image>().sprite = planeAtlas.GetSprite("resFileImgPlane5");
                var UIPanelItem1 = await GameEntry.Resource.LoadAssetAsync<GameObject>(UIPanelItem);
                Instantiate(UIPanelItem1);
            }
            curHeight += height;
            curHeight += height;

            if (GUI.Button(new Rect(curWidth, curHeight, width, height), "加载并切换场景", style))
            {
                await GameEntry.Resource.LoadSceneAsync(TestScene);
            }
            curHeight += height;
            curHeight += height;

            if (GUI.Button(new Rect(curWidth, curHeight, width, height), "测试实例化、反实例化", style))
            {
                List<GameObject> res = new List<GameObject>();
                for (int i = 0; i < 10; i++)
                {
                    var go0 = await GameEntry.Resource.InstantiateAsset(Entity_Cube);
                    go0.transform.position = new Vector3(0, i, 0);
                    res.Add(go0);
                }

                while (res.Count > 0)
                {
                    await Task.Delay(1000);
                    GameEntry.Resource.ReleaseInstance(res[0]);
                    res.Remove(res[0]);
                }
            }
            curHeight += height;

            if (GUI.Button(new Rect(curWidth, curHeight, width, height), "测试卸载资源", style))
            {
                Debugger.Log("time1:" + System.DateTime.Now.ToString());
                for (int i = 0; i < 1000000; i++)
                {
                    await GameEntry.Resource.LoadAssetAsync<GameObject>(Entity_Sphere);
                }
                Debugger.Log("time2:" + System.DateTime.Now.ToString());

                for (int i = 0; i < 1000000; i++)
                {
                    await Addressables.LoadAssetAsync<GameObject>(Entity_Sphere).Task;
                }
                Debugger.Log("time3:" + System.DateTime.Now.ToString());

                //GameObject go = null;
                //Debugger.Log("time1:" + System.DateTime.Now.ToString());

                //for (int i = 0; i < 10000; i++)
                //{
                //    go = await GameEntry.Resource.LoadAssetAsync<GameObject>(Entity_Sphere);
                //    Instantiate(go);
                //}
                //Debugger.Log("time2:" + System.DateTime.Now.ToString());

                //for (int i = 0; i < 10000; i++)
                //{
                //    go = await Addressables.InstantiateAsync(Entity_Sphere).Task;
                //}
                //Debugger.Log("time3:" + System.DateTime.Now.ToString());



                //Debugger.Log("time1:" + System.DateTime.Now.ToString());
                //for (int i = 0; i < 100; i++)
                //{
                //    go = await GameEntry.Resource.LoadAssetAsync<GameObject>(UIPanelMain);
                //    Instantiate (go);
                //}
                //Debugger.Log("time2:" + System.DateTime.Now.ToString());

                //await Task.Delay(1000);
                //GameEntry.Resource.ReleaseAsset<GameObject>(UIPanelMain);
                //var t = GameEntry.Resource.GetAssetHandle<GameObject>(UIPanelMain);      
            }
            curHeight += height;
        }

        private void Log<T>(T res) where T : Object
        {
            Debugger.Log($"load complete {res.GetType()} ，res:{res}", LogType.Test);
        }
    }
}