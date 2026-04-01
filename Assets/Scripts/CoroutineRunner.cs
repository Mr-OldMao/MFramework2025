using System.Collections;
using UnityEngine;

/// <summary>
/// 全局协程执行者（任意类都能调用，无需继承 MonoBehaviour）
/// 自动创建、永不销毁、线程安全
/// </summary>
public static class CoroutineRunner
{
    // 内部宿主（自动生成）
    private static MonoBehaviour _runner;

    static CoroutineRunner()
    {
        // 静态构造函数：保证游戏启动时自动创建宿主
        CreateRunner();
    }

    private static void CreateRunner()
    {
        if (_runner != null) return;

        GameObject go = new GameObject("【Coroutine Runner】");
        Object.DontDestroyOnLoad(go);
        _runner = go.AddComponent<CoroutineHost>();
    }

    // 启动协程
    public static Coroutine Start(IEnumerator enumerator)
    {
        if (_runner == null) CreateRunner();
        return _runner.StartCoroutine(enumerator);
    }

    // 停止协程
    public static void Stop(Coroutine coroutine)
    {
        if (_runner != null && coroutine != null)
            _runner.StopCoroutine(coroutine);
    }

    // 内部空宿主
    private class CoroutineHost : MonoBehaviour { }
}