using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MFramework.Runtime
{
	public interface ISceneManager : IGameBase
	{
        UniTask LoadSceneAsync(string sceneName);
        UniTask UnloadSceneAsync(string sceneName);
        string CurrentScene { get; }
    }

}