using System.Threading.Tasks;

// 模块基础接口
public interface IGameBase : IShutdown, IInit
{

}

public interface IInit
{
    Task Init();
}

public interface IShutdown
{
    void Shutdown();
}

// 可更新模块接口
public interface IUpdatableModule
{
    void OnUpdate(float deltaTime);
}

// 可销毁模块接口  
public interface IDisposableModule : IGameBase
{
    void Dispose();
}