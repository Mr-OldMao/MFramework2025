using System;
using System.Collections.Generic;
using TTSDK;
using UnityEngine;

namespace GameMain
{
    public partial class TTSDKManager
    {
        public static string AdvBannerID = "11r92jkkidfcd1dfe7";
        public static string AdvVideoID = "1f49fefc9hhgefggib";
        public static string AdvInterstitialID = "eaf3j2069d6l3j1cek";

        //private TTBannerAd bannerAd;
        //private TTInterstitialAd interstitialAd;
        //private TTRewardedVideoAd rewardedVideoAd;

        private Dictionary<string, AdvBannerData> m_DicAdvBannerData = new Dictionary<string, AdvBannerData>();
        private Dictionary<string, AdvVideoData> m_DicAdvVideoData = new Dictionary<string, AdvVideoData>();
        private Dictionary<string, AdvInterstitialData> m_DicAdvInsertData = new Dictionary<string, AdvInterstitialData>();

        private AdvEventInfo m_TargetAdvEventInfo;


        public class AdvBannerData
        {
            public TTBannerAd adBanner;
            public AdvEventInfo advEventInfo;
        }
        public class AdvVideoData
        {
            public TTRewardedVideoAd adVideo;
            public AdvEventInfo advEventInfo;
        }
        public class AdvInterstitialData
        {
            public TTInterstitialAd adInterstitial;
            public AdvEventInfo advEventInfo;
        }

        public class AdvEventInfo
        {
            public Action closeCallback; //关闭插入、banner广告
            public Action<bool, int> closeVideoCallback;//关闭激励视频
            public Action loadCallback;
            public Action<int, string> errorCallback;
        }

        private void InitAdv()
        {
            CreateAdvBanner(AdvBannerID);
            CreateAdvVideo(AdvVideoID);
            CreateAdvInsert(AdvInterstitialID);
        }

        private void CreateAdvBanner(string advID)
        {
            if (string.IsNullOrEmpty(advID))
            {
                Debug.LogError("-----CreateAdvBanner advID is null or empty");
                return;
            }
            Debug.LogError($"----- CreateAdvBanner A");

            if (m_DicAdvBannerData.ContainsKey(advID))
            {
                Debug.Log("---- CreateAdvBanner 已存在");
                return;
                //Debug.LogError($"-----Adv Destroy A");
                ////m_DicAdvBannerData[advID].adBanner.Destroy();
                //m_DicAdvBannerData[advID] = null;
                //m_DicAdvBannerData.Remove(advID);
                //Debug.LogError($"-----Adv Destroy B");
            }

            TTBannerStyle style = new TTBannerStyle
            {
                top = 0,
                left = 0,//Screen.width / 2,
                width = 208,//150
            };
            AdvBannerData advData = new AdvBannerData
            {
                adBanner = TT.CreateBannerAd(new CreateBannerAdParam()
                {
                    BannerAdId = AdvBannerID,
                    Style = style,
                    AdIntervals = 30
                }),
                advEventInfo = new AdvEventInfo()
            };
            //Debug.LogError($"BannerAdv style.top:{style.top},style.left:{style.left}");

            advData.adBanner.OnClose += () =>
            {
                Debug.LogError($"-----Adv OnClose Banner广告关闭");
                m_TargetAdvEventInfo?.closeCallback?.Invoke();
                m_TargetAdvEventInfo = null;

                Debug.LogError($"-----Adv OnClose Banner广告关闭 创建新实例A");
                //CreateAdvBanner(AdvBannerID);
                Debug.LogError($"-----Adv OnClose Banner广告关闭 创建新实例B");
            };
            advData.adBanner.OnError += (errorCode, msg) =>
            {
                Debug.LogError($"-----Adv OnError Banner广告出错 errorCode:{errorCode},msg:{msg}");
                m_TargetAdvEventInfo?.errorCallback?.Invoke(errorCode, msg);
                m_TargetAdvEventInfo = null;
            };
            advData.adBanner.OnLoad += () =>
            {
                Debug.LogError($"-----Adv OnLoad Banner广告加载");
                advData.advEventInfo?.loadCallback?.Invoke();
            };
            //advData.adBanner.OnResize += (int width, int height) =>
            //{
            //    Debug.LogError($"-----Adv OnResize Banner,Screen.width:{Screen.width},Screen.height:{Screen.height}, width:{width},height:{height}");

            //    int left = Screen.width - width;
            //    int top = (Screen.height - height)/2;
            //    advData.adBanner.ReSize(new TTBannerStyle
            //    {
            //        left = left,
            //        top = top,
            //        width = 208,
            //    });
            //};
            
            m_DicAdvBannerData.Add(advID, advData);
        }

        private void CreateAdvVideo(string advID)
        {
            if (string.IsNullOrEmpty(advID))
            {
                Debug.LogError("-----CreateAdvVideo advID is null or empty");
                return;
            }
            AdvVideoData advData = new AdvVideoData
            {
                adVideo = TT.CreateRewardedVideoAd(new CreateRewardedVideoAdParam()
                {
                    AdUnitId = AdvVideoID,

                    ProgressTip = true,
                    Multiton = true,
                    MultitonRewardMsg = new List<string>() { "测试再得广告奖励文案1", "测试再得广告奖励文案2", "测试再得广告奖励文案3" },
                    MultitonRewardTimes = 4
                }),
                advEventInfo = new AdvEventInfo()
            };

            advData.adVideo.OnClose += (bool isEnded, int count) =>
            {
                Debug.Log($"-----Adv OnClose 激励广告关闭 isEnded:{isEnded},count:{count}");
                if (isEnded)
                {
                    MMPOceanEngine.OceanPostback(MMPOceanEngine.game_addiction_event);
                }
                m_TargetAdvEventInfo?.closeVideoCallback?.Invoke(isEnded, count);
                m_TargetAdvEventInfo = null;
            };
            advData.adVideo.OnError += (errorCode, msg) =>
            {
                Debug.LogError($"-----Adv OnError 激励广告出错 errorCode:{errorCode},msg:{msg}");
                m_TargetAdvEventInfo?.errorCallback?.Invoke(errorCode, msg);
                m_TargetAdvEventInfo = null;
            };
            advData.adVideo.OnLoad += () =>
            {
                Debug.Log($"-----Adv OnError 激励广告加载");
                m_TargetAdvEventInfo?.loadCallback?.Invoke();
            };
            m_DicAdvVideoData.Add(advID, advData);
        }

        private void CreateAdvInsert(string advID)
        {
            if (string.IsNullOrEmpty(advID))
            {
                Debug.LogError("-----CreateAdvInsert advID is null or empty");
                return;
            }

            AdvInterstitialData advData = new AdvInterstitialData
            {
                adInterstitial = TT.CreateInterstitialAd(new CreateInterstitialAdParam()
                {
                    InterstitialAdId = AdvInterstitialID,
                }),
                advEventInfo = new AdvEventInfo()
            };

            advData.adInterstitial.OnClose += () =>
            {
                Debug.Log($"-----Adv OnClose 插屏广告关闭");
                MMPOceanEngine.OceanPostback(MMPOceanEngine.game_addiction_event);
                m_TargetAdvEventInfo.closeCallback?.Invoke();
                m_TargetAdvEventInfo = null;
            };
            advData.adInterstitial.OnError += (errorCode, msg) =>
            {
                Debug.LogError($"-----Adv OnError 插屏广告出错 errorCode:{errorCode},msg:{msg}");
                /*
                2001    触发频率限制 小程序启动一定时间内不允许展示插屏广告
                2002    触发频率限制  距离小程序插屏广告或者激励视频广告上次播放时间间隔不足，不允许展示插屏广告
                2003    触发频率限制  当前正在播放激励视频广告或者插屏广告，不允许再次展示插屏广告
                2004    广告渲染失败  该项错误不是开发者的异常情况，或因小程序页面切换导致广告渲染失败
                2005    广告调用异常  插屏广告实例不允许跨页面调用
                 */
                m_TargetAdvEventInfo?.errorCallback?.Invoke(errorCode, msg);
                m_TargetAdvEventInfo = null;
            };
            advData.adInterstitial.OnLoad += () =>
            {
                Debug.Log($"-----Adv OnError 插屏广告加载");
                m_TargetAdvEventInfo?.loadCallback?.Invoke();
            };
            m_DicAdvInsertData.Add(advID, advData);
        }

        public void ShowAdvBanner(Action closeCallback = null, Action<int, string> loadFailCallback = null, Action loadCallback = null, string advID = "")
        {
            Debug.LogError("---- ShowAdvBanner A");
#if UNITY_EDITOR
            closeCallback?.Invoke();
            return;
#endif
            m_TargetAdvEventInfo = new AdvEventInfo
            {
                closeCallback = closeCallback,
                errorCallback = loadFailCallback,
                loadCallback = loadCallback
            };
            Debug.LogError("---- ShowAdvBanner B");

            if (!m_DicAdvBannerData.ContainsKey(AdvBannerID))
            {
                Debug.LogError("---- ShowAdvBanner CreateAdvBanner A");
                CreateAdvBanner(AdvBannerID);
                Debug.LogError("---- ShowAdvBanner CreateAdvBanner B");
            }
            else
            {
                HideAdvBanner();
            }
            Debug.LogError("---- ShowAdvBanner C");

            m_DicAdvBannerData[AdvBannerID]?.adBanner.Show();
            Debug.LogError("---- ShowAdvBanner D");

        }

        public void HideAdvBanner()
        {
#if UNITY_EDITOR
            return;
#endif
            Debug.LogError("---- HideAdvBanner A");

            m_DicAdvBannerData[AdvBannerID]?.adBanner.Hide();
            Debug.LogError("---- HideAdvBanner B");

        }

        public void ShowAdvVideo(Action<bool, int> closeCallback, Action loadCallback = null,
            Action<int, string> loadFailCallback = null, string advID = "")
        {
#if UNITY_EDITOR
            closeCallback?.Invoke(true, 1);
            return;
#endif

            m_TargetAdvEventInfo = new AdvEventInfo
            {
                closeVideoCallback = closeCallback,
                errorCallback = loadFailCallback,
                loadCallback = loadCallback
            };

            if (!m_DicAdvVideoData.ContainsKey(AdvVideoID))
            {
                CreateAdvVideo(advID);
            }
            m_DicAdvVideoData[AdvVideoID]?.adVideo.Show();
        }

        public void ShowAdvInsert(Action closeCallback = null, Action<int, string> loadFailCallback = null, Action loadCallback = null, string advID = "")
        {
#if UNITY_EDITOR
            closeCallback?.Invoke();
            return;
#endif
            m_TargetAdvEventInfo = new AdvEventInfo
            {
                closeCallback = closeCallback,
                errorCallback = loadFailCallback,
                loadCallback = loadCallback
            };

            if (!m_DicAdvInsertData.ContainsKey(AdvInterstitialID))
            {
                CreateAdvVideo(advID);
            }
            m_DicAdvInsertData[AdvInterstitialID]?.adInterstitial.Show();
        }
    }
}
