using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace MFramework.Runtime
{
	public interface ISceneManager : IGameModule
	{
        Task LoadSceneAsync(string sceneName);
        Task UnloadSceneAsync(string sceneName);
        string CurrentScene { get; }
    }

}