// FrameworkConfig.cs - 可配置的模块开关
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

namespace MFramework.Editor
{
    [CreateAssetMenu(fileName = "FrameworkConfig", menuName = "Game Framework/Config")]
    public class FrameworkConfig : ScriptableObject
    {
        [Header("核心模块配置")]
        public bool enableLogSystem = true;
        public LogLevel logLevel = LogLevel.Info;

        [Header("功能模块配置")]
        public bool enableNetwork = false;
        public bool enableHotUpdate = false;
        public bool enableDebugTools = true;

        [Header("初始化设置")]
        public bool preloadCommonResources = true;
        public int targetFrameRate = 60;

        [Header("模块初始化优先级")]
        public List<string> moduleInitOrder = new List<string>
    {
        "LoggerModule",
        "EventManager",
        "ConfigManager",
        "PoolManager",
        "ResourceManager",
        "SceneManager",
        "UIManager",
        "AudioManager",
        "TimerManager"
    };
    } 
}