using UnityEngine;
using UnityEngine.SceneManagement;

namespace MFramework.Runtime
{
    public class TestSceneManager : MonoBehaviour
    {
        private const string TestScene = "TestScene";
        private const string TestSubScene = "TestSubScene";

        private async void ShowAPI()
        {
            // 1. 加载单一场景
            GameEntry.Scene.LoadSceneAsync("MainMenu",
                LoadSceneMode.Single,
                (progress) => Debug.Log($"加载进度: {progress:P0}"),
                () => Debug.Log("主菜单加载完成"),
                ()=> Debug.Log("自动卸载完成单一场景回调，仅对LoadSceneMode.Single返回")
                ,1.0f); // 最小加载时间1秒

            // 2. 叠加加载UI场景
            GameEntry.Scene.LoadSceneAsync("UI_Overlay",
                LoadSceneMode.Additive,
                progress => Debug.Log($"UI加载进度: {progress:P0}"),
                () => Debug.Log("UI叠加场景加载完成"),
                 ()=> Debug.Log("自动卸载完成单一场景回调，仅对LoadSceneMode.Single返回")
                , 1.0f); // 最小加载时间1秒

            // 3. 预加载下一关卡
            GameEntry.Scene.PreloadSceneAsync("Level2",
                LoadSceneMode.Single,
                progress => Debug.Log($"预加载进度: {progress:P0}"),
                () => Debug.Log("下一关卡预加载完成"));

            GameEntry.Scene.PreloadSceneAsync("Level2",
                LoadSceneMode.Additive,
                progress => Debug.Log($"预加载进度: {progress:P0}"),
                () => Debug.Log("下一关卡预加载完成"));

            // 4. 卸载场景
            GameEntry.Scene.UnloadSceneAsync("UI_Overlay",
                progress => Debug.Log($"卸载进度: {progress:P0}"),
                () => Debug.Log("UI场景卸载完成"));

            // 5. 重新加载当前场景
            GameEntry.Scene.ReloadCurrentScene(
                LoadSceneMode.Additive,
                progress => Debug.Log($"重新加载进度: {progress:P0}"),
                () => Debug.Log("场景重新加载完成"),
                2.0f); // 最小加载时间2秒

            // 6. 检查场景状态
            if (GameEntry.Scene.IsSceneLoaded("MainMenu"))
            {
                Debug.Log("主菜单已加载");
            }

            // 7. 获取所有已加载的场景
            var loadedScenes = GameEntry.Scene.GetAllLoadedScenes();
            foreach (var scene in loadedScenes)
            {
                Debug.Log($"已加载场景: {scene}");
            }

            // 8. 获取所有正在加载的场景
            var loadingScenes = GameEntry.Scene.GetAllLoadedScenes();
            foreach (var scene in loadedScenes)
            {
                Debug.Log($"正在加载场景: {scene}");
            }
        }

        // 模拟游戏中的场景切换
        public void StartGame()
        {

            GameEntry.Scene.LoadSceneAsync("GameLevel",
                LoadSceneMode.Single,
                ShowLoadingScreen,
                OnGameLevelLoaded);
        }

        private void ShowLoadingScreen(float progress)
        {
            // 更新UI加载进度条
            Debug.Log($"游戏关卡加载进度: {progress:P0}");
        }

        private void OnGameLevelLoaded()
        {
            Debug.Log("游戏关卡加载完成，开始游戏！");
            // 初始化游戏逻辑...
        }

        public void ReturnToMainMenu()
        {
            GameEntry.Scene.LoadSceneAsync("MainMenu",
                LoadSceneMode.Single,
                null, // 不需要进度回调
                () => Debug.Log("返回主菜单完成"));
        }

        // Start is called before the first frame update
        private async void OnGUI()
        {
            GUIStyle style = new GUIStyle("Button");
            style.fontSize = 36;

            int width = 800;
            int height = 100;

            int curWidth = 0;
            int curHeight = 0;


            if (GUI.Button(new Rect(curWidth, curHeight, width, height), $"切换单一场景,限制最短加载时长不低于2秒", style))
            {
                GameEntry.Scene.LoadSceneAsync(TestSubScene, LoadSceneMode.Single,
                               (progress) =>
                               {
                                   Debugger.Log($"加载进度: {progress:P0}");
                               }, () =>
                               {
                                   Debugger.LogError("场景加载完成，3s后切换场景TestScene");
                                   GameEntry.Timer.AddDelayTimer(3f, () =>
                                   {
                                       GameEntry.Scene.LoadSceneAsync(TestScene, LoadSceneMode.Single, (progress) =>
                                       {
                                           Debugger.Log($"加载进度: {progress:P0}");
                                       }, () =>
                                       {
                                           Debugger.Log("场景加载完成");
                                       });
                                   });
                               }, () =>
                               {
                                   Debugger.Log("自动卸载完成单一场景回调，仅对LoadSceneMode.Single返回");
                               }, 2f);
            }
            curHeight += height;

            if (GUI.Button(new Rect(curWidth, curHeight, width, height), $"预加载单一场景,加载时长无限制", style))
            {
                GameEntry.Scene.PreloadSceneAsync(TestSubScene, LoadSceneMode.Single, (progress) =>
                {
                    Debugger.Log($"预加载场景中，加载进度: {progress:P0}");
                }, () =>
                {
                    Debugger.Log($"预加载场景完成");
                });
            }

            curHeight += height;
            curHeight += height;

            if (GUI.Button(new Rect(curWidth, curHeight, width, height), $"叠加新场景,加载时长无限制", style))
            {
                GameEntry.Scene.LoadSceneAsync(TestSubScene,
                               LoadSceneMode.Additive,
                               (progress) =>
                               {
                                   Debugger.Log($"加载进度: {progress:P0}");
                               }, () =>
                               {
                                   Debugger.LogError("场景加载完成，3s后卸载场景TestSubScene");
                                   GameEntry.Timer.AddDelayTimer(3f, () =>
                                   {
                                       GameEntry.Scene.UnloadSceneAsync(TestSubScene,(progress) =>
                                       {
                                           Debug.Log($"卸载进度: {progress:P0}");
                                       }, () =>
                                       {
                                           Debug.Log("场景卸载完成");
                                       });
                                   });
                               }, () =>
                               {
                                   Debugger.Log("自动卸载完成单一场景回调，仅对LoadSceneMode.Single返回");
                               }, 0);
            }
            curHeight += height;
            if (GUI.Button(new Rect(curWidth, curHeight, width, height), $"预加载新增场景,加载时长无限制", style))
            {
                GameEntry.Scene.PreloadSceneAsync(TestSubScene, LoadSceneMode.Additive, (progress) =>
                {
                    Debugger.Log($"预加载场景中，加载进度: {progress:P0}");
                }, () =>
                {
                    Debugger.Log($"预加载场景完成");
                });
            }

            curHeight += height;
            curHeight += height;

            if (GUI.Button(new Rect(curWidth, curHeight, width, height), $"重新加载当前场景", style))
            {
                GameEntry.Scene.ReloadCurrentScene(LoadSceneMode.Additive, (progress) =>
                {
                    Debugger.Log($"重新加载场景中，加载进度: {progress:P0}");
                }, () =>
                {
                    Debugger.Log($"重新加载场景完成");
                });
            }
        }
    }
}
