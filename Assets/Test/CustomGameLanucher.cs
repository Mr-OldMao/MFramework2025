using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFramework.Runtime;

public class CustomGameLanucher : GameLauncher
{ 
    public override void ShowLaunchScreen()
    {
        Debug.Log("显示框架启动前Loading界面");
    }


    public override void OnFrameworkInitialized()
    {
        Debug.Log("显示框架启动完成MainCanvas界面");

        TestEventManager  testFreamwork  = new TestEventManager();
        testFreamwork.Test();
    }

     
}
