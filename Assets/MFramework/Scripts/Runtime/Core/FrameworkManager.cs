using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MFramework.Runtime
{
    // FrameworkManager.cs - 框架核心管理器
    public class FrameworkManager
    {
        private readonly Dictionary<Type, IGameBase> _modules = new();
        private readonly List<IUpdatableModule> _updateModules = new();

        public static FrameworkManager Instance { get; private set; }

        public FrameworkManager()
        {
            Instance = this;
        }

        // 模块注册系统
        public void RegisterModule<T>(T module) where T : IGameBase
        {
            var type = GetInterfaceType(module);// typeof(T);

            // 检查是否已注册
            if (_modules.ContainsKey(type))
            {
                Debugger.LogWarning($"模块 {type.Name} 已经注册过了", LogType.FrameNormal);
                return;
            }

            _modules[type] = module;

            if (module is IUpdatableModule updatable)
            {
                _updateModules.Add(updatable);
            }

            Debugger.Log($"模块 {module.GetType()} 注册成功", LogType.FrameCore);
        }

        private Type GetInterfaceType(IGameBase module)
        {
            // 通过反射获取模块实现的主要接口
            var interfaces = module.GetType().GetInterfaces()
                .Where(i => typeof(IGameBase).IsAssignableFrom(i) && i != typeof(IGameBase))
                .FirstOrDefault();

            return interfaces ?? module.GetType();
        }

        public T GetModule<T>() where T : class, IGameBase
        {
            var type = typeof(T);
            _modules.TryGetValue(type, out var module);
            if (module != null)
            {
                return module as T;
            }
            else
            {
                foreach (var item in _modules.Values)
                {
                    if (item.GetType() == type)
                    {
                        return item as T;
                    }
                }
            }
            return null;
        }

        // 非泛型获取模块（用于依赖检查）
        public IGameBase GetModule(Type moduleType)
        {
            _modules.TryGetValue(moduleType, out var module);
            return module;
        }

        public void Update()
        {
            var deltaTime = Time.deltaTime;
            foreach (var module in _updateModules)
            {
                try
                {
                    (module as IUpdatableModule)?.OnUpdate(deltaTime);
                }
                catch (Exception e)
                {
                    Debugger.LogError($"模块 {module.GetType().Name} 更新异常: {e}", LogType.FrameCore);
                }
            }
        }

        // 关闭框架时清理所有模块
        public void OnDestroy()
        {
            foreach (var module in _modules.Values)
            {
                module.Shutdown();

                if (module is IDisposableModule disposable)
                {
                    disposable.Dispose();
                }
            }

            _modules.Clear();
            _updateModules.Clear();
            Instance = null;
        }
    }
}