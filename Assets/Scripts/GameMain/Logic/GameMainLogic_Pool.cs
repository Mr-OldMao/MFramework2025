using Cysharp.Threading.Tasks;
using MFramework.Runtime;
using MFramework.Runtime.Extend;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D;
using static UnityEngine.EventSystems.EventTrigger;
using Random = UnityEngine.Random;

namespace GameMain
{


    public partial class GameMainLogic
    {
        public Transform RootNode { get; private set; }

        private Transform NodePool;
        private Transform NodePoolEff;
        private Transform NodePoolBulletPlayer1;
        private Transform NodePoolTankEnemy;

        private int m_PoolIdEffSmallBomb;
        private int m_PoolIdEffNormallBomb;
        private int m_PoolIdBulletPlayer1;
        public int PoolIdTankEnemy { get; private set; }


        private int m_CurEnemyEntityID;
        public async UniTask Init()
        {
            RootNode = InitNodePool("RootNode");

            NodePool = InitNodePool("NodePool", RootNode);

            NodePoolEff = InitNodePool("NodePoolEff");

            NodePoolBulletPlayer1 = InitNodePool("NodePoolBulletPlayer1");

            NodePoolTankEnemy = InitNodePool("NodePoolTankEnemy");

            m_CurEnemyEntityID = 0;
            await GeneratePool();
        }

        private Transform InitNodePool(string nodePoolName, Transform parentNode = null)
        {
            Transform nodePool = GameObject.Find(nodePoolName)?.transform;
            if (nodePool == null)
            {
                nodePool = new GameObject(nodePoolName).transform;
            }
            if (parentNode == null && NodePool != null)
            {
                parentNode = NodePool;
            }
            nodePool.SetParent(parentNode);
            return nodePool;
        }

        private async UniTask GeneratePool()
        {
            GameObject goEffSmallBomb = await GameEntry.Resource.LoadAssetAsync<GameObject>(SystemConstantData.PATH_PREFAB_ENTITY_EFF_ROOT + "EffSmallBomb.prefab", false);
            m_PoolIdEffSmallBomb = GameEntry.Pool.CreatPool(new Pool(goEffSmallBomb, (go, b) =>
            {
                if (b)
                {
                    //go.SetActive(false);
                    go.transform.SetParent(NodePoolEff);
                }
                GameEntry.Timer.AddDelayTimer(0.1f, () =>
                {
                    GameEntry.Pool.GetPool(m_PoolIdEffSmallBomb).RecycleEntity(go);
                });
            }, (go) =>
            {

            }, 1));

            GameObject goEffNormalBomb = await GameEntry.Resource.LoadAssetAsync<GameObject>(SystemConstantData.PATH_PREFAB_ENTITY_EFF_ROOT + "EffNormalBomb.prefab", false);
            m_PoolIdEffNormallBomb = GameEntry.Pool.CreatPool(new Pool(goEffNormalBomb, (go, b) =>
            {
                if (b)
                {
                    //go.SetActive(false);
                    go.transform.SetParent(NodePoolEff);
                }
                GameEntry.Timer.AddDelayTimer(0.2f, () =>
                {
                    GameEntry.Pool.GetPool(m_PoolIdEffNormallBomb).RecycleEntity(go);
                });
            }, (go) =>
            {

            }, 1));

            var bulletPrefab = await GameEntry.Resource.LoadAssetAsync<GameObject>(SystemConstantData.PATH_PREFAB_ENTITY_ROOT + "map/2d/Bullet.prefab", false);
            bulletPrefab.gameObject.name = "123";
            m_PoolIdBulletPlayer1 = GameEntry.Pool.CreatPool(new Pool(bulletPrefab, (go, b) =>
            {
                if (b)
                {
                    go.transform.SetParent(NodePoolBulletPlayer1);
                    go.transform.localPosition = Vector3.zero;
                    go.AddComponent<BulletEntity>();
                }
                go.GetComponent<BulletEntity>().InitFireBullet(TankOwnerType.Player1, m_PoolIdBulletPlayer1);
            }, (go) =>
            {
                Debug.Log("回收子弹TODO " + go);
            }, 1, 50));


            var enemyTankAtlas = await GameEntry.Resource.LoadAssetAsync<SpriteAtlas>(SystemConstantData.PATH_PREFAB_TEXTURE_ATLAS_ROOT + "enemyTankAtlas.spriteatlas", false);
            var entmyTankPrefab = await GameEntry.Resource.LoadAssetAsync<GameObject>(SystemConstantData.PATH_PREFAB_ENTITY_ROOT + "tank/Enemy.prefab", false);
            PoolDataInfo tankEnemyPoolDataInfo = new PoolDataInfo
            {
                templateObj = entmyTankPrefab,
                getObjCallback = (enemy, b) =>
                {
                    if (b)
                    {
                        enemy.transform.SetParent(NodePoolTankEnemy);
                        enemy.GetOrAddComponent<EnemyEntity>();
                    }
                    enemy.gameObject.SetActive(true);
                    bool isRedTank = Random.Range(0f, 1f) > 0.7f;
                    int tankTypeID = isRedTank ? Random.Range(301, 304) : Random.Range(201, 206);
                    string spriteName = DataTools.GetTankEnemy(tankTypeID).ResName;
                    enemy.GetComponentInChildren<SpriteRenderer>().sprite = enemyTankAtlas.GetSprite(spriteName);
                    Vector2 posBornEnemy = GameEntry.UI.GetModel<UIModelMap>().GetRandomGridPosBornEnemy();
                    enemy.transform.localPosition = new Vector3(posBornEnemy.x, 0, posBornEnemy.y);
                    enemy.GetComponent<EnemyEntity>().InitData(TankOwnerType.Enemy, tankTypeID, ++m_CurEnemyEntityID);
                    enemy.name = "entmy" + m_CurEnemyEntityID;
                    Debugger.Log($"generate enemy tank {m_CurEnemyEntityID}");
                },
                recycleObjCallback = (go) => Debug.Log("回收坦克 " + go),
                preloadObjCallback = (go) =>
                {
                    go.transform.SetParent(NodePoolTankEnemy);
                    go.GetOrAddComponent<EnemyEntity>();
                },
                initCount = 1,
                maxCount = 50
            };
            PoolIdTankEnemy = GameEntry.Pool.CreatPool(new Pool(tankEnemyPoolDataInfo));
        }



        public GameObject GetPoolEffSmallBomb()
        {
            return GameEntry.Pool.GetPool(m_PoolIdEffSmallBomb).GetEntity();
        }

        public GameObject GetPoolEffNormalBomb()
        {
            return GameEntry.Pool.GetPool(m_PoolIdEffNormallBomb).GetEntity();
        }

        public GameObject GetPoolBullet(TankOwnerType tankOwnerType)
        {
            GameObject go = null;
            switch (tankOwnerType)
            {
                case TankOwnerType.Player1:
                    go = GameEntry.Pool.GetPool(m_PoolIdBulletPlayer1).GetEntity();
                    break;
                case TankOwnerType.Player2:
                    break;
                case TankOwnerType.Enemy:
                    break;
            }
            return go;
        }

        public GameObject GetPoolTankEnemy()
        {
            return GameEntry.Pool.GetPool(PoolIdTankEnemy).GetEntity();
        }
    }
}
