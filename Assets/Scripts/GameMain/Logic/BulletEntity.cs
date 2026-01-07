using Cysharp.Threading.Tasks;
using GameMain.Generate.FlatBuffers;
using MFramework.Runtime;
using System;
using UnityEngine;

namespace GameMain
{
    public partial class BulletEntity : MonoBehaviour
    {
        public bool isCanMove = false;

        public MoveDirType bulletDirType;
        public TankOwnerType tankOwnerType;
        public float bulletSpeed;
        public bool isPlayer;


        private int m_PoolID;
        private int m_MaxDis;

        private Vector3 m_bulletDir;

        private const float BulletSpeedConst = 3f;

        private Action collCallback;
        public FB_bullet_bullet BulletData { get; private set; }
        public void InitFireBullet(TankOwnerType tankOwnerType, int m_PoolID)
        {
            this.tankOwnerType = tankOwnerType;
            this.m_PoolID = m_PoolID;

            m_MaxDis = Mathf.Max(GameEntry.UI.GetModel<UIModelMap>().COLUMN_NUM, GameEntry.UI.GetModel<UIModelMap>().ROW_NUM) + 1;
        }

        public void Fire(Vector3 startPos, MoveDirType bulletDirType, int bulletID, Action collCallback)
        {
            transform.position = startPos;
            BulletData = DataTools.GetBulletBullet(bulletID);
            bulletSpeed = BulletData.BulletSpeed * BulletSpeedConst;
            isCanMove = true;
            this.collCallback = collCallback;
            this.bulletDirType = bulletDirType;
            switch (bulletDirType)
            {
                case MoveDirType.Forward:
                    m_bulletDir = Vector3.forward;
                    transform.localRotation = Quaternion.Euler(0, 0, 0);
                    break;
                case MoveDirType.Back:
                    m_bulletDir = Vector3.forward;
                    transform.localRotation = Quaternion.Euler(0, 180, 0);
                    break;
                case MoveDirType.Left:
                    m_bulletDir = Vector3.forward;
                    transform.localRotation = Quaternion.Euler(0, 270, 0);
                    break;
                case MoveDirType.Right:
                    m_bulletDir = Vector3.forward;
                    transform.localRotation = Quaternion.Euler(0, 90, 0);
                    break;
            }
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
                HintSelf(BulletCollisionType.None);
            }
        }

        private void HintSelf(BulletCollisionType bulletCollisionType, Vector3? hitPos = null)
        {
            if (!isCanMove)
            {
                return;
            }
            isCanMove = false;
            GameEntry.Pool.GetPool(m_PoolID).RecycleEntity(gameObject);
            if (hitPos == null)
            {
                hitPos = transform.position;
            }
            GenerateEffNormalBomb((Vector3)hitPos, bulletCollisionType);
        }


        public async UniTask OnTriggerEnter(Collider other)
        {
            //Debug.Log($"OnTriggerEnter : {other.name}");

            BulletEntity bulletEntity = other.GetComponent<BulletEntity>();
            if (bulletEntity != null && bulletEntity.tankOwnerType != tankOwnerType)
            {
                HintSelf(BulletCollisionType.None);
                //Debug.Log($"当前子弹 :{gameObject}, 敌方子弹：{other.name}");
                return;
            }


            TankEntityBase tankEntityBase = other.GetComponent<TankEntityBase>();
            if (tankEntityBase != null)
            {
                if (tankEntityBase.TankOwnerType == this.tankOwnerType)
                {
                    return;
                }
                else
                {
                    //Debug.Log($"当前子弹 :{tankOwnerType},击中坦克：{tankEntityBase.TankOwnerType},{other.name}");
                    tankEntityBase.TankBeHit(this, (isDead) =>
                    {
                        if (isDead)
                        {
                            HintSelf(BulletCollisionType.TankDead, tankEntityBase.RectAnimTank.transform.position);
                        }
                        else
                        {
                            HintSelf(BulletCollisionType.TankHit);
                        }
                    });
                }
            }

            MapEntity mapEntity = other.GetComponentInParent<MapEntity>();
            if (mapEntity != null)
            {
                switch (mapEntity.mapEntityType)
                {
                    case EMapEntityType.None:
                    case EMapEntityType.Grass:
                    case EMapEntityType.Water:
                    case EMapEntityType.Snow:

                        break;
                    case EMapEntityType.Wall:
                    case EMapEntityType.Wall_LU:
                    case EMapEntityType.Wall_LD:
                    case EMapEntityType.Wall_RU:
                    case EMapEntityType.Wall_RD:
                    case EMapEntityType.Stone:
                    case EMapEntityType.Stone_LU:
                    case EMapEntityType.Stone_LD:
                    case EMapEntityType.Stone_RU:
                    case EMapEntityType.Stone_RD:
                    case EMapEntityType.AirBorder:
                    case EMapEntityType.Brid:
                    case EMapEntityType.DeadBrid:
                        mapEntity.BulletCollEvent(this, other.gameObject);

                        //等待一帧
                        await UniTask.Delay(1);
                        HintSelf(BulletCollisionType.Map);
                        collCallback?.Invoke();
                        break;
                }
            }
        }

        private void GenerateEffNormalBomb(Vector3 pos, BulletCollisionType bulletCollisionType)
        {
            GameObject go = null;

            switch (bulletCollisionType)
            {
                case BulletCollisionType.None:
                    break;
                case BulletCollisionType.TankHit:
                    go = GameMainLogic.Instance.GetPoolEffSmallBomb();
                    break;
                case BulletCollisionType.TankDead:
                    go = GameMainLogic.Instance.GetPoolEffNormalBomb();
                    break;
                case BulletCollisionType.Map:
                    go = GameMainLogic.Instance.GetPoolEffSmallBomb();
                    break;
            }
            if (go != null)
            {
                go.transform.position = pos;
                go.SetActive(true);
            }
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

    /// <summary>
    /// 子弹碰撞效果类型
    /// </summary>
    public enum BulletCollisionType
    {
        None,
        TankHit,
        TankDead,
        Map
    }
}
