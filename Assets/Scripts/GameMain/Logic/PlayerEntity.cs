using GameMain.Generate.FlatBuffers;
using MFramework.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMain
{
    public partial class PlayerEntity : TankEntityBase
    {
        private GameObject player;
        private Transform imgTankIcon;

        private MoveDirType moveDirType = MoveDirType.Forward;

        private FB_tank_player m_TankPlayerData;


        protected override void Init()
        {
            player = this.gameObject;
            imgTankIcon = player.transform.GetChild(0).GetComponent<Transform>();

            InitAnim();

            ChangeTankType(TankTypeID);

            InitMove(new Vector2(player.transform.localPosition.x, player.transform.localPosition.z));
            InitFire();

        }

        private void FixedUpdate()
        {
            Move();

            Fire();

            if (IsMoving)
            {
                PauseAnim(false);
            }
            else
            {
                PauseAnim(true);
            }
        }

        private void ChangeTankType(int id)
        {
            if (DataTools.GetTankPlayer(TankTypeID).ByteBuffer == null)
            {
                Debugger.LogError($"没有该坦克数据 id:{id}");
                return;
            }
            TankTypeID = id;
            m_TankPlayerData = DataTools.GetTankPlayer(TankTypeID);
            UpdateBulletInterval();
            UpdateTankMoveSpeed();
            UpdateTankAnim();

        }

        #region Public

        public void AddLevel()
        {
            int id = TankTypeID + 1;
            ChangeTankType(id);
        }
        public void SubLevel()
        {
            int id = TankTypeID - 1;
            ChangeTankType(id);
        }
        #endregion
    }
}
