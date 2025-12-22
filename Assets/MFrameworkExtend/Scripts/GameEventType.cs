/// <summary>
/// 事件类型枚举
/// </summary>
public enum GameEventType
{
    TestEvent = 0,
    TestUIEvent = 101,

    LoadingProgress = 200,

    GameStart = 300,
    GameOver = 301,

    /// <summary>
    /// 坦克被击中
    /// </summary>
    TankHit = 400,

    /// <summary>
    /// 坦克死亡
    /// </summary>
    TankDead =410,

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