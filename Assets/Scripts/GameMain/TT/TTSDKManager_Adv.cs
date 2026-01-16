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
            AdvBannerData advData = new AdvBannerData
            {
                adBanner = TT.CreateBannerAd(new CreateBannerAdParam()
                {
                    BannerAdId = AdvBannerID,
                    Style = new TTBannerStyle
                    {
                        top = 0,
                        left = 0,
                        width = 320
                    },
                    AdIntervals = 30
                }),
            };
            advData.adBanner.OnClose += () =>
            {
                Debug.Log($"-----Adv OnClose Banner广告关闭");
                m_TargetAdvEventInfo?.closeCallback?.Invoke();
                m_TargetAdvEventInfo = null;
            };
            advData.adBanner.OnError += (errorCode, msg) =>
            {
                Debug.LogError($"-----Adv OnError Banner广告出错 errorCode:{errorCode},msg:{msg}");
                m_TargetAdvEventInfo?.errorCallback?.Invoke(errorCode, msg);
                m_TargetAdvEventInfo = null;
            };
            advData.adBanner.OnLoad += () =>
            {
                Debug.Log($"-----Adv OnError Banner广告加载");
                advData.advEventInfo?.loadCallback?.Invoke();
            };
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
                Debug.Log($"-----Adv OnClose 视频广告关闭");
                m_TargetAdvEventInfo?.closeVideoCallback?.Invoke(isEnded, count);
                m_TargetAdvEventInfo = null;
            };
            advData.adVideo.OnError += (errorCode, msg) =>
            {
                Debug.LogError($"-----Adv OnError 视频广告出错 errorCode:{errorCode},msg:{msg}");
                m_TargetAdvEventInfo?.errorCallback?.Invoke(errorCode, msg);
                m_TargetAdvEventInfo = null;
            };
            advData.adVideo.OnLoad += () =>
            {
                Debug.Log($"-----Adv OnError 视频广告加载");
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
            m_TargetAdvEventInfo = new AdvEventInfo
            {
                closeCallback = closeCallback,
                errorCallback = loadFailCallback,
                loadCallback = loadCallback
            };

            if (!m_DicAdvBannerData.ContainsKey(AdvBannerID))
            {
                CreateAdvBanner(advID);
            }
            m_DicAdvBannerData[AdvBannerID]?.adBanner.Show();
        }


        public void ShowAdvVideo(Action<bool, int> closeCallback, Action loadCallback = null,
            Action<int, string> loadFailCallback = null, string advID = "")
        {
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

        public void ShowAdvInsert(Action closeCallback = null, Action<int, string> loadFailCallback = null, Action loadCallback = null,string advID = "")
        {
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
