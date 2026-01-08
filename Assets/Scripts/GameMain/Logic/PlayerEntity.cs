using GameMain.Generate.FlatBuffers;
using MFramework.Runtime;
using System;
using UnityEngine;

namespace GameMain
{
    public partial class PlayerEntity : TankEntityBase
    {
        private FB_tank_player m_TankPlayerData;

        /// <summary>
        /// 下次坦克生成是否初始化坦克剩余生命
        /// </summary>
        public bool IsInitLife = true;
        protected override void InitBornBefore()
        {
            UpdatePlayerLife();
        }

        protected override void InitBornAfter()
        {
            ChangeTankType(tankTypeID);
            InitMove(new Vector2(entity.transform.localPosition.x, entity.transform.localPosition.z));
            InitFire();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            Move();

            FireByKeyCode();
        }

        private void ChangeTankType(int id)
        {
            if (DataTools.GetTankPlayer(id).ByteBuffer == null)
            {
                Debugger.LogError($"没有该坦克数据 id:{id}");
                return;
            }
            UpdateTankData(id);
            m_TankPlayerData = DataTools.GetTankPlayer(tankTypeID);
            UpdateHP();
            UpdateBulletInterval();
            UpdateTankMoveSpeed();
        }

        #region Public

        //重置坦克数据
        public void ResetTankData()
        {
            UpdatePlayerLife();
        }

        public void RecycleTank()
        {
            entity.SetActive(false);
            SubLife();
            IsCanMove = false;
            m_IsCanFire = false;
        }

        public void AddLevel(int addNum = 1)
        {
            int id = tankTypeID + addNum;
            ChangeTankType(id);
        }
        public void SubLevel(int subNum = 1)
        {
            int id = tankTypeID - subNum;
            ChangeTankType(id);
        }

        public void UpdateHP()
        {
            HP = m_TankPlayerData.HP;
        }

        public void SubLife()
        {
            --remainLife;
        }

        public void UpdatePlayerLife()
        {
            if (IsInitLife)
            {
                remainLife = DataTools.GetConst("Player_Tank_Life");
                GameEntry.UI.GetView<UIPanelBattle>().RefreshUI();
            }
        }


        public void TryRevive()
        {
            Dead();
            //判定能否复活
            bool isCanRevive = remainLife >= 0;
            IsInitLife = !isCanRevive;
            if (isCanRevive)
            {
                GameEntry.Pool.GetPool(GameMainLogic.Instance.PoolIdTankPlayer).GetEntity();
            }
        }

        public void Dead()
        {
            GameEntry.Pool.GetPool(GameMainLogic.Instance.PoolIdTankPlayer).RecycleEntity(entity);
            IsExtendBeforeDataNextGenerate = false;
            GameMainLogic.Instance.JudgeGameFail();
        }
        #endregion
    }
}
