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


        private FB_tank_player m_TankPlayerData;


        protected override void Init()
        {
            player = this.gameObject;
            imgTankIcon = player.transform.GetChild(0).GetComponent<Transform>();
            MoveDirType = MoveDirType.Forward;
            //InitAnim();

            ChangeTankType(TankTypeID);

            InitMove(new Vector2(player.transform.localPosition.x, player.transform.localPosition.z));
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

        public void AddLevel(int addNum =1)
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

            HP = m_TankPlayerData.HP;
            player.transform.position = new Vector3(GameEntry.UI.GetModel<UIModelMap>().GridPosBornPlayer1.x, 0, GameEntry.UI.GetModel<UIModelMap>().GridPosBornPlayer1.y);
            player.SetActive(true);
        }

        public void Dead()
        {
            player.SetActive(false);
        }
        #endregion
    }
}
