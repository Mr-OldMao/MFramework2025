using System;
using UnityEngine;

namespace MiniGameSDK
{
#if SDK_KS
    public class KuaishouSDK : IGameSDK, IKuaishouSpecial
    {
        public void InitSDK(Action<bool> callback = null) { }
        public void Login(Action<bool> callback) { }
        public string GetPlatformName() => "Kuaishou";
        public bool IsLoggedIn { get; private set; } = false;

        public void GetUserInfo(Action<UserInfo> onSuccess, Action<string> onFail) => onSuccess?.Invoke(new UserInfo());
        public void ShowAdvReward(Action<bool> onClose, Action<string> onError = null) => onClose?.Invoke(true);
        public void ShowAdvInsert(Action onClose = null, Action<string> onError = null) => onClose?.Invoke();
        public void ShowAdvBanner(int position = 0, Action onClose = null, Action<string> onError = null) { }
        public void HideAdvBanner() { }

        public void ShowTaskCenter() { }
    }
#endif
}