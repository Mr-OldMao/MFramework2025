using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MFramework.Runtime
{
    public class EventManager : GameModuleBase
    {
        public override int Priority => 10; // 高优先级

        private readonly Dictionary<int, List<Action>> m_DicEventHandlers = new();
        private readonly Dictionary<int, List<Delegate>> m_DicEventTHandlers = new Dictionary<int, List<Delegate>>();

        private readonly int m_MaxEventCountHint = 2;
        protected override async Task OnInitialize()
        {
            Logger.Log("事件系统初始化中...", LogType.FrameNormal);
            // 初始化事件系统
            await Task.Delay(100); // 模拟初始化耗时
            Logger.Log("事件系统初始化完成...", LogType.FrameNormal);
        }

        protected override void OnShutdown()
        {
            Logger.Log("事件系统已关闭...", LogType.FrameNormal);
            m_DicEventHandlers.Clear();
            m_DicEventTHandlers.Clear();
        }

        public void RegisterEvent(int eventId, Action callback)
        {
            if (!m_DicEventHandlers.ContainsKey(eventId))
            {
                m_DicEventHandlers[eventId] = new List<Action>();
            }
            m_DicEventHandlers[eventId].Add(callback);
            if (m_DicEventHandlers[eventId].Count > m_MaxEventCountHint)
            {
                Logger.LogError($"事件eventId：{eventId}注册次数过多，请检查是否存在重复注册 count：{m_DicEventHandlers[eventId].Count}");
            }
        }

        public void UnRegisterEvent(int eventId, Action callback)
        {
            if (m_DicEventHandlers.ContainsKey(eventId))
            {
                m_DicEventHandlers[eventId].Remove(callback);
                if (m_DicEventHandlers[eventId].Count == 0)
                {
                    m_DicEventHandlers.Remove(eventId);
                }
            }
        }

        public void DispatchEvent(int eventId)
        {
            if (m_DicEventHandlers.ContainsKey(eventId))
            {
                foreach (var handler in m_DicEventHandlers[eventId])
                {
                    try
                    {
                        handler?.Invoke();
                    }
                    catch (Exception e)
                    {
                        Logger.LogError($"事件处理异常 [{eventId}]: {e}", LogType.FrameCore);
                    }
                }
            }
        }

        public void RegisterEvent<T>(int eventId, Action<T> callback)
        {
            if (!m_DicEventTHandlers.ContainsKey(eventId))
            {
                m_DicEventTHandlers[eventId] = new List<Delegate>();
            }
            m_DicEventTHandlers[eventId].Add(callback);//TODO 这里有装箱操作，后续需要优化
            if (m_DicEventTHandlers[eventId].Count > m_MaxEventCountHint)
            {
                Logger.LogError($"事件eventId：{eventId}注册次数过多，请检查是否存在重复注册 m_MaxEventCount：{m_MaxEventCountHint}");
            }
        }

        public void UnRegisterEvent<T>(int eventId, Action<T> callback)
        {
            if (m_DicEventTHandlers.ContainsKey(eventId))
            {
                m_DicEventTHandlers[eventId].Remove(callback);
                if (m_DicEventTHandlers[eventId].Count == 0)
                {
                    m_DicEventTHandlers.Remove(eventId);
                }
            }
        }

        public void DispatchEvent<T>(int eventId, T eventData)
        {
            if (m_DicEventTHandlers.ContainsKey(eventId))
            {
                foreach (var handler in m_DicEventTHandlers[eventId])
                {
                    try
                    {
                        (handler as Action<T>)?.Invoke(eventData);//TODO 这里有拆箱操作，后续需要优化
                    }
                    catch (Exception e)
                    {
                        Logger.LogError($"事件处理异常 [{eventId}]: {e}", LogType.FrameCore);
                    }
                }
            }
        }

        public void UnRegisterEvent(int eventId)
        {
            m_DicEventTHandlers.Remove(eventId);
            m_DicEventHandlers.Remove(eventId);
        }

        ///// <summary>
        ///// 自动绑定MonoBehaviour销毁时自动解绑
        ///// </summary>
        ///// <param name="eventId"></param>
        ///// <param name="callback"></param>
        ///// <param name="mono"></param>
        ///// <param name="atuoBindUnregister"></param>
        //public void RegisterEvent(int eventId, Action callback, MonoBehaviour mono)
        //{
        //    RegisterEvent(eventId, callback);
        //    mono.StartCoroutine(WaitForDestroy(mono, () =>
        //    {
        //        Logger.Log($"事件自动解绑成功，mono：{mono}，eventId：{eventId}");
        //        UnRegisterEvent(eventId, callback);
        //    }));
        //}

        ///// <summary>
        ///// 自动绑定MonoBehaviour销毁时自动解绑
        ///// </summary>
        ///// <param name="eventId"></param>
        ///// <param name="callback"></param>
        ///// <param name="mono"></param>
        ///// <param name="atuoBindUnregister"></param>
        //public void RegisterEvent<T>(int eventId, Action<T> callback, MonoBehaviour mono)
        //{
        //    RegisterEvent<T>(eventId, callback);
        //    mono.StartCoroutine(WaitForDestroy(mono, () => UnRegisterEvent<T>(eventId, callback)));
        //}

        //private static IEnumerator WaitForDestroy(MonoBehaviour behaviour, Action onDestroy)
        //{
        //    yield return new WaitUntil(() => behaviour == null);
        //    onDestroy?.Invoke();
        //}

    } 
}
