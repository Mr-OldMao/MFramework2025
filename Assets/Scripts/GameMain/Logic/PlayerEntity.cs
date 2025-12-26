using GameMain.Generate.FlatBuffers;
using MFramework.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GameMain
{
    public partial class PlayerEntity : TankEntityBase
    {
        private FB_tank_player m_TankPlayerData;


        protected override void Init()
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

            Fire();
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

        public void Revive()
        {
            Dead();
            //判定能否复活
            bool isCanRevive = true;
            if (isCanRevive)
            {
                GameEntry.Pool.GetPool(GameMainLogic.Instance.PoolIdPlayerEnemy).GetEntity();
                //await TankBorn(TankOwnerType);
            }

        }

        public void Dead()
        {
            GameEntry.Pool.GetPool(GameMainLogic.Instance.PoolIdPlayerEnemy).RecycleEntity(entity);
        }
        #endregion
    }
}
