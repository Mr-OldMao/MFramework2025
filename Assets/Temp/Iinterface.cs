//// 各模块接口定义
//using UnityEngine;

//public interface IEventManager : IGameModule
//{
//    void Publish(GameEvent gameEvent);
//}

//public interface IResourceManager : IGameModule
//{
//    GameObject LoadPrefab(string path);
//}

//public interface IUIManager : IGameModule
//{
//    void ShowUI(string uiName);
//    void HideUI(string uiName);
//}

//public interface IAudioManager : IGameModule
//{
//    void PlaySound(string soundName);
//    void PlayMusic(string musicName);
//}

//public interface ITimerManager : IGameModule, IUpdatableModule
//{
//    void AddTimer(float duration, Action callback);
//}

//public interface IDebugManager : IGameModule
//{
//    void ShowDebugPanel();
//}