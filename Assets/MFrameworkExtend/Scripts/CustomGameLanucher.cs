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

            GameEntry.Scene.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        }



    }
}
