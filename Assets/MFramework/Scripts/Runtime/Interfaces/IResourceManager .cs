using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace MFramework.Runtime
{
	public interface IResourceManager : IGameModule
	{
        T LoadAsset<T>(string path) where T : UnityEngine.Object;
        Task<T> LoadAssetAsync<T>(string path) where T : UnityEngine.Object;
        void UnloadAsset(string path);
    }

}