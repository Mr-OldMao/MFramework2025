using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

namespace GameMain
{
    public partial class GameMainLogic
    {
        public static GameMainLogic Instance;

        public async UniTask Init()
        {
            await InitPool();
        }

        public void GameParse()
        {
            Debug.Log("GameParse");
            Time.timeScale = Time.timeScale == 0 ? 1 : 0;
        }
    }
}
