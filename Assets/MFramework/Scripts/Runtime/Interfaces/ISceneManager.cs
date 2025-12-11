using System;
using UnityEngine.SceneManagement;
namespace MFramework.Runtime
{
    public interface ISceneManager : IGameBase
    {
        void LoadSceneAsync(string sceneName, LoadSceneMode sceneLoadMode = LoadSceneMode.Single, Action<float> loadingCallback = null,
                            Action loadedCallback = null, Action unloadedCallback = null, float minLoadTime = 1.0f);
        void UnloadSceneAsync(string sceneName, Action<float> unloadingCallback = null, Action unloadedCallback = null);
        void PreloadSceneAsync(string sceneName, LoadSceneMode sceneLoadMode, Action<float> loadingCallback = null, Action loadedCallback = null);
        void ReloadCurrentScene(LoadSceneMode sceneLoadMode = LoadSceneMode.Single,Action<float> loadingCallback = null, Action loadedCallback = null, float minLoadTime = 1.0f);

        string CurrentScene { get; }
    }


    ///// <summary>
    ///// 场景加载模式
    ///// </summary>
    //public enum LoadSceneMode
    //{
    //    /// <summary>
    //    /// 单一模式，卸载所有其他场景
    //    /// </summary>
    //    Single,
    //    /// <summary>
    //    /// 叠加模式，保留当前场景
    //    /// </summary>
    //    Additive
    //}

    public enum ESceneStateType
    {
        None,
        Loading,
        Loaded,

        Unloading,
        Unloaded,

        Preloading,
        Preloaded
    }
}