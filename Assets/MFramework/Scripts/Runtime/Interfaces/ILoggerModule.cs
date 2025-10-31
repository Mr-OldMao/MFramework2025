public interface ILoggerModule : IGameModule //, IUpdatableModule
{
    void Log(string content, LogType logType = LogType.Default); 
    void LogWarning(string content, LogType logType = LogType.Default);
    void LogError(string content, LogType logType = LogType.Default);
}

/// <summary>
/// 日志类型
/// </summary>
public enum LogType
{
    Default = 0,
    /// <summary>
    /// 测试日志
    /// </summary>
    Test,
    /// <summary>
    /// 框架普通日志
    /// </summary>
    FrameNormal,
    /// <summary>
    /// 框架重要日志
    /// </summary>
    FrameCore,
}
