using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MFramework.Runtime
{
    public class TimerManager : ITimerManager
    {
        private readonly Dictionary<int, TimerInfo> m_DicCacheTimer = new Dictionary<int, TimerInfo>();

        private int m_CurTimerId = 0;

        public bool IsStartingUp { get ; set ; }

        public Task Init()
        {
            IsStartingUp = true;
            m_CurTimerId = 0;
            return Task.CompletedTask;
        }

        public class TimerInfo
        {
            public int timerId;
            public Action callback;
            public float targetDelaySeconds;
            public float curDelaySeconds;
            public DelayLoopInfo delayLoopInfo;
        }

        public class DelayLoopInfo
        {
            public int targetInvokeCount;
            public int curInvokeCount;
            public float targetLoopIntervalSeconds;
            public float curLoopIntervalSeconds;
            public Action loopEndCallback;
        }

        public int AddDelayTimer(float delaySeconds, Action callback)
        {
            TimerInfo timerInfo = new TimerInfo
            {
                timerId = ++m_CurTimerId,
                callback = callback,
                targetDelaySeconds = delaySeconds,
                curDelaySeconds = 0,
                delayLoopInfo = null
            };
            return AddDelayTimer(timerInfo);
        }

        public int AddDelayTimer(int delayMilliSeconds, Action callback)
        {
            TimerInfo timerInfo = new TimerInfo
            {
                timerId = ++m_CurTimerId,
                callback = callback,
                targetDelaySeconds = delayMilliSeconds / 1000f,
                curDelaySeconds = 0,
                delayLoopInfo = null
            };
            return AddDelayTimer(timerInfo);
        }

        /// <summary>
        /// 新增延时循环定时器
        /// </summary>
        /// <param name="delaySeconds">延时时间</param>
        /// <param name="loopSeconds">循环间隔时间</param>
        /// <param name="callback"></param>
        /// <param name="loopEndCallback"></param>
        /// <param name="targetLoopCount">-1表示无限循环</param>
        /// <returns></returns>
        public int AddDelayTimer(float delaySeconds, float loopSeconds, Action callback, Action loopEndCallback = null, int targetLoopCount = -1)
        {
            TimerInfo timerInfo = new TimerInfo
            {
                timerId = ++m_CurTimerId,
                callback = callback,
                targetDelaySeconds = delaySeconds,
                curDelaySeconds = 0,

                delayLoopInfo = new DelayLoopInfo
                {
                    targetInvokeCount = targetLoopCount,
                    curInvokeCount = 0,
                    targetLoopIntervalSeconds = loopSeconds,
                    curLoopIntervalSeconds = 0,
                    loopEndCallback = loopEndCallback
                }
            };
            return AddDelayTimer(timerInfo);
        }

        public void RemoveDelayTimer(int timerId)
        {
            var timerInfo = GetTimerInfo(timerId);
            if (timerInfo == null)
            {
                return;
            }
            timerInfo.callback = null;
        }


        public TimerInfo GetTimerInfo(int timerId)
        {
            if (!m_DicCacheTimer.ContainsKey(timerId))
            {
                Debugger.LogError($"TimerManager-GetTimerInfo , timerId:{timerId} is not exist", LogType.FrameCore);
                return default;
            }
            return m_DicCacheTimer[timerId];
        }

        public string ToString(int timerId)
        {
            var timerInfo = GetTimerInfo(timerId);
            if (timerInfo != null)
            {
                string info = string.Empty;
                if (timerInfo.delayLoopInfo == null)
                {
                    info = $"timerId:{timerInfo.timerId},curDelaySeconds:{timerInfo.curDelaySeconds},targetDelaySeconds:{timerInfo.targetDelaySeconds}";
                }
                else
                {
                    info = $"timerId:{timerInfo.timerId}" 
                        + $",curDelaySeconds:{timerInfo.curDelaySeconds},targetDelaySeconds:{timerInfo.targetDelaySeconds}"
                        + $",curLoopCount:{timerInfo.delayLoopInfo.curInvokeCount},targetLoopCount:{timerInfo.delayLoopInfo.targetInvokeCount}"
                        + $",curLoopIntervalSeconds:{timerInfo.delayLoopInfo.curLoopIntervalSeconds},targetLoopIntervalSeconds:{timerInfo.delayLoopInfo.targetLoopIntervalSeconds}";
                }
                return info;
            }
            return default;
        }


        public void OnUpdate(float deltaTime)
        {
            if (!IsStartingUp || m_DicCacheTimer == null || m_DicCacheTimer.Count == 0)
            {
                return;
            }
            for (int i = 0; i < m_DicCacheTimer.Count;)
            {
                var timerInfo = m_DicCacheTimer.ElementAt(i).Value;

                if (timerInfo.callback == null)
                {
                    m_DicCacheTimer.Remove(m_DicCacheTimer.ElementAt(i).Key);
                    continue;
                }

                if (timerInfo.curDelaySeconds < timerInfo.targetDelaySeconds)
                {
                    timerInfo.curDelaySeconds += deltaTime;
                }

                if (timerInfo.curDelaySeconds >= timerInfo.targetDelaySeconds)
                {
                    if (timerInfo.delayLoopInfo == null)
                    {
                        timerInfo.callback();
                        m_DicCacheTimer.Remove(m_DicCacheTimer.ElementAt(i).Key);
                        continue;
                    }
                    else if (timerInfo.delayLoopInfo.curInvokeCount == 0)
                    {
                        timerInfo.callback();
                        timerInfo.delayLoopInfo.curInvokeCount++;
                    }
                }

                if (timerInfo.delayLoopInfo?.curInvokeCount > 0)
                {
                    if (timerInfo.delayLoopInfo.curInvokeCount >= timerInfo.delayLoopInfo.targetInvokeCount
                         && timerInfo.delayLoopInfo.targetInvokeCount > 0)
                    {
                        timerInfo.delayLoopInfo.loopEndCallback?.Invoke();
                        m_DicCacheTimer.Remove(m_DicCacheTimer.ElementAt(i).Key);
                        continue;
                    }

                    timerInfo.delayLoopInfo.curLoopIntervalSeconds += deltaTime;
                    if (timerInfo.delayLoopInfo.curLoopIntervalSeconds >= timerInfo.delayLoopInfo.targetLoopIntervalSeconds)
                    {
                        timerInfo.delayLoopInfo.curLoopIntervalSeconds = 0;
                        timerInfo.callback();
                        timerInfo.delayLoopInfo.curInvokeCount++;
                    }
                }
                i++;
            }
        }


        private int AddDelayTimer(TimerInfo timerInfo)
        {
            if (timerInfo == null)
            {
                Debugger.LogError($"TimerManager-AddDelayTimer Fail, timerInfo is exist", LogType.FrameCore);
                return default;
            }
            if (m_DicCacheTimer.ContainsKey(timerInfo.timerId))
            {
                Debugger.LogError($"TimerManager-AddDelayTimer Fail, timerId:{timerInfo.timerId} is exist", LogType.FrameCore);
                return default;
            }
            m_DicCacheTimer.Add(timerInfo.timerId, timerInfo);
            return timerInfo.timerId;
        }


        public void Shutdown()
        {
            Debugger.Log("Shutdown TimerManager", LogType.FrameNormal);
        }
    }
}
