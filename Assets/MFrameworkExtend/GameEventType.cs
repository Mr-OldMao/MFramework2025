/// <summary>
/// 事件类型枚举
/// </summary>
public enum GameEventType
{
    #region Test
    // 玩家相关事件
    PlayerHealthChanged = 1001,
    PlayerLevelUp = 1002,
    PlayerDeath = 1003,

    // 物品相关事件
    ItemCollected = 2001,
    ItemUsed = 2002,

    // 场景相关事件
    SceneLoaded = 3001,
    SceneUnloaded = 3002,

    // UI相关事件
    UIOpened = 4001,
    UIClosed = 4002,

    // 游戏状态事件
    GamePaused = 5001,
    GameResumed = 5002,
    GameOver = 5003 
    #endregion
}