using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace MFramework.Runtime
{
    public class LoggerModule : GameModuleBase, ILoggerModule
    {
        //TODO:错误上报等

        private readonly string Color_Test = "#5EB6FF";
        private readonly string Color_FrameNormal = "#FFC95D";
        private readonly string Color_FrameCore = "#FF4C4C";

        public void Log(string content, LogType logType = LogType.Default)
        {
            switch (logType)
            {
                case LogType.Default:
                    Debug.Log(content);
                    break;
                case LogType.Test:
                    Debug.Log(SetColor(content, Color_Test));
                    break;
                case LogType.FrameNormal:
                    Debug.Log(SetColor(content, Color_FrameNormal));
                    break;
                case LogType.FrameCore:
                    Debug.Log(SetColor(content, Color_FrameCore));
                    break;
            }
        }

        public void LogError(string content, LogType logType = LogType.Default)
        {
            switch (logType)
            {
                case LogType.Default:
                    Debug.LogError(content);
                    break;
                case LogType.Test:
                    Debug.LogError(SetColor(content, Color_Test));
                    break;
                case LogType.FrameNormal:
                    Debug.LogError(SetColor(content, Color_FrameNormal));
                    break;
                case LogType.FrameCore:
                    Debug.LogError(SetColor(content, Color_FrameCore));
                    break;
            }
        }

        public void LogWarning(string content, LogType logType = LogType.Default)
        {
            switch (logType)
            {
                case LogType.Default:
                    Debug.LogWarning(content);
                    break;
                case LogType.Test:
                    Debug.LogWarning(SetColor(content, Color_Test));
                    break;
                case LogType.FrameNormal:
                    Debug.LogWarning(SetColor(content, Color_FrameNormal));
                    break;
                case LogType.FrameCore:
                    Debug.LogWarning(SetColor(content, Color_FrameCore));
                    break;
            }
        }

        public string SetColor(string content, string color)
        {
            string[] lines = content.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = $"<color={color}>{lines[i]}</color>";
            }
            string coloredContent = string.Join("\n", lines);
            return coloredContent;
        }

        protected override Task OnInitialize()
        {
            Debugger.Initialize(this);
            return Task.CompletedTask;
        }

        protected override void OnShutdown()
        {
            Debug.Log("OnShutdown LoggerModule");
        }
    }

}