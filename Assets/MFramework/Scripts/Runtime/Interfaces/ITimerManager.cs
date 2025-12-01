using System;
using static MFramework.Runtime.TimerManager;

namespace MFramework.Runtime
{
	public interface ITimerManager :IGameBase, IUpdatableModule
	{
        bool IsStartingUp { get; set; }
        int AddDelayTimer(float delaySeconds, Action callback);
        int AddDelayTimer(int delayMilliSeconds, Action callback);
        int AddDelayTimer(float delaySeconds, float loopSeconds, Action callback, Action loopEndCallback = null, int targetLoopCount = -1);
        void RemoveDelayTimer(int timerId);
        TimerInfo GetTimerInfo(int timerId);
        string ToString(int timerId);
    }

}