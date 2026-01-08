using MFramework.Runtime;
using UnityEngine;

namespace GameMain
{
    public partial class PlayerEntity
    {
        private Transform NodePosBullet;

        public float m_BulletInterval;
        private bool m_IsCanFire;
        private bool m_IsFiring;
        private float m_CurBulletTimer;


        public void InitFire()
        {
            NodePosBullet = transform.Find<Transform>("NodePosBullet");
            m_IsCanFire = true;
            m_IsFiring = false;
            m_CurBulletTimer = 0;
        }

        private void UpdateBulletInterval()
        {
            m_BulletInterval = DataTools.GetBulletBullet(m_TankPlayerData.BulletID).BulletInterval;
        }

        public void FireByKeyCode()
        {
            if (!m_IsFiring)
            {
                m_CurBulletTimer += Time.deltaTime;
                if (m_CurBulletTimer >= m_BulletInterval)
                {
                    m_IsFiring = true;
                    m_CurBulletTimer = 0;
                }
            }

            if (Input.GetKey(KeyCode.Space))
            {
                Fire();
            }
        }

        public void FireByTouch()
        {
            Fire();
        }

        private void Fire()
        {
            if (m_IsCanFire && m_IsFiring)
            {
                BulletEntity bulletEntity = GameMainLogic.Instance.GetPoolBullet(TankOwnerType).GetComponent<BulletEntity>();
                bulletEntity.Fire(NodePosBullet.position, MoveDirType, DataTools.GetTankPlayer(tankTypeID).BulletID, () =>
                {
                    ResetFireState();
                });
                m_IsFiring = false;
            }
        }

        private void ResetFireState()
        {
            /// <summary>
            /// 碰撞后子弹重置最短时间
            /// </summary>
            float m_ResetBulletMinTimer = m_BulletInterval / 2;
            if (!m_IsFiring && m_CurBulletTimer < m_ResetBulletMinTimer)
            {
                m_CurBulletTimer = m_ResetBulletMinTimer;
            }
        }
    }
}
