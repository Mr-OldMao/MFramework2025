using GameMain.Generate.FlatBuffers;
using MFramework.Runtime;
using UnityEngine;

namespace GameMain
{

    public partial class EnemyEntity
    {
        public bool IsAutoFire;

        private Transform NodePosBullet;

        /// <summary>
        /// 当前坦克的发射子弹最短子弹间隔
        /// </summary>
        public float m_BulletInterval;
       

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
            m_BulletInterval = DataTools.GetBulletBullet(m_TankEnemyData.BulletID).BulletInterval;
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
            bulletEntity.Fire(NodePosBullet.position, MoveDirType, DataTools.GetTankEnemy(TankTypeID).BulletID, () =>
            {
                ResetFireState();
            });
        }

        private void SetNextFireBulletCountdown()
        {
            float randomNextFireInterval = Random.Range(m_TankEnemyData.AutoFireInterval(0), m_TankEnemyData.AutoFireInterval(1));
            m_NextFireBulletCountdown = randomNextFireInterval > m_BulletInterval ? randomNextFireInterval : m_BulletInterval;
        }

        private void ResetFireState()
        {
            /// <summary>
            /// 碰撞后子弹重置最短时间
            /// </summary>
            float m_ResetBulletMinTimer = m_BulletInterval / 2;
            if (!IsAutoFire && m_NextFireBulletCountdown < m_ResetBulletMinTimer)
            {
                m_NextFireBulletCountdown = m_ResetBulletMinTimer;
            }
        }
    }
}
