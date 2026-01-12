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

        public void InitRegisterEvents()
        {
            GameEntry.Event.RegisterEvent(GameEventType.GameWin, () =>
            {
                GameWinEvent();
            });
            GameEntry.Event.RegisterEvent(GameEventType.GameFail, () =>
            {
                GameFailEvent();
            });
            GameEntry.Event.RegisterEvent<TankUnbeatableInfo>(GameEventType.TankUnbeatable, (p) =>
            {
                OnTankUnbeatable(p);
            });
        }

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
            GameEntry.Audio.PlaySound("prop_award.ogg");
            ChangeTankType(id);
        }
        public void SubLevel(int subNum = 1)
        {
            int id = tankTypeID - subNum;
            ChangeTankType(id);
        }

        public void AddLife(int addNum = 1)
        {
            ++remainLife;
            GameEntry.UI.GetView<UIPanelBattle>().RefreshUI();
            GameEntry.Audio.PlaySound("prop_addlife.mp3");
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

            if (remainLife < 0)
            {
                if (GameMainLogic.Instance.GameStateType != GameStateType.GameFail)
                {
                    GameMainLogic.Instance.GameStateType = GameStateType.GameFail;
                }
            }
        }
        #endregion
    }
}
