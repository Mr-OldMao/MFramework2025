using System;
using System.Collections;
using System.Collections.Generic;
using TTSDK;
using TTSDK.UNBridgeLib.LitJson;
using Unity.VisualScripting;
using UnityEngine;
using static StarkSDKSpace.CanIUse.StarkAdManager;

namespace GameMain
{
    public partial class TTSDKManager
    {
        private static TTSDKManager m_Instance;
        public static TTSDKManager Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new TTSDKManager();
                }
                return m_Instance;
            }
        }

        public void Init()
        {
            Debug.Log("-------TTSDKManager Init CheckScene");
            TT.Login(SuccCallback, FailCallback);



            TT.CheckScene(TTSideBar.SceneEnum.SideBar,
                (b) =>
                {
                    if (b)
                    {
                    }
                    Debug.Log($"TT.CheckScene succ b:{b}");
                }, () =>
                {
                    Debug.Log($"TT.CheckScene Completed");
                }, (p1, p2) =>
                {
                    Debug.Log($"TT.CheckScene Fail p1:{p1},p2:{p2}");
                });

            InitAdv();
        }

        private void FailCallback(string errMsg)
        {
            Debug.Log($"--- Login FailCallback errMsg:{errMsg}");
        }

        private void SuccCallback(string code, string anonymousCode, bool isLogin)
        {
            Debug.Log($"--- Login SuccCallback code:{code}, anonymousCode:{anonymousCode},isLogin:{isLogin}");
        }

        public void ShowRevisitGuide()
        {
            Debug.Log("-------TTSDKManager Init ShowRevisitGuide");
            TT.ShowRevisitGuide((b) =>
            {
                Debug.Log($"TT.ShowRevisitGuide b:{b}");
            });
        }

        public void GuideClickSidebar(Action<bool> callback)
        {
            Debug.Log("-------TTSDKManager Init NavigateToScene");
            var jd = new JsonData
            {
                ["scene"] = "sidebar",
                //["activityId"] = "cacheActivityId",
            };
            Debug.Log("jd" + jd.ToString());
            TT.NavigateToScene(jd,
             () =>
             {
                 Debug.Log($"TT.NavigateToScene succ");
                 callback?.Invoke(true);
             }, () =>
             {
                 Debug.Log($"TT.NavigateToScene Completed");
             }, (p1, p2) =>
             {
                 Debug.LogError($"TT.NavigateToScene Fail p1:{p1},p2:{p2}");
                 callback?.Invoke(false);
             });
        }

        public void ShotToast(string title, string icon = "", Action complete = null, int durationMS = 1000)
        {
            TT.ShowToast(new TTShowToastParam
            {
                duration = durationMS,
                title = title,
                complete = _ => complete?.Invoke(),
                icon = icon //success, loading, none, fail
            });
        }
    }
}
