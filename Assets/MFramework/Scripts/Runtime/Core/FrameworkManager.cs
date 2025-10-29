using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MFramework.Runtime
{
    // FrameworkManager.cs - 框架核心管理器
    public class FrameworkManager
    {
        private readonly Dictionary<Type, IGameModule> _modules = new();
        private readonly List<IUpdatableModule> _updateModules = new();

        // 服务定位器
        public static FrameworkManager Instance { get; private set; }

        public FrameworkManager()
        {
            Instance = this;
        }

        // 模块注册系统
        public void RegisterModule<T>(T module) where T : IGameModule
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

        private Type GetInterfaceType(IGameModule module)
        {
            // 通过反射获取模块实现的主要接口
            var interfaces = module.GetType().GetInterfaces()
                .Where(i => typeof(IGameModule).IsAssignableFrom(i) && i != typeof(IGameModule))
                .FirstOrDefault();

            return interfaces ?? module.GetType();
        }

        public T GetModule<T>() where T : class, IGameModule
        {
            _modules.TryGetValue(typeof(T), out var module);

            if (module != null)
            {
                return module as T;
            }
            else
            {
                foreach (var item in _modules.Values)
                {
                    if (item.GetType() == typeof(T))
                    {
                        return item as T;
                    }
                }
                //foreach (var kvp in _modules)
                //{
                //    if (typeof(T).IsAssignableFrom(kvp.Key))
                //    {
                //        return kvp.Value as T;
                //    }
                //}
            }
            return null;
        }

        // 非泛型获取模块（用于依赖检查）
        public IGameModule GetModule(Type moduleType)
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