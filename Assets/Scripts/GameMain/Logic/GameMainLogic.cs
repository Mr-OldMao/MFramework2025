using Cysharp.Threading.Tasks;
using MFramework.Runtime;
using System;
using System.Collections;
using UnityEngine;

namespace GameMain
{
    public partial class GameMainLogic
    {
        public bool IsDebugger { get; private set; }

        public static GameMainLogic Instance;

        public int StageID { get; set; } = 1;

        /// <summary>
        /// 当前关卡开局是否拥有道具奖励
        /// </summary>
        public bool IsCurStageReward = false;

        private int m_TimerIdDelayShowAdv;
        public async UniTask Init()
        {
#if UNITY_EDITOR
            IsDebugger = true;
#else
            IsDebugger = false;
#endif

            InitRegisterEvent();
            InitUserData();
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

            if (m_TimerIdDelayShowAdv > 0)
            {
                GameEntry.Timer.RemoveDelayTimer(m_TimerIdDelayShowAdv);
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

        public void GameParseAutoShowInsertAdv(bool isPlaySound = true, float delayTime = 5f)
        {
            Debug.Log("GameParse");
            Time.timeScale = Time.timeScale == 0 ? 1 : 0;
            if (isPlaySound)
            {
                GameEntry.Audio.PlaySound("pause.ogg");
            }
            if (Time.timeScale == 1)
            {
                if (m_TimerIdDelayShowAdv > 0)
                {
                    GameEntry.Timer.RemoveDelayTimer(m_TimerIdDelayShowAdv);
                }
            }
            if (Time.timeScale == 0)
            {
                m_TimerIdDelayShowAdv = GameEntry.Timer.AddDelayTimer(delayTime, () =>
                {
                    m_TimerIdDelayShowAdv = 0;

                    TTSDKManager.Instance.ShowAdvInsert(() =>
                    {
                        Debug.Log("Show Adv Insert close callback");
                    });
                }, true);
            }

            GameEntry.Audio.StopBGM();
        }
    }
}
