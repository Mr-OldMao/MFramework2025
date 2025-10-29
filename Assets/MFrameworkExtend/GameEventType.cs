/// <summary>
/// 事件类型枚举
/// </summary>
public enum GameEventType
{
    TestEvent = 0,

    #region 框架层事件
    /// <summary>
    /// 场景加载开始
    /// </summary>
    SceneLoadStart = 500,
    /// <summary>
    /// 场景加载中
    /// </summary>
    SceneLoading = 501,
    /// <summary>
    /// 场景加载完成
    /// </summary>
    SceneLoaded = 502,
    #endregion
}