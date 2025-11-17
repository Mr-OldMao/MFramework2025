using System.Threading.Tasks;

// 模块基础接口
public interface IGameModule
{
    //int Priority { get; } // 初始化优先级
    Task Initialize();
    void Shutdown();
}

// 可更新模块接口
public interface IUpdatableModule 
{
    void OnUpdate(float deltaTime);
}

// 可销毁模块接口  
public interface IDisposableModule : IGameModule
{
    void Dispose();
}