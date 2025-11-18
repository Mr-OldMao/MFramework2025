using MFramework.Runtime;
using System.Collections;
using System.Collections.Generic;
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


        public override void OnFrameworkInitialized()
        {
            Debugger.Log("框架启动完成,即将切换主场景");
            string sceneName = "testScene";
            SceneManager.LoadSceneAsync(sceneName);
            Debugger.Log($"切换场景完成，{sceneName}");
        }
    } 
}
