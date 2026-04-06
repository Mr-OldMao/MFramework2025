using System;

namespace MiniGameSDK
{
    public interface IGameSDK
    {
        // 初始化相关方法
        void InitSDK(Action<bool> callback = null, bool isAutoInitAdv = true); // 初始化SDK
        string GetPlatformName(); // 获取平台名称

        // 登录相关方法
        bool IsLoggedIn { get; } // 检查用户是否已登录
        void Login(Action<bool> callback); // 用户登录
        void GetUserInfo(Action<UserInfo> onSuccess, Action<string> onFail); // 获取用户信息
        // 广告相关方法
        bool IsCanUseAdv { get; set; } // 是否可以显示广告
        void ShowAdvReward(Action<bool> onClose, Action<string> onError = null); // 显示激励广告
        void ShowAdvInsert(Action onClose = null, Action<string> onError = null); // 显示插屏广告

        // 横幅广告相关方法
        void ShowAdvBanner(int position = 0, Action onClose = null, Action<string> onError = null); // 显示横幅广告
        void HideAdvBanner(); // 隐藏横幅广告

        //Toast
        void ShotToast(string title, string icon = "", Action completeCallback = null, int durationMS = 1500);

        //// 分享相关方法
        //void Share(string title, string imgUrl, Action onSuccess, Action onFail); // 分享功能

        //// 小游戏相关方法
        //void NavigateToMiniGame(string appId, string path); // 跳转到小游戏

        //// 排行榜相关方法
        //void UploadScore(int rankId, int score, Action onSuccess, Action<string> onFail); // 上传分数
        //void GetRankList(int rankId, Action<RankData[]> onSuccess, Action<string> onFail); // 获取排行榜数据

        //// 存储相关方法
        //void SetStorage(string key, string value, Action onSuccess, Action<string> onFail); // 设置存储数据
        //void GetStorage(string key, Action<string> onSuccess, Action<string> onFail); // 获取存储数据

        //// 系统功能相关方法
        //void Vibrate(bool isLong = false); // 震动功能
        //void ShowToast(string msg); // 显示提示信息
        //void QuitGame(); // 退出游戏

        //// 生命周期回调方法
        //void OnShow(); // 游戏显示时的回调
        //void OnHide(); // 游戏隐藏时的回调
    }
}
