using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFramework.Runtime
{
	public interface IPoolManager : IGameBase
	{
        GameObject Get(string poolName);
        void Return(GameObject obj, string poolName);
        void CreatePool(string poolName, GameObject prefab, int size);
    }

}