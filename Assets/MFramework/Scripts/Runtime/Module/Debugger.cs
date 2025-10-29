namespace MFramework.Runtime
{
    public static class Debugger
    {
        private static ILoggerModule _logger;

        public static void Initialize(ILoggerModule loggerModule)
        {
            _logger = loggerModule;
            Log("全局日志系统初始化完成", LogType.FrameCore);
        }

        public static void Log(string message, LogType logType = LogType.Default)
        {
            _logger?.Log(message, logType);
        }

        public static void LogWarning(string message, LogType logType = LogType.Default)
        {
            _logger?.LogWarning(message, logType);
        }

        public static void LogError(string message, LogType logType = LogType.Default)
        {
            _logger?.LogError(message, logType);
        }

        public static void InfoFormat(string format, params object[] args)
        {
            Log(string.Format(format, args));
        }

        public static void WarningFormat(string format, params object[] args)
        {
            LogWarning(string.Format(format, args));
        }

        public static void ErrorFormat(string format, params object[] args)
        {
            LogError(string.Format(format, args));
        }


        //[System.Diagnostics.Conditional("UNITY_EDITOR")]
        //[System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        //public static void Debug(string message, LogType logType = LogType.Default)
        //{
        //    Log($"[DEBUG] {message}", logType);
        //}

    }
}