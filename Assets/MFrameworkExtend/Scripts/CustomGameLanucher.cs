using MFramework.Runtime;
using MiniGameSDK;
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
            //GameEntry.Scene.LoadSceneAsync("FPS_Example");
            //return;

            await DataTools.Init();

            var uiPanelLoad = await GameEntry.UI.ShowViewAsync<UIPanelLoad>();
            uiPanelLoad.ShowLoadSlider();

            GameEntry.UI.GetModel<UIModelSettlement>().InitData();
            SDKManager.Instance.InitSDK(p =>
            {
                SDKManager.GetSpecial<DouyinSDK>().OnInitCompletedCallback();
            });
            GameEntry.Timer.AddDelayTimer(3f, () =>
            {
                Debug.LogError("--showAdvBanner 3f");
                SDKManager.Instance.ShowAdvBanner();
            });
            GameEntry.Timer.AddDelayTimer(10f, () =>
            {
                Debug.LogError("--showAdvBanner 10f");
                SDKManager.Instance.ShowAdvBanner();
            });
            GameEntry.Timer.AddDelayTimer(20f, () =>
            {
                Debug.LogError("--showAdvBanner 20f");
                SDKManager.Instance.ShowAdvBanner();
            });
            GameEntry.Timer.AddDelayTimer(31f, () =>
            {
                Debug.LogError("--showAdvInsert 31f");
                SDKManager.Instance.ShowAdvInsert();
            });

            GameEntry.Scene.LoadSceneAsync(sceneName, LoadSceneMode.Single, (p) =>
            {
                //Debug.Log($"progress:{p}");
                GameEntry.UI.GetModel<UIModelLoad>().SetLoadingProgress(p);
            }, async () =>
            {
                GameEntry.UI.GetModel<UIModelLoad>().SetLoadingProgress(1f);
                await GameEntry.UI.ShowViewAsync<UIPanelMap>();
                //await GameEntry.UI.ShowViewAsync<UIPanelGM>();
                await GameEntry.UI.ShowViewAsync<UIPanelMenu>();
                uiPanelLoad.HidePanel();
            }, null, 1f);
        }
    }
}
