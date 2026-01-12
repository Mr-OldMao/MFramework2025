using MFramework.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMain
{
    public partial class PlayerEntity
    {
        public override void GameWinEvent()
        {
            IsCanMove = false;
            m_IsCanFire = false;
            GameEntry.Audio.StopBGM();

            IsExtendBeforeDataNextGenerate = true;
        }

        public override void GameFailEvent()
        {
            IsCanMove = false;
            m_IsCanFire = false;
            GameEntry.Audio.StopBGM();

            IsInitLife = true;
            IsExtendBeforeDataNextGenerate = false;
        }

    }
}
