using GameMain.Generate.FlatBuffers;
using MFramework.Runtime;
using System;
using UnityEngine;

namespace GameMain
{
    public partial class PlayerEntity : TankEntityBase
    {
        private FB_tank_player m_TankPlayerData;

        private bool m_FirstInit = true;
        protected override void InitBornBefore()
        {
            InitPlayerLife();
        }


        protected override void InitBornAfter()
        {
            MoveDirType = MoveDirType.Forward;
            ChangeTankType(TankTypeID);
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
            m_TankPlayerData = DataTools.GetTankPlayer(TankTypeID);
            UpdateHP();
            UpdateBulletInterval();
            UpdateTankMoveSpeed();
        }

        #region Public

        public void AddLevel(int addNum = 1)
        {
            int id = TankTypeID + addNum;
            ChangeTankType(id);
        }
        public void SubLevel(int subNum = 1)
        {
            int id = TankTypeID - subNum;
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

        public void InitPlayerLife()
        {
            if (m_FirstInit)
            {
                m_FirstInit = false;
                remainLife = DataTools.GetConst("Player_Tank_Life");
            }
        }

        public void TryRevive()
        {
            Dead();
            //判定能否复活
            bool isCanRevive = remainLife >= 0;
            if (isCanRevive)
            {
                GameEntry.Pool.GetPool(GameMainLogic.Instance.PoolIdPlayerEnemy).GetEntity();
                //await TankBorn(TankOwnerType);
            }
        }

        public void Dead()
        {
            GameEntry.Pool.GetPool(GameMainLogic.Instance.PoolIdPlayerEnemy).RecycleEntity(entity);
            GameMainLogic.Instance.JudgeGameFail();
        }
        #endregion
    }
}
