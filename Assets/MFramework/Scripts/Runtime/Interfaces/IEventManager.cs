using System;
using System.Collections;
using UnityEngine; 
public interface IEventManager : IGameBase
{
    void RegisterEvent(int eventId, Action callback);
    void UnRegisterEvent(int eventId, Action callback);
    void DispatchEvent(int eventId);


    void RegisterEvent<T>(int eventId, Action<T> callback);
    void UnRegisterEvent<T>(int eventId, Action<T> callback);
    void DispatchEvent<T>(int eventId, T eventData);


    void UnRegisterEvent(int eventId);
}
