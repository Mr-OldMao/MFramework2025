using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestFreamwork : MonoBehaviour
{
    public void Test()
    {
        FrameworkManager.Instance.LoggerModule.Log("-----------------------------------");

    }

    #region event
    [ContextMenu("事件/注册无参事件")]
    public void EventRegister()
    {
        FrameworkManager.Instance.GetModule<EventManager>().RegisterEvent((int)GameEventType.SceneUnloaded, EventCallback);
        FrameworkManager.Instance.LoggerModule.Log("注册无参事件");
    }
    [ContextMenu("事件/注销无参事件")]
    public void EventUnregister()
    {
        FrameworkManager.Instance.GetModule<EventManager>().UnRegisterEvent((int)GameEventType.SceneUnloaded, EventCallback);
        FrameworkManager.Instance.LoggerModule.Log("注销无参事件");
    }
    [ContextMenu("事件/派发无参事件")]
    public void EventDispatch()
    {
        FrameworkManager.Instance.GetModule<EventManager>().DispatchEvent((int)GameEventType.SceneUnloaded);
        FrameworkManager.Instance.LoggerModule.Log("派发无参事件");
    }
    private void EventCallback()
    {
        Debug.Log("EventCallback() 无参事件回调");
    }


    [ContextMenu("事件/注册带参事件")]
    public void EventTRegister()
    {
        FrameworkManager.Instance.GetModule<EventManager>().RegisterEvent<EventParamClass>((int)GameEventType.SceneUnloaded, EventTCallback);
        FrameworkManager.Instance.LoggerModule.Log("注册带参事件");
    }
    [ContextMenu("事件/注销带参事件")]
    public void EventTUnregister()
    {
        FrameworkManager.Instance.GetModule<EventManager>().UnRegisterEvent<EventParamClass>((int)GameEventType.SceneUnloaded, EventTCallback);
        FrameworkManager.Instance.LoggerModule.Log("注销带参事件");
    }
    [ContextMenu("事件/派发带参事件")]
    public void EventTDispatch()
    {
        FrameworkManager.Instance.GetModule<EventManager>().DispatchEvent((int)GameEventType.SceneUnloaded, new EventParamClass
        {
            id = 1001,
            des = "1001参数描述"
        });
        FrameworkManager.Instance.LoggerModule.Log("派发带参事件");
    }
    private void EventTCallback(EventParamClass p)
    {
        Debug.Log($"EventTCallback(p) 带参事件回调 id:{p.id} des:{p.des}");
    }

    public class EventParamClass
    {
        public int id;
        public string des;
    }

    [ContextMenu("事件/注销当前事件id所有事件")]
    public void EventAllUnregister()
    {
        FrameworkManager.Instance.GetModule<EventManager>().UnRegisterEvent((int)GameEventType.SceneUnloaded);
        FrameworkManager.Instance.LoggerModule.Log("注销当前事件id所有事件");
    }

    //[ContextMenu("事件/注册无参事件,销毁时自动解绑")]
    //public void RegisterEventAndAutoBindUnregister()
    //{
    //    FrameworkManager.Instance.GetModule<EventManager>().RegisterEvent((int)GameEventType.SceneUnloaded, EventCallback,this);
    //    FrameworkManager.Instance.LoggerModule.Log("注册无参事件,销毁时自动解绑");
    //}
    #endregion
}
