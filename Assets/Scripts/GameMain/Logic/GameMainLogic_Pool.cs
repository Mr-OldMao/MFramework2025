using Cysharp.Threading.Tasks;
using MFramework.Runtime;
using MFramework.Runtime.Extend;
using System.Collections.Generic;
using UnityEngine;

namespace GameMain
{


    public partial class GameMainLogic
    {
        public Transform RootNode { get; private set; }

        private Transform NodePool;
        private Transform NodePoolEff;
        private Transform NodePoolBulletPlayer1;

        private int m_PoolIdEffSmallBomb;
        private int m_PoolIdEffNormallBomb;
        private int m_PoolIdBulletPlayer1;



        public async UniTask Init()
        {
            RootNode = GameObject.Find("RootNode")?.transform;
            if (RootNode == null)
            {
                RootNode = new GameObject("RootNode").transform;
            }

            NodePool = GameObject.Find("NodePool")?.transform;
            if (NodePool == null)
            {
                NodePool = new GameObject("NodePool").transform;
            }
            NodePool.transform.SetParent(RootNode);

            NodePoolEff = GameObject.Find("NodePoolEff")?.transform;
            if (NodePoolEff == null)
            {
                NodePoolEff = new GameObject("NodePoolEff").transform;
            }
            NodePoolEff.SetParent(NodePool);

            NodePoolBulletPlayer1 = GameObject.Find("NodePoolBulletPlayer1")?.transform;
            if (NodePoolBulletPlayer1 == null)
            {
                NodePoolBulletPlayer1 = new GameObject("NodePoolBulletPlayer1").transform;
            }
            NodePoolBulletPlayer1.SetParent(NodePool);

            await GeneratePool();
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
    }
}
