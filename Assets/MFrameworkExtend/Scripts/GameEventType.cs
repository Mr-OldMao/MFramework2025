/// <summary>
/// 事件类型枚举
/// </summary>
public enum GameEventType
{
    TestEvent = 0,
    TestUIEvent = 101,

    LoadingProgress = 200,

    GameStart = 300,

    GamePause,
    GameWin,
    GameFail,

    /// <summary>
    /// 坦克被击中
    /// </summary>
    TankBeHit = 400,

    /// <summary>
    /// 玩家1坦克死亡
    /// </summary>
    Player1TankDead =410,
    /// <summary>
    /// 玩家2坦克死亡
    /// </summary>
    Player2TankDead = 411,
    /// <summary>
    /// 敌方坦克死亡
    /// </summary>
    EnemyTankDead = 412,

    /// <summary>
    /// 敌方坦克出生
    /// </summary>
    EnemyTankGenerate,

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