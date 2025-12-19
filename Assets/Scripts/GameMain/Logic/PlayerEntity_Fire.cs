using Cysharp.Threading.Tasks;
using MFramework.Runtime;
using System.Collections.Generic;
using UnityEngine;

namespace GameMain
{
    public partial class PlayerEntity
    {
        public GameObject BulletContainer;
        public Transform NodePosBullet;

        private List<GameObject> m_ListBullet = new List<GameObject>();
        private int m_PoolID;
        public float m_BulletInterval;
        private bool m_IsCanFire;
        private float m_CurBulletTimer;


        public async UniTask InitFire()
        {
            BulletContainer = GameObject.Find("BulletContainer");
            if (BulletContainer == null)
            {
                BulletContainer = new GameObject("BulletContainer");
            }

            NodePosBullet = transform.Find("spriteRenderer/NodePosBullet");

            var assets = await GameEntry.Resource.PreloadAssetsAsync<GameObject>(new List<string>
            {
                "Assets/Download/prefab/entity/map/3d/Bullet.prefab"
            }, false);


            m_IsCanFire = true;
            m_CurBulletTimer = 0;
            //UpdateBulletInterval();

            m_PoolID = GameEntry.Pool.CreatPool(new Pool(assets[0], (go, b) =>
            {
                if (b)
                {
                    go.transform.SetParent(BulletContainer.transform);
                    go.transform.localPosition = Vector3.zero;
                    go.AddComponent<BulletEntity>();
                    m_ListBullet.Add(go);
                }
                go.GetComponent<BulletEntity>().InitFireBullet(TankOwnerType, m_PoolID);
            }, (go) =>
            {
                Debug.Log("回收 " + go);
            }, 10, 50));
        }

        private void UpdateBulletInterval()
        {
            m_BulletInterval = DataTools.GetBulletBullet(m_TankPlayerData.BulletID).BulletInterval;
        }

        public void Fire()
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

            if (m_IsCanFire && Input.GetKey(KeyCode.Space))
            {
                BulletEntity bulletEntity = GameEntry.Pool.GetPool(m_PoolID).GetEntity().GetComponent<BulletEntity>();
                bulletEntity.Fire(NodePosBullet.position, moveDirType, DataTools.GetTankPlayer((int)TankType).BulletID, () =>
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
