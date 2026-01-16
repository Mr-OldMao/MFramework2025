using Cysharp.Threading.Tasks;
using MFramework.Runtime;
using UnityEngine;

namespace GameMain
{
    public partial class GameMainLogic
    {
        public static GameMainLogic Instance;

        public int StageID { get; set; } = 1;

        public async UniTask Init()
        {
            InitRegisterEvent();
            await InitPool();
        }

        public void GameParse(bool isParse, bool isPlaySound = true)
        {
            Debug.Log("GameParse");
            Time.timeScale = isParse ? 0 : 1;
            if (isPlaySound)
            {
                GameEntry.Audio.PlaySound("pause.ogg");
            }
            GameEntry.Audio.StopBGM();
        }

        public void GameParse(bool isPlaySound = true)
        {
            Debug.Log("GameParse");
            Time.timeScale = Time.timeScale == 0 ? 1 : 0;
            if (isPlaySound)
            {
                GameEntry.Audio.PlaySound("pause.ogg");
            }
            GameEntry.Audio.StopBGM();
        }
    }
}
