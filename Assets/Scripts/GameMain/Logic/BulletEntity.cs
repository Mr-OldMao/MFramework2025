using MFramework.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMain
{
    public partial class BulletEntity : MonoBehaviour
    {
        public bool isCanMove = false;

        public MoveDirType bulletDirType;
        public BulletOwnerType bulletOwnerType;
        public float bulletSpeed;
        public bool isPlayer;


        private int m_PoolID;
        private int m_MaxDis;

        private Vector3 m_bulletDir;

        private const float BulletSpeedConst = 3f;
        public void InitFireBullet(MoveDirType bulletDirType, BulletOwnerType bulletOwnerType, float bulletSpeed, int m_PoolID)
        {
            this.bulletDirType = bulletDirType;
            this.bulletOwnerType = bulletOwnerType;
            this.bulletSpeed = bulletSpeed * BulletSpeedConst;
            this.m_PoolID = m_PoolID;

            m_MaxDis = Mathf.Max(GameEntry.UI.GetModel<UIModelMap>().COLUMN_NUM, GameEntry.UI.GetModel<UIModelMap>().ROW_NUM) + 1;

            switch (bulletDirType)
            {
                case MoveDirType.Forward:
                    m_bulletDir = Vector3.forward;
                    break;
                case MoveDirType.Back:
                    m_bulletDir = Vector3.back;
                    break;
                case MoveDirType.Left:
                    m_bulletDir = Vector3.left;
                    break;
                case MoveDirType.Right:
                    m_bulletDir = Vector3.right;
                    break;
            }
        }

        public void Fire(Vector3 startPos)
        {
            transform.position = startPos;
            isCanMove = true;
        }

        void Update()
        {
            if (isCanMove)
            {
                Move();
            }
        }

        private void Move()
        {
            transform.Translate(m_bulletDir * Time.deltaTime * bulletSpeed);

            if (transform.localPosition.x < -1
                || transform.localPosition.z < -1
                || transform.localPosition.x > m_MaxDis
                || transform.localPosition.z > m_MaxDis)
            {
                HintEntity();
            }
        }

        private void HintEntity()
        {
            isCanMove = false;
            GameEntry.Pool.GetPool(m_PoolID).RecycleEntity(gameObject);
        }
    }

    /// <summary>
    /// 子弹方向
    /// </summary>
    public enum BulletDirType
    {
        Forward,
        Back,
        Left,
        Right
    }

    /// <summary>
    /// 子弹归属
    /// </summary>
    public enum BulletOwnerType
    {
        Player,
        Enemy
    }
}
