using System;
using UnityEngine;

namespace MiniGameSDK
{
    public class EditorSDK : IGameSDK
    {
        public bool IsCanUseAdv { get; set; } = true;

        public void InitSDK(Action<bool> callback = null, bool isAutoInitAdv = true) { }

        public void Login(Action<bool> callback) { }

        public string GetPlatformName() => "Editor";


        public void GetUserInfo(Action<UserInfo> onSuccess, Action<string> onFail)
            => onSuccess?.Invoke(new UserInfo { nickName = "测试玩家" });

        public bool IsLoggedIn { get; private set; }

        public void ShowAdvReward(Action<bool> onClose, Action<string> onError = null) => onClose?.Invoke(true);
        public void ShowAdvInsert(Action onClose = null, Action<string> onError = null) => onClose?.Invoke();
        public void ShowAdvBanner(int position = 0, Action onClose = null, Action<string> onError = null) { }
        public void HideAdvBanner() { }

        public void ShotToast(string title, string icon = "", Action completeCallback = null, int durationMS = 1500)
        {

        }
    }
}