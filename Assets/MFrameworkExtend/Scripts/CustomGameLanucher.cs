using MFramework.Runtime;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameMain
{
    public class CustomGameLanucher : GameLauncher
    {
        public override void ShowLaunchScreen()
        {
            Debug.Log("显示框架启动前Loading界面");
        }


        public override async void OnFrameworkInitialized()
        {
            Debugger.Log("框架启动完成,即将切换主场景");
            string sceneName = "MainScene";

            await DataTools.Init();

            await GameEntry.UI.ShowViewAsync<UIPanelLoad>();

            GameEntry.Scene.LoadSceneAsync(sceneName, LoadSceneMode.Single, (p) =>
            {
                Debug.Log($"progress:{p}");
                GameEntry.UI.GetModel<UIModelLoad>().SetLoadingProgress(p);
            }, async () =>
            {
                GameEntry.UI.GetModel<UIModelLoad>().SetLoadingProgress(1f);
                await GameEntry.UI.ShowViewAsync<UIPanelMap>();
                GameEntry.UI.HideView<UIPanelLoad>();
            },null,3f);
        }
    }
}
