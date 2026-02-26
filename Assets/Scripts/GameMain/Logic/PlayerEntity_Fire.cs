using Cysharp.Threading.Tasks;
using GameMain.Generate.FlatBuffers;
using MFramework.Runtime;
using MFramework.Runtime.Extend;
using UnityEngine;
using static GameMain.PlayerEntity;

namespace GameMain
{
    public partial class PlayerEntity
    {
        private Transform NodePosBullet;

        public float m_BulletInterval;
        private bool m_IsCanFire;
        private bool m_IsFiring;
        private float m_NextFireBulletCountdown;

        private EBulletStateType m_BulletStateType;

        private FB_bullet_bullet m_BulletData;

        public void InitFire()
        {
            NodePosBullet = transform.Find<Transform>("NodePosBullet");
            m_IsCanFire = true;
            m_IsFiring = false;
            m_BulletData = DataTools.GetBulletBullet(m_TankPlayerData.BulletID);
            m_NextFireBulletCountdown = m_BulletData.BulletInterval;
            m_BulletStateType = EBulletStateType.BulletCDEnd;
        }

        private void UpdateBulletInterval()
        {
            m_BulletData = DataTools.GetBulletBullet(m_TankPlayerData.BulletID);
            m_BulletInterval = DataTools.GetBulletBullet(m_TankPlayerData.BulletID).BulletInterval;
        }

        public void FireByKeyCode()
        {
            if (m_BulletStateType == EBulletStateType.FireBulletEnd
               || m_BulletStateType == EBulletStateType.BulletCD)
            {
                m_BulletStateType = EBulletStateType.BulletCD;

                m_NextFireBulletCountdown -= Time.deltaTime;
                if (m_NextFireBulletCountdown <= 0)
                {
                    m_BulletStateType = EBulletStateType.BulletCDEnd;
                    m_NextFireBulletCountdown = m_BulletData.BulletInterval;
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

        private async void Fire()
        {
            if (m_IsCanFire && m_BulletStateType == EBulletStateType.BulletCDEnd)
            {
                m_BulletStateType = EBulletStateType.FireBulletStart;



                if (!m_BulletData.IsCanFireDouble)
                {
                    m_BulletStateType = EBulletStateType.FireBulleting;
                    BulletEntity bulletEntity = GameMainLogic.Instance.GetPoolBullet(TankOwnerType).GetComponent<BulletEntity>();
                    bulletEntity.Fire(NodePosBullet.position, MoveDirType, DataTools.GetTankPlayer(tankTypeID).BulletID, true, () =>
                    {
                        ResetFireState();
                    });
                    GameEntry.Audio.PlaySound("fire.mp3");
                    m_BulletStateType = EBulletStateType.FireBulletEnd;
                }
                else
                {
                    int bulletID = DataTools.GetTankPlayer(tankTypeID).BulletID;

                    m_BulletStateType = EBulletStateType.FireBulleting;
                    BulletEntity bulletEntity1 = GameMainLogic.Instance.GetPoolBullet(TankOwnerType).GetComponent<BulletEntity>();
                    bulletEntity1.Fire(NodePosBullet.position, MoveDirType, bulletID, true, null);
                    GameEntry.Audio.PlaySound("fire.mp3");

                    //GameEntry.Timer.AddDelayTimer(0.2f, () =>
                    //{
                    await UniTask.Delay(100);
                    BulletEntity bulletEntity2 = GameMainLogic.Instance.GetPoolBullet(TankOwnerType).GetComponent<BulletEntity>();
                    bulletEntity2.Fire(NodePosBullet.position, MoveDirType, bulletID, true, () =>
                    {
                        ResetFireState();
                    });
                    GameEntry.Audio.PlaySound("fire.mp3");
                    m_BulletStateType = EBulletStateType.FireBulletEnd;
                    //});
                }
            }
        }

        private void ResetFireState()
        {
            /// <summary>
            /// 碰撞后子弹重置最短时间
            /// </summary>
            //float m_ResetBulletMinTimer = m_BulletInterval / 2;
            //if (!m_IsFiring && m_CurBulletTimer < m_ResetBulletMinTimer)
            //{
            //    m_CurBulletTimer = m_ResetBulletMinTimer;
            //}

            m_NextFireBulletCountdown = m_BulletData.BulletIntervalMin;
        }


        public enum EBulletStateType
        {
            FireBulletStart,
            FireBulleting,
            FireBulletEnd,
            BulletCD,
            BulletCDEnd,
        }
    }
}
