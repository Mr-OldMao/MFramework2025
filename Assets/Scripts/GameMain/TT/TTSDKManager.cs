using MFramework.Runtime;
using System;
using TTSDK;
using TTSDK.UNBridgeLib.LitJson;
using UnityEngine;
using RankData = TTSDK.TTRank.RankData;

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
            Debug.Log("-------TTSDKManager Init");

            InitAdv();

            TT.InitSDK((_, _) =>
            {
                MMPOceanEngine.OceanPostback(MMPOceanEngine.active_event);
                Debug.LogError("-------TTSDKManager InitSDK callback succ");
            });

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


        #region 排行榜
        public void ShowRankList()
        {
            JsonData jd = new JsonData();
            jd["rankType"] = "week";
            jd["dataType"] = 0;
            jd["relationType"] = "all";
            jd["suffix"] = "分";
            jd["rankTitle"] = "本周排行榜";
            jd["zoneId"] = "default";
            TT.GetImRankList(jd, (b, s) =>
            {
                Debug.Log($"排行榜Show TODO {b},{s} ");
            });
        }

        public void SetRankListData(int value)
        {
            JsonData jd = new JsonData();
            jd["dataType"] = 0;
            jd["value"] = value;
            jd["priority"] = 0;
            jd["zoneId"] = "default";
            TT.SetImRankData(jd, (b, s) =>
            {
                Debug.Log($"排行榜Set TODO {b},{s} ");
            });
        }

        /// <summary>
        /// 获取当前用户在IM排行榜中的最高分数
        /// </summary>
        /// <param name="callback">具体分数</param>
        public void GetRankData(Action<RankData> callback)
        {
            JsonData jd = new JsonData();
            jd["dataType"] = 0;
            jd["relationType"] = "all";
            jd["pageSize"] = 30;//(0,40)
            jd["rankType"] = "week";
            jd["pageNum"] = 1;
            jd["zoneId"] = "default";
            TT.GetImRankData(jd, (ref RankData rankData) =>
            {
                Debug.LogError($"GetRankData succ");
                callback?.Invoke(rankData);
            }, (msg) =>
            {
                Debug.LogError($"GetRankData Fail msg:{msg} ");
            });
        }

        public void GetRankData(Action<int> callback)
        {
            JsonData jd = new JsonData();
            jd["dataType"] = 0;
            jd["relationType"] = "all";
            jd["pageSize"] = 30;//(0,40)
            jd["rankType"] = "week";
            jd["pageNum"] = 1;
            jd["zoneId"] = "default";
            TT.GetImRankData(jd, (ref RankData rankData) =>
            {
                Debug.LogError($"GetRankData succ");
                int vaule = 0;
                if (!string.IsNullOrEmpty(rankData?.SelfItem?.Item?.Value))
                {
                    vaule = int.Parse(rankData.SelfItem.Item.Value);
                }
                callback?.Invoke(vaule);
            }, (msg) =>
            {
                Debug.LogError($"GetRankData Fail msg:{msg} ");
            });
        }

        #endregion
    }
}
