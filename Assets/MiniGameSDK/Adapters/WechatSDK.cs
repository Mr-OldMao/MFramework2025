using System;
using UnityEngine;
#if SDK_WX
using WeChatWASM;
#endif

namespace MiniGameSDK
{
#if SDK_WX
    public class WechatSDK : IGameSDK, IWechatSpecial
    {
        private const string ADV_REWARD = "1f49fefc9hhgefggib";
        private const string ADV_INTER = "eaf3j2069d6l3j1cek";
        private const string ADV_BANNER = "11r92jkkidfcd1dfe7";

        private WXRewardedVideoAd m_AdvReward;
        private WXInterstitialAd m_AdvInsert;
        private WXBannerAd m_AdvBanner;

        private AdvState m_AdvRewardState = AdvState.Unload;
        private AdvState m_AdvInterState = AdvState.Unload;
        private AdvState m_AdvBannerState = AdvState.Unload;

        public enum AdvState
        {
            Unload,
            Loading,
            LoadedSuc,
            LoadedFail,
        }

        public void InitSDK(Action<bool> callback = null)
        {
            WX.InitSDK((num) =>
            {
                InitADReward();
                InitADInsert();
                InitADBanner();
                callback?.Invoke(true);
            });
        }
        public bool IsLoggedIn { get; private set; } = false;

        public void Login(Action<bool> callback)
        {
            LoginOption p = new LoginOption
            {
                success = (p) =>
                {
                    Debug.Log($"[WX] 微信登录成功回调");
                    IsLoggedIn = true;
                    callback?.Invoke(true);
                },
                fail = (p) =>
                {
                    Debug.Log($"[WX] 微信登录失败回调 {p.errno},{p.errMsg}");
                    IsLoggedIn = false;
                    callback?.Invoke(false);
                },
                complete = (p) =>
                {
                    Debug.Log($"[WX] 微信登录完成回调");
                }
            };
            WX.Login(p);
        }

        public string GetPlatformName() => "Wechat";


        public void GetUserInfo(Action<UserInfo> onSuccess, Action<string> onFail)
        {
            new GetUserInfoOption()
            {
                success = (p) =>
                {
                    onSuccess.Invoke(new UserInfo
                    {
                        userData = p.userInfo
                    });
                },
                 fail = (p) =>
                 {
                     onFail?.Invoke($"[WX] 获取用户数据失败 errMsg:{p.errMsg}");
                 }
            };
        }



        #region Adv Init
        private void InitADReward(Action<bool> loadCallback = null)
        {
            if (m_AdvRewardState == AdvState.Unload || m_AdvRewardState == AdvState.LoadedFail)
            {
                m_AdvRewardState = AdvState.Loading;
                m_AdvReward = WX.CreateRewardedVideoAd(new WXCreateRewardedVideoAdParam
                {
                    adUnitId = ADV_REWARD,
                    multiton = false
                });
            }
            m_AdvReward.Load((_) =>
            {
                m_AdvRewardState = AdvState.LoadedSuc;
                loadCallback?.Invoke(true);
            }, (p) =>
            {
                m_AdvRewardState = AdvState.LoadedFail;
                loadCallback?.Invoke(false);
                Debug.LogError($"[WX] 激励广告加载失败 ,errCode:{p.errCode},errMsg:{p.errMsg}");
            });
        }

        private void InitADInsert(Action<bool> loadCallback = null)
        {
            if (m_AdvInterState == AdvState.Unload || m_AdvInterState == AdvState.LoadedFail)
            {
                m_AdvInterState = AdvState.Loading;
                m_AdvInsert = WX.CreateInterstitialAd(new WXCreateInterstitialAdParam
                {
                    adUnitId = ADV_INTER,
                });
            }
            m_AdvInsert.Load((_) =>
            {
                m_AdvInterState = AdvState.LoadedSuc;
                loadCallback?.Invoke(true);
            }, (p) =>
            {
                m_AdvInterState = AdvState.LoadedFail;
                loadCallback?.Invoke(false);
                Debug.LogError($"[WX] 插屏广告加载失败 ,errCode:{p.errCode},errMsg:{p.errMsg}");
            });
        }

        private void InitADBanner(Action<bool> loadCallback = null)
        {
            if (m_AdvBannerState == AdvState.Unload || m_AdvBannerState == AdvState.LoadedFail)
            {
                m_AdvBannerState = AdvState.Loading;
                m_AdvBanner = WX.CreateBannerAd(new WXCreateBannerAdParam()
                {
                    adUnitId = ADV_BANNER,
                    adIntervals = 30,
                    style = new Style()
                    {
                        left = 0,
                        top = 0,
                        width = 600,
                        height = 200
                    }
                });
            }
            m_AdvBanner.OnLoad((p) =>
            {
                m_AdvBannerState = AdvState.LoadedSuc;
                loadCallback?.Invoke(true);
            });
        }
        #endregion

        #region Adv Show

        public void ShowAdvReward(Action<bool> onClose, Action<string> onError = null)
        {
            switch (m_AdvRewardState)
            {
                case AdvState.Unload:
                case AdvState.Loading:
                case AdvState.LoadedFail:
                    InitADReward(isLoadedSuc =>
                    {
                        if (isLoadedSuc)
                        {
                            m_AdvReward.onCloseAction = p =>
                            {
                                onClose?.Invoke(p.isEnded);
                            };
                            m_AdvReward.Show();
                        }
                        else
                        {
                            onError?.Invoke("[WX] adv rewardloaded fail");
                        }
                    });
                    break;
                case AdvState.LoadedSuc:
                    m_AdvReward.onCloseAction = p =>
                    {
                        onClose?.Invoke(p.isEnded);
                    };
                    m_AdvReward.Show();
                    break;
            }
        }

        public void ShowAdvInsert(Action onClose = null, Action<string> onError = null)
        {
            switch (m_AdvInterState)
            {
                case AdvState.Unload:
                case AdvState.Loading:
                case AdvState.LoadedFail:
                    InitADInsert(isLoadedSuc =>
                    {
                        if (isLoadedSuc)
                        {
                            m_AdvInsert.onCloseAction = () =>
                            {
                                onClose.Invoke();
                            };
                            m_AdvInsert.Show();
                        }
                        else
                        {
                            onError?.Invoke("[WX] adv insert loaded fail");
                        }
                    });
                    break;
                case AdvState.LoadedSuc:
                    m_AdvInsert.onCloseAction = () =>
                    {
                        onClose?.Invoke();
                    };
                    m_AdvInsert.Show();
                    break;
            }
        }

        public void ShowAdvBanner(int position = 0, Action onClose = null, Action<string> onError = null)
        {
            switch (m_AdvBannerState)
            {
                case AdvState.Unload:
                case AdvState.Loading:
                case AdvState.LoadedFail:
                    InitADBanner(isLoadedSuc =>
                    {
                        if (isLoadedSuc)
                        {
                            //注册关闭BannerAdv事件 onClose.Invoke();
                            m_AdvBanner.Show();
                        }
                        else
                        {
                            onError?.Invoke("[WX] adv banner loaded fail");
                        }
                    });
                    break;
                case AdvState.LoadedSuc:
                    //注册关闭BannerAdv事件 onClose.Invoke();
                    m_AdvBanner.Show();
                    break;
            }
        }


        public void HideAdvBanner()
        {
            m_AdvBanner?.Hide();

            m_AdvBanner.Destroy();
            m_AdvBanner = null;
            m_AdvBannerState = AdvState.Unload;
        }
        #endregion



        public void PrivacyAuthorizeResolve()
        {
            PrivacyAuthorizeResolveOption p = new PrivacyAuthorizeResolveOption
            {
                eventString = "agree"
            };
            WX.PrivacyAuthorizeResolve(p);
        }
    }
#endif
}