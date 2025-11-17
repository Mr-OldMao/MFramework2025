// 自动处理模块依赖关系
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MFramework.Runtime;
using UnityEngine;
public class ModuleDependencyResolver
{
    //public List<Type> ResolveInitOrder(List<IGameModule> modules)
    //{
    //    // 根据模块优先级和依赖关系排序
    //    return modules.OrderBy(m => m.Priority)
    //                 .Select(m => m.GetType())
    //                 .ToList();
    //}

    public void ValidateDependencies(IGameModule module)
    {
        var dependencies = module.GetType()
            .GetCustomAttributes(typeof(DependsOnAttribute), true)
            .Cast<DependsOnAttribute>();

        foreach (var dep in dependencies)
        {
            if (FrameworkManager.Instance.GetModule(dep.ModuleType) == null)
            {
                throw new Exception($"模块 {module.GetType().Name} 依赖 {dep.ModuleType.Name} 未注册");
            }
        }
    }
}

// 依赖标记特性
[AttributeUsage(AttributeTargets.Class)]
public class DependsOnAttribute : Attribute
{
    public Type ModuleType { get; }

    public DependsOnAttribute(Type moduleType)
    {
        ModuleType = moduleType;
    }
}

//// 使用示例
//[DependsOn(typeof(ILoggerModule))]
////[DependsOn(typeof(IEventManager))]
//public class UIManager : GameModuleBase, IUIManager
//{
//    public T GetPanel<T>(string panelName) where T : Component
//    {
//        throw new NotImplementedException();
//    }

//    public void HidePanel<T>() where T : class
//    {
//        throw new NotImplementedException();
//    }

//    public void HidePanel(string panelName)
//    {
//        throw new NotImplementedException();
//    }

//    public void OnUpdate(float deltaTime)
//    {
//        throw new NotImplementedException();
//    }

//    public void ShowPanel<T>() where T : class
//    {
//        throw new NotImplementedException();
//    }

//    public void ShowPanel(string panelName)
//    {
//        throw new NotImplementedException();
//    }

//    // UI管理器依赖日志和事件系统
//    protected override Task OnInitialize()
//    {
//        throw new NotImplementedException();
//    }

//    protected override void OnShutdown()
//    {
//        throw new NotImplementedException();
//    }
//}