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
        public override TankOwnerType TankOwnerType => TankOwnerType.Player1;

        private GameObject player;
        private Transform imgTankIcon;

        private MoveDirType moveDirType = MoveDirType.Forward;

        private FB_tank_player m_TankPlayerData;

        private void Awake()
        {
            player = this.gameObject;
            imgTankIcon = player.transform.GetChild(0).GetComponent<Transform>();
            Init();
        }

        private void Init()
        {
            InitAnim();

            ChangeTankType(ETankType.TankType1);

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

        private void ChangeTankType(ETankType eTankType)
        {
            TankType = eTankType;
            m_TankPlayerData = DataTools.GetTankPlayer((int)TankType);
            UpdateBulletInterval();
            UpdateTankMoveSpeed();
            UpdateTankAnim();

        }

        #region Public

        public void AddLevel()
        {
            int tankTypeInt = (int)TankType + 1;
            if (DataTools.GetTankPlayer(tankTypeInt).ByteBuffer != null)
            {
                ChangeTankType((ETankType)tankTypeInt);
            }
            else
            {
                Debug.Log("已经达到最高等级");
            }
        }
        public void SubLevel()
        {
            int tankTypeInt = (int)TankType - 1;
            if (DataTools.GetTankPlayer(tankTypeInt).ByteBuffer != null)
            {
                ChangeTankType((ETankType)tankTypeInt);
            }
            else
            {
                Debug.Log("已经达到最低等级");
            }
        }
        #endregion


        [ContextMenu("一星坦克")]
        public void SetBulletLevel1()
        {
            ChangeTankType(ETankType.TankType1);
        }
        [ContextMenu("二星坦克")]
        public void SetBulletLevel2()
        {
            ChangeTankType(ETankType.TankType2);
        }
        [ContextMenu("三星坦克")]
        public void SetBulletLevel3()
        {
            ChangeTankType(ETankType.TankType3);
        }
        [ContextMenu("四星坦克")]
        public void SetBulletLevel4()
        {
            ChangeTankType(ETankType.TankType4);
        }


    }
}
