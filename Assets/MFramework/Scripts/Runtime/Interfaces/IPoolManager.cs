using UnityEngine;
using Object = UnityEngine.Object;
namespace MFramework.Runtime
{
    public interface IPoolManager : IGameBase
    {
        int CreatPool(IPool pool);
        void DestoryPool(int poolID);
        void DestoryAllPool();
        IPool GetPool(int poolID);
    }

    public interface IPool
    {
        GameObject GetEntity();
        Object GetEntityObject();
        void RecycleEntity(Object obj);
        void DestroyPool();
    }
}