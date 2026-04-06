using System;
using UnityEngine;
namespace MiniGameSDK
{
#if SDK_DY

    using TTSDK;
    using TTSDK.UNBridgeLib.LitJson;
    using static TTSDK.TTRank;
    public class DouyinSDK : IGameSDK, IDouyinSpecial
    {
        private TTRewardedVideoAd _rewardAd;
        private TTInterstitialAd _interAd;
        private TTBannerAd _bannerAd;

        private const string AD_REWARD = "1f49fefc9hhgefggib";
        private const string AD_INTER = "eaf3j2069d6l3j1cek";
        private const string AD_BANNER = "11r92jkkidfcd1dfe7";

        // 激励广告回调
        private Action<bool> _onRewardClose;
        private Action<string> _onRewardError;
        private bool _isRewardLoaded;

        // 插屏广告回调
        private Action _onInterClose;
        private Action<string> _onInterError;
        private bool _isInterLoaded;

        // Banner 广告回调
        private Action _onBannerClose;
        private Action<string> _onBannerError;

        public void InitSDK(Action<bool> callback = null)
        {
            TT.InitSDK((int code, ContainerEnv env) =>
            {
                Debug.LogError($"[DouyinSDK] InitSDK: {code} {env}");
                bool isInitSucc = code == 0;
                if (isInitSucc)
                {
                    InitBannerAd();
                    InitInterAd();
                    InitRewardAd();
                }
                callback?.Invoke(isInitSucc);
            });
        }

        public string GetPlatformName() => "Douyin";

        #region 登录
        public bool IsLoggedIn { get; private set; } = false;

        public void Login(Action<bool> callback)
        {
            TT.Login((string code, string anonymousCode, bool isLogin) =>
            {
                Debug.Log($"[DouyinSDK] Login: {code} {anonymousCode} {isLogin}");
                IsLoggedIn = true;
                callback?.Invoke(true);
            }, (string errMsg) =>
            {
                Debug.LogError($"[DouyinSDK] Login Error: {errMsg}");
                IsLoggedIn = false;
                callback?.Invoke(false);
            }, true);
        }

        public void GetUserInfo(Action<UserInfo> onSuccess, Action<string> onFail)
        {
            TT.GetUserInfoAuth((bool auth) =>
            {
                if (!auth)
                {
                    onFail?.Invoke("授权失败");
                    return;
                }

                TT.GetUserInfo((ref TTUserInfo scUserInfo) =>
                {
                    if (scUserInfo != null)
                    {
                        var user = new UserInfo
                        {
                            nickName = scUserInfo.nickName,
                            avatarUrl = scUserInfo.avatarUrl,
                            openId = scUserInfo.cloudId
                        };
                        onSuccess?.Invoke(user);
                    }
                    else
                    {
                        Debug.LogError($"[DouyinSDK] GetUserInfo Error: scUserInfo is null");
                    }
                }, (string errMsg) =>
                {
                    onFail?.Invoke(errMsg);
                });
            }, (string errMsg) =>
            {
                onFail?.Invoke(errMsg);
            });
        }
        #endregion

        #region 激励广告
        private void InitRewardAd()
        {
            if (_rewardAd != null)
            {
                return;
            }

            _rewardAd = TT.CreateRewardedVideoAd(AD_REWARD);
            _isRewardLoaded = false;

            _rewardAd.OnLoad += () =>
            {
                Debug.LogError($"[DouyinSDK] RewardAd OnLoad ");
                _isRewardLoaded = true;
            };

            _rewardAd.OnError += (int code, string message) =>
            {
                Debug.LogError($"[DouyinSDK] RewardAd Error: {code} {message}");
                _rewardAd = null;
                _isRewardLoaded = false;
                _onRewardError?.Invoke($"[DouyinSDK] RewardAd Error: {code} {message}");
            };
            _rewardAd.OnClose += (bool isEnded, int count) =>
            {
                Debug.Log($"[DouyinSDK] RewardAd Close: {isEnded} {count}");

                _onRewardClose?.Invoke(isEnded);
                ClearRewardCallback();
                // 关闭后重建
                _rewardAd = null;
                _isRewardLoaded = false;
                InitRewardAd();
            };
            _rewardAd.Load();
        }

        public void ShowAdvReward(Action<bool> onClose, Action<string> onError = null)
        {
            _onRewardClose = onClose;
            _onRewardError = onError;

            if (_rewardAd == null)
            {
                InitRewardAd();
                Debug.LogError("[DouyinSDK] ShowAdvReward 初始化中");
                return;
            }
            if (!_isRewardLoaded)
            {
                Debug.LogError("[DouyinSDK] ShowAdvReward 广告加载中");
                return;
            }

            try
            {
                _rewardAd.Show();
            }
            catch (Exception e)
            {
                Debug.LogError($"[DouyinSDK] ShowAdvReward 激励广告异常,{e.Message}");
            }
        }

        private void ClearRewardCallback()
        {
            _onRewardClose = null;
            _onRewardError = null;
        }
        #endregion

        #region 插屏广告
        private void InitInterAd()
        {
            if (_interAd != null)
            {
                return;
            }

            CreateInterstitialAdParam param = new CreateInterstitialAdParam();
            param.InterstitialAdId = AD_INTER;

            _interAd = TT.CreateInterstitialAd(param);
            _isInterLoaded = false;

            _interAd.OnLoad += () =>
            {
                _isInterLoaded = true;
                Debug.Log($"[DouyinSDK] InterAd OnLoad");
            };

            _interAd.OnError += (int code, string message) =>
            {
                Debug.LogError($"[DouyinSDK] InterAd Error: {code} {message}");
                /*
               2001    触发频率限制 小程序启动一定时间内不允许展示插屏广告
               2002    触发频率限制  距离小程序插屏广告或者激励视频广告上次播放时间间隔不足，不允许展示插屏广告
               2003    触发频率限制  当前正在播放激励视频广告或者插屏广告，不允许再次展示插屏广告
               2004    广告渲染失败  该项错误不是开发者的异常情况，或因小程序页面切换导致广告渲染失败
               2005    广告调用异常  插屏广告实例不允许跨页面调用
                */
                _onInterError?.Invoke($"[DouyinSDK] InterAd Error: {code} {message}");
                _interAd.Destroy();
                _interAd = null;
                _isInterLoaded = false;
                InitInterAd(); //自动重新加载
            };

            _interAd.OnClose += () =>
            {
                Debug.Log($"[DouyinSDK] InterAd Close");

                _onInterClose?.Invoke();
                ClearInterCallback();
                _interAd.Destroy();
                _interAd = null;
                _isInterLoaded = false;
                InitInterAd(); // 关闭后自动重新加载
            };

            _interAd.Load();
        }

        public void ShowAdvInsert(Action onClose = null, Action<string> onError = null)
        {
            _onInterClose = onClose;
            _onInterError = onError;

            if (_interAd == null)
            {
                InitInterAd();
                Debug.LogError("[DouyinSDK] ShowAdvInsert 初始化中");
                return;
            }
            if (!_isInterLoaded)
            {
                Debug.LogError("[DouyinSDK] ShowAdvInsert 插屏加载中");
                return;
            }

            try
            {
                _interAd.Show();
            }
            catch (Exception e)
            {
                Debug.LogError($"[DouyinSDK] ShowAdvInsert 插屏广告异常,{e.Message}");
            }
        }

        private void ClearInterCallback()
        {
            _onInterClose = null;
            _onInterError = null;
        }
        #endregion

        #region Banner 广告
        private void InitBannerAd()
        {
            if (_bannerAd != null)
            {
                return;
            }

            TTBannerStyle style = new TTBannerStyle();
            style.width = 300;
            style.left = (Screen.width - style.width) / 2;
            //style.top = position == 0 ? Screen.height - 120 : 20;
            style.top = Screen.height - 120;

            CreateBannerAdParam param = new CreateBannerAdParam();
            param.BannerAdId = AD_BANNER;
            param.Style = style;
            param.AdIntervals = 60;

            _bannerAd = TT.CreateBannerAd(param);
            _bannerAd.OnLoad += () =>
            {
                Debug.LogError($"[DouyinSDK] BannerAd OnLoad");
            };
            _bannerAd.OnError += (int code, string message) =>
            {
                _onBannerError?.Invoke($"[DouyinSDK] BannerAd Error: {code} {message}");
                Debug.LogError($"[DouyinSDK] BannerAd Error: {code} {message}");
                //HideAdvBanner();
            };
            _bannerAd.OnClose += () =>
            {
                Debug.Log($"[DouyinSDK] BannerAd Close");
                _onBannerClose?.Invoke();
                //HideAdvBanner();
            };

        }

        public void ShowAdvBanner(int position = 0, Action onClose = null, Action<string> onError = null)
        {
            _onBannerClose = onClose;
            _onBannerError = onError;

            InitBannerAd();

            try
            {
                _bannerAd.Show();
            }
            catch (Exception e)
            {
                Debug.LogError($"[DouyinSDK] ShowAdvBanner Banner广告异常,{e.Message}");
            }
        }

        public void HideAdvBanner()
        {
            if (_bannerAd != null)
            {
                _bannerAd.Hide();
                //_bannerAd.Destroy();
                //_bannerAd = null;
            }

            _onBannerClose?.Invoke();
            _onBannerClose = null;
            _onBannerError = null;
        }
        #endregion

        #region 特有接口

        public void ShowSideBar(Action<bool> callback)
        {
            var jd = new JsonData
            {
                ["scene"] = "sidebar",
                //["activityId"] = "cacheActivityId",
            };
            TT.NavigateToScene(jd,
             () =>
             {
                 Debug.Log($"[DouyinSDK] NavigateToScene Success");
                 callback?.Invoke(true);
             }, () =>
             {
                 Debug.Log($"[DouyinSDK] NavigateToScene Completed");
             }, (p1, p2) =>
             {
                 Debug.LogError($"[DouyinSDK] NavigateToScene Fail p1:{p1},p2:{p2}");
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
                Debug.Log($"[DouyinSDK] GetImRankList ,jd:{jd},{b},{s} ");
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
                Debug.Log($"[DouyinSDK] SetImRankData,jd:{jd}, {b},{s} ");
            });
        }

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
                Debug.LogError($"[DouyinSDK] GetImRankData succ");
                callback?.Invoke(rankData);
            }, (msg) =>
            {
                Debug.LogError($"[DouyinSDK] GetImRankData Fail msg:{msg} ");
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
                Debug.LogError($"[DouyinSDK] GetImRankData succ");
                int vaule = 0;
                if (!string.IsNullOrEmpty(rankData?.SelfItem?.Item?.Value))
                {
                    vaule = int.Parse(rankData.SelfItem.Item.Value);
                }
                callback?.Invoke(vaule);
            }, (msg) =>
            {
                Debug.LogError($"[DouyinSDK] GetImRankData Fail msg:{msg} ");
            });
        }

        public void ShowRevisitGuide(Action<bool> callback = null)
        {
            TT.ShowRevisitGuide((b) =>
            {
                Debug.Log($"[DouyinSDK] ShowRevisitGuide callback b:{b}");
                callback?.Invoke(b);
            });
        }

        public void CheckScene()
        {
            TT.CheckScene(TTSideBar.SceneEnum.SideBar,
            (b) =>
            {
                Debug.Log($"[DouyinSDK] CheckScene succ b:{b}");
            }, () =>
            {
                Debug.Log($"[DouyinSDK] CheckScene Completed");
            }, (p1, p2) =>
            {
                Debug.LogError($"[DouyinSDK] CheckScene Fail p1:{p1},p2:{p2}");
            });
        }

        public void OnInitCompletedCallback()
        {
            Login(null);
            CheckScene();
        }

        #endregion

        //#region 其他功能
        //public void Share(string title, string imgUrl, Action onSuccess, Action onFail)
        //{
        //    //https://developer.open-douyin.com/docs/resource/zh-CN/mini-game/develop/api/javascript-api/open-capacity/retweet/share-param
        //    JsonData jd = new JsonData
        //    {
        //        ["title"] = title,
        //    };
        //    //https://developer.open-douyin.com/docs/resource/zh-CN/mini-game/develop/api/c-api/open-ability/game-share/share-module
        //    TT.ShareAppMessage(jd, (Dictionary<string, object> data) =>
        //    {
        //        Debug.Log($"ShareAppMessage Succ: {data}");
        //        onSuccess?.Invoke();
        //    }, (string errMsg) =>
        //    {
        //        Debug.LogError($"ShareAppMessage Fail: {errMsg}");
        //        onFail?.Invoke(errMsg);
        //    }, () =>
        //    {
        //        Debug.Log($"ShareAppMessage Cancel");
        //    });
        //}

        //public void NavigateToMiniGame(string appId, string path)
        //    => TT.NavigateToMiniProgram(null);

        //public void UploadScore(int rankId, int score, Action onSuccess, Action<string> onFail)
        //    => TT.SetRank(rankId.ToString(), score, res =>
        //    {
        //        if (res.IsSuccess) onSuccess?.Invoke();
        //        else onFail?.Invoke(res.ErrMsg);
        //    });

        //public void GetRankList(int rankId, Action<RankData[]> onSuccess, Action<string> onFail)
        //    => onSuccess?.Invoke(Array.Empty<RankData>());

        //public void SetStorage(string key, string value, Action onSuccess, Action<string> onFail)
        //    => TT.SetStorage(key, value, res =>
        //    {
        //        if (res.IsSuccess) onSuccess?.Invoke();
        //        else onFail?.Invoke(res.ErrMsg);
        //    });

        //public void GetStorage(string key, Action<string> onSuccess, Action<string> onFail)
        //    => TT.GetStorage(key, res =>
        //    {
        //        if (res.IsSuccess) onSuccess?.Invoke(res.Data);
        //        else onFail?.Invoke(res.ErrMsg);
        //    });

        //public void Vibrate(bool isLong = false)
        //{
        //    if (isLong) TT.VibrateLong();
        //    else TT.VibrateShort();
        //}

        //public void ShowToast(string msg) => TT.ShowToast(new TTToastParam { Content = msg });
        //public void QuitGame() => TT.ExitGame();
        //public void OnShow() { }
        //public void OnHide() { }
        //public void ShowSideBar() => TT.ShowSideBar();
        //#endregion
    }
#endif
}