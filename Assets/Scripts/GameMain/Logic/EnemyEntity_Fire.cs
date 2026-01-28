using GameMain.Generate.FlatBuffers;
using MFramework.Runtime;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameMain
{

    public partial class EnemyEntity
    {
        public bool IsAutoFire;

        private Transform NodePosBullet;

        public FB_bullet_bullet m_BulletData;


        /// <summary>
        /// 下次开火倒计时
        /// </summary>
        private float m_NextFireBulletCountdown;

        private bool m_IsCanFire;
        public void InitFire()
        {
            NodePosBullet = transform.Find<Transform>("NodePosBullet");
            IsAutoFire = true;
            m_IsCanFire = false;
            SetNextFireBulletCountdown();
            UpdateBulletInterval();
        }

        private void UpdateBulletInterval()
        {
            m_BulletData = DataTools.GetBulletBullet(m_TankEnemyData.BulletID);
        }

        private void AutoFireUpdate()
        {
            if (eTankState != ETankState.Born && eTankState != ETankState.Dead)
            {
                if (IsAutoFire)
                {
                    m_NextFireBulletCountdown -= Time.deltaTime;
                    if (m_NextFireBulletCountdown <= 0)
                    {
                        m_IsCanFire = true;
                        SetNextFireBulletCountdown();
                    }
                }

                if (m_IsCanFire)
                {
                    m_IsCanFire = false;
                    Fire();
                }
            }
        }

        private void Fire()
        {
            BulletEntity bulletEntity = GameMainLogic.Instance.GetPoolBullet(TankOwnerType).GetComponent<BulletEntity>();
            eTankState = ETankState.Attack;
            bulletEntity.Fire(NodePosBullet.position, MoveDirType, DataTools.GetTankEnemy(tankTypeID).BulletID, () =>
            {
                ResetFireState();
            });
            //GameEntry.Audio.PlaySound("fire.ogg");
        }

        private void SetNextFireBulletCountdown()
        {
            float randomNextFireInterval = Random.Range(m_TankEnemyData.AutoFireInterval(0), m_TankEnemyData.AutoFireInterval(1));
            //m_NextFireBulletCountdown = Math.Max(randomNextFireInterval, m_BulletInterval);
            m_NextFireBulletCountdown = randomNextFireInterval;
        }

        private void ResetFireState()
        {
            /// <summary>
            /// 碰撞后子弹重置最短时间
            /// </summary>
            SetNextFireBulletCountdown();
        }
    }
}
