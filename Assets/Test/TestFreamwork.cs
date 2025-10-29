using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFramework.Runtime;
public class TestFreamwork : MonoBehaviour
{
    public void Test()
    {
        Debugger.Log("开始测试");
    }

    #region event
    [ContextMenu("事件/注册无参事件")]
    public void EventRegister()
    {
        GameEntry.Event.RegisterEvent((int)GameEventType.TestEvent, EventCallback);
        Debugger.Log("注册无参事件");
    }
    [ContextMenu("事件/注销无参事件")]
    public void EventUnregister()
    {
        GameEntry.Event.UnRegisterEvent((int)GameEventType.TestEvent, EventCallback);
        Debugger.Log("注销无参事件");
    }
    [ContextMenu("事件/派发无参事件")]
    public void EventDispatch()
    {
        GameEntry.Event.DispatchEvent((int)GameEventType.TestEvent);
        Debugger.Log("派发无参事件");
    }
    private void EventCallback()
    {
        UnityEngine.Debug.Log("EventCallback() 无参事件回调");
    }


    [ContextMenu("事件/注册带参事件")]
    public void EventTRegister()
    {
        GameEntry.Event.RegisterEvent<EventParamClass>((int)GameEventType.TestEvent, EventTCallback);
        Debugger.Log("注册带参事件");
    }
    [ContextMenu("事件/注销带参事件")]
    public void EventTUnregister()
    {
        GameEntry.Event.UnRegisterEvent<EventParamClass>((int)GameEventType.TestEvent, EventTCallback);
        Debugger.Log("注销带参事件");
    }
    [ContextMenu("事件/派发带参事件")]
    public void EventTDispatch()
    {
        GameEntry.Event.DispatchEvent((int)GameEventType.TestEvent, new EventParamClass
        {
            id = 1001,
            des = "1001参数描述"
        });
        Debugger.Log("派发带参事件");
    }
    private void EventTCallback(EventParamClass p)
    {
        UnityEngine.Debug.Log($"EventTCallback(p) 带参事件回调 id:{p.id} des:{p.des}");
    }

    public class EventParamClass
    {
        public int id;
        public string des;
    }

    [ContextMenu("事件/注销当前事件id所有事件")]
    public void EventAllUnregister()
    {
        GameEntry.Event.UnRegisterEvent((int)GameEventType.TestEvent);
        Debugger.Log("注销当前事件id所有事件");
    }

    //[ContextMenu("事件/注册无参事件,销毁时自动解绑")]
    //public void RegisterEventAndAutoBindUnregister()
    //{
    //    GameEntry.Event.RegisterEvent((int)GameEventType.Test, EventCallback,this);
    //    Debugger.Log("注册无参事件,销毁时自动解绑");
    //}
    #endregion
}
