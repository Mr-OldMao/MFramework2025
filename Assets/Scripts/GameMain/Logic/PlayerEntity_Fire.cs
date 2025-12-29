using MFramework.Runtime;
using UnityEngine;

namespace GameMain
{
    public partial class PlayerEntity
    {
        private Transform NodePosBullet;

        public float m_BulletInterval;
        private bool m_IsCanFire;
        private float m_CurBulletTimer;


        public void InitFire()
        {
            NodePosBullet = transform.Find<Transform>("NodePosBullet");
            m_IsCanFire = true;
            m_CurBulletTimer = 0;
        }

        private void UpdateBulletInterval()
        {
            m_BulletInterval = DataTools.GetBulletBullet(m_TankPlayerData.BulletID).BulletInterval;
        }

        public void FireByKeyCode()
        {
            if (!m_IsCanFire)
            {
                m_CurBulletTimer += Time.deltaTime;
                if (m_CurBulletTimer >= m_BulletInterval)
                {
                    m_IsCanFire = true;
                    m_CurBulletTimer = 0;
                }
            }

            if (Input.GetKey(KeyCode.Space))
            {
                Fire();
            }
        }

        public void FireByTouch( )
        {
            Fire();
        }

        private void Fire()
        {
            if (m_IsCanFire)
            {
                BulletEntity bulletEntity = GameMainLogic.Instance.GetPoolBullet(TankOwnerType).GetComponent<BulletEntity>();
                bulletEntity.Fire(NodePosBullet.position, MoveDirType, DataTools.GetTankPlayer(TankTypeID).BulletID, () =>
                {
                    ResetFireState();
                });
                m_IsCanFire = false;
            }
        }

        private void ResetFireState()
        {
            /// <summary>
            /// 碰撞后子弹重置最短时间
            /// </summary>
            float m_ResetBulletMinTimer = m_BulletInterval / 2;
            if (!m_IsCanFire && m_CurBulletTimer < m_ResetBulletMinTimer)
            {
                m_CurBulletTimer = m_ResetBulletMinTimer;
            }
        }
    }
}
