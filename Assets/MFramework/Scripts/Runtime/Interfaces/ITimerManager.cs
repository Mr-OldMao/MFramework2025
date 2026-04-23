using System;
using static MFramework.Runtime.TimerManager;

namespace MFramework.Runtime
{
    public interface ITimerManager : IGameBase, IUpdatableModule
    {
        bool IsStartingUp { get; set; }
        int AddDelayTimer(float delaySeconds, Action callback, bool isUnscaledDeltaTime = false);
        int AddDelayTimer(int delayMilliSeconds, Action callback, bool isUnscaledDeltaTime = false);
        int AddDelayTimer(float delaySeconds, float loopSeconds, Action callback, Action loopEndCallback = null, int targetLoopCount = -1, bool isUnscaledDeltaTime = false);
        void RemoveDelayTimer(int timerId);
        TimerInfo GetTimerInfo(int timerId);
        string ToString(int timerId);
    }

}