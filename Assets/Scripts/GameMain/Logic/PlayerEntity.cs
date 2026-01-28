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
        /// 下次坦克生成是否初始化坦克数据
        /// </summary>
        public bool IsInitPlayerData = true;

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
            UpdatePlayerData();
            UpdatePlayerStageReward();
        }

        /// <summary>
        /// 更新玩家关卡奖励
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void UpdatePlayerStageReward()
        {
            //侧边栏奖励
            var sidebarData = GameMainLogic.Instance.GetUserDataSidebar();
            Debugger.Log($"侧边栏奖励 isGetTodayReward:{sidebarData.isGetTodayReward},rewardType:{sidebarData.rewardType},rewardValue:{sidebarData.rewardValue}");

            if (sidebarData.isGetTodayReward && IsInitPlayerData)
            {
                //1-第一关 关卡开始生命值+x
                //2-第一关 关卡开始坦克等级+x
                //3-每关开始获取随机道具
                bool isFirstStage = GameMainLogic.Instance.StageID == 1;
                
                switch (sidebarData.rewardType)
                {
                    case 1:
                        if (isFirstStage)
                        {
                            AddLife(sidebarData.rewardValue, true);
                        }
                        break;
                    case 2:
                        if (isFirstStage)
                        {
                            AddLevel(sidebarData.rewardValue, true);
                        }
                        break;
                    case 3:
                        GameEntry.Pool.GetPool(GameMainLogic.Instance.PoolIdReward).GetEntity();
                        break;
                }
            }
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
            var tankDatas = DataTools.GetTankPlayer();
            int maxLevelID = tankDatas[tankDatas.Count - 1].ID;
            int minLevelID = tankDatas[0].ID;
            if (id > maxLevelID)
            {
                id = maxLevelID;
            }
            else if (id < minLevelID)
            {
                id = minLevelID;
            }

            if (DataTools.GetTankPlayer(id).ByteBuffer == null)
            {
                Debugger.LogError($"没有该坦克数据 id:{id}");
               
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
            UpdatePlayerData();
        }

        public void RecycleTank()
        {
            entity.SetActive(false);
            if (GameMainLogic.Instance.GameStateType == GameStateType.GameRunning)
            {
                SubLife();
            }
            IsCanMove = false;
            m_IsCanFire = false;
        }

        public void AddLevel(int addNum = 1, bool isPlaySound = true)
        {
            int id = tankTypeID + addNum;
            if (isPlaySound)
            {
                GameEntry.Audio.PlaySound("prop_award.mp3");
            }
            ChangeTankType(id);
        }
        public void SubLevel(int subNum = 1)
        {
            int id = tankTypeID - subNum;
            ChangeTankType(id);
        }

        public void AddLife(int addNum = 1, bool isPlaySound = true)
        {
            remainLife += addNum;
            GameEntry.UI.GetView<UIPanelBattle>().RefreshUI();
            if (isPlaySound)
            {
                GameEntry.Audio.PlaySound("prop_addlife.mp3");
            }
        }

        public void UpdateHP()
        {
            HP = m_TankPlayerData.HP;
        }

        public void SubLife()
        {
            --remainLife;
        }

        public void UpdatePlayerData()
        {
            if (IsInitPlayerData)
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
            IsInitPlayerData = !isCanRevive;
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
