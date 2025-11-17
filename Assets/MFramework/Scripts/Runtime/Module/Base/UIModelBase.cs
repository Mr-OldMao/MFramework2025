using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFramework.Runtime
{
    public abstract class UIModelBase : IUIModel
    {
        public virtual void Initialize() { }
        public virtual void Reset() { }

        public void DispatchEvent(int eventId)
        {
            GameEntry.Event.DispatchEvent(eventId);
        }

        public void DispatchEvent(GameEventType gameEventType)
        {
            GameEntry.Event.DispatchEvent(gameEventType);
        }

        public void DispatchEvent<T>(int eventId, T eventData)
        {
            GameEntry.Event.DispatchEvent(eventId, eventData);
        }
    }
}
