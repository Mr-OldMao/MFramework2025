using Cysharp.Threading.Tasks;
using UnityEngine;

// 模块基础接口
public interface IGameBase : IShutdown, IInit
{

}

public interface IInit
{
    UniTask Init();
}

public interface IShutdown
{
    void Shutdown();
}

// 可更新模块接口
public interface IUpdatableModule
{
    void OnUpdate(float deltaTime, float unscaledDeltaTime);
}

public interface IUpdateModule
{
    void OnUpdate(float unscaledDeltaTime);
}

// 可销毁模块接口  
public interface IDisposableModule : IGameBase
{
    void Dispose();
}