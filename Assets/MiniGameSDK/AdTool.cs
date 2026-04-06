using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiniGameSDK
{
    /// <summary>
    /// 广告工具：防重复点击、自动重试、冷却
    /// </summary>
    public static class AdTool
    {
        private static float _lastRewardTime;
        private const float COOLDOWN = 1.2f;

        public static bool CanShowReward()
        {
            return Time.realtimeSinceStartup - _lastRewardTime > COOLDOWN;
        }

        public static void ShowAdvReward(Action<bool> onClose, Action<string> onError)
        {
            if (!CanShowReward()) return;
            _lastRewardTime = Time.realtimeSinceStartup;

            SDKManager.Instance.ShowAdvReward(onClose, err =>
            {
                // 简单自动重试一次
                Debug.LogWarning($"广告失败：{err}，1秒后重试");
                Scheduler.Instance.Delay(1f, () =>
                {
                    SDKManager.Instance.ShowAdvReward(onClose, onError);
                });
            });
        }
    }

    // 简易延迟调用
    public class Scheduler
    {
        public static readonly Scheduler Instance = new Scheduler();
        private readonly GameObject _go;

        private Scheduler()
        {
            _go = new GameObject("Scheduler");
            Object.DontDestroyOnLoad(_go);
        }

        public void Delay(float delay, Action cb)
        {
            _go.AddComponent<DelayRunner>().Start(delay, cb);
        }

        private class DelayRunner : MonoBehaviour
        {
            private Action _cb;

            public async void Start(float delay, Action cb)
            {
                _cb = cb;
                await System.Threading.Tasks.Task.Delay((int)(delay * 1000));
                _cb?.Invoke();
                Destroy(this);
            }
        }
    }
}