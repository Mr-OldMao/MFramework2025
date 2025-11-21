using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFramework.Runtime
{
	public interface ITimerManager :IGameBase, IUpdatableModule
	{
        int AddTimer(float delay, System.Action callback);
        void RemoveTimer(int timerId);
        void PauseTimer(int timerId);
        void ResumeTimer(int timerId);
    }

}