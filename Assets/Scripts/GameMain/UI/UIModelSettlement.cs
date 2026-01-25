using MFramework.Runtime;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace GameMain
{
    public  class UIModelSettlement : UIModelBase
    {
        private Dictionary<int, KillDataInfo> m_DicPlayer1KillData = new Dictionary<int, KillDataInfo>();
        private Dictionary<int, KillDataInfo> m_DicPlayer2KillData = new Dictionary<int, KillDataInfo>();

        public int LastTotalScore { get; set; }
        public Dictionary<int, KillDataInfo> GetDicPlayer1KillData()
        {
            return m_DicPlayer1KillData;
        }

        public Dictionary<int, KillDataInfo> GetDicPlayer2KillData()
        {
            return m_DicPlayer2KillData;
        }

        public UIModelSettlement(IUIController controller) : base(controller)
        {

        }

        public override async UniTask Init()
        {
            await UniTask.CompletedTask;
        }

        public void InitData()
        {
            foreach (var item in DataTools.GetTankEnemys().Keys)
            {
                m_DicPlayer1KillData.Add(item, new KillDataInfo());
                m_DicPlayer2KillData.Add(item, new KillDataInfo());
            }
        }

        public void AddScore(int enemyTypeID, TankOwnerType killTank)
        {
            switch (killTank)
            {
                case TankOwnerType.Player1:
                    if (!m_DicPlayer1KillData.ContainsKey(enemyTypeID))
                    {
                        m_DicPlayer1KillData.Add(enemyTypeID, new KillDataInfo());
                    }
                    m_DicPlayer1KillData[enemyTypeID].Kill();
                    break;
                case TankOwnerType.Player2:
                    if (!m_DicPlayer2KillData.ContainsKey(enemyTypeID))
                    {
                        m_DicPlayer2KillData.Add(enemyTypeID, new KillDataInfo());
                    }
                    m_DicPlayer2KillData[enemyTypeID].Kill();
                    break;
            }
        }

        public void ResetScore()
        {
            foreach (var item in m_DicPlayer1KillData.Values)
            {
                item.Reset();
            }
            foreach (var item in m_DicPlayer2KillData.Values)
            {
                item.Reset();
            }
        }

        public class KillDataInfo
        {
            public int KillCount { get; private set; }

            public void Kill()
            {
                KillCount++;
            }

            public void Reset()
            {
                KillCount = 0;
            }
        }
    }
}
