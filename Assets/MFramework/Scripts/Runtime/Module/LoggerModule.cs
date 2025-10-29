using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LoggerModule : GameModuleBase, ILoggerModule
{
    //TODO:¥ÌŒÛ…œ±®µ»

    public void Log(string content, LogType logType = LogType.Default)
    {
        switch (logType)
        {
            case LogType.Default:
                Debug.Log($"{content}");
                break;
            case LogType.FrameNormal:
                Debug.Log($"<Color=#FFC95D>{content}</Color>");
                break;
            case LogType.FrameCore:
                Debug.Log($"<Color=#FF4C4C>----------{content}----------</Color>");
                break;
        }
    }

    public void LogError(string content, LogType logType = LogType.Default)
    {
        switch (logType)
        {
            case LogType.Default:
                Debug.LogError($"{content}");
                break;
            case LogType.FrameNormal:
                Debug.LogError($"<Color=#FFC95D{content}</Color>");
                break;
            case LogType.FrameCore:
                Debug.LogError($"<Color=#FF4C4C>----------{content}----------</Color>");
                break;
        }
    }

    public void LogWarning(string content, LogType logType = LogType.Default)
    {
        switch (logType)
        {
            case LogType.Default:
                Debug.LogWarning($"{content}");
                break;
            case LogType.FrameNormal:
                Debug.LogWarning($"<Color=#FFC95D{content}</Color>");
                break;
            case LogType.FrameCore:
                Debug.LogWarning($"<Color=#FF4C4C>----------{content}----------</Color>");
                break;
        }
    }

    protected override Task OnInitialize()
    {
        Debug.Log("------OnInitialize   LoggerModule");
        return Task.CompletedTask;
    }

    protected override void OnShutdown()
    {
        Debug.Log("------OnShutdown");
    }
}
