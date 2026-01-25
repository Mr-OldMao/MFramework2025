using Cysharp.Threading.Tasks;
using MFramework.Runtime;
using MFramework.Runtime.Extend;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using Random = UnityEngine.Random;

namespace GameMain
{
    public partial class GameMainLogic
    {
        public Transform RootNode { get; private set; }

        private Transform NodePool;
        private Transform NodePoolEff;
        private Transform NodePoolBulletPlayer1;
        private Transform NodePoolBulletEnemy;
        private Transform NodePoolTankEnemy;
        private Transform NodePoolPlayer1Enemy;
        private Transform NodePoolReward;

        private int m_PoolIdEffSmallBomb;
        private int m_PoolIdEffNormallBomb;
        private int m_PoolIdBulletPlayer1;
        private int m_PoolIdBulletEnemy;
        public int PoolIdTankEnemy { get; private set; }
        public int PoolIdTankPlayer { get; private set; }

        public int PoolIdReward { get; private set; }

        private int m_CurTankID;
        public async UniTask InitPool()
        {
            RootNode = InitNodePool("RootNode");
            NodePool = InitNodePool("NodePool", RootNode);
            NodePoolEff = InitNodePool("NodePoolEff");
            NodePoolBulletPlayer1 = InitNodePool("NodePoolBulletPlayer1");
            NodePoolBulletEnemy = InitNodePool("NodePoolBulletEnemy");
            NodePoolTankEnemy = InitNodePool("NodePoolTankEnemy");
            NodePoolPlayer1Enemy = InitNodePool("NodePoolPlayer1Enemy");
            NodePoolReward = InitNodePool("NodePoolReward");

            m_CurTankID = 0;
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
                //Debug.Log("回收子弹TODO " + go);
            }, 1, 50));

            m_PoolIdBulletEnemy = GameEntry.Pool.CreatPool(new Pool(bulletPrefab, (go, b) =>
            {
                if (b)
                {
                    go.transform.SetParent(NodePoolBulletEnemy);
                    go.transform.localPosition = Vector3.zero;
                    go.AddComponent<BulletEntity>();
                }
                go.GetComponent<BulletEntity>().InitFireBullet(TankOwnerType.Enemy, m_PoolIdBulletEnemy);
            }, (go) =>
            {
                //Debug.Log("回收子弹TODO " + go);
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
                        var enemyEntity = enemy.GetOrAddComponent<EnemyEntity>();
                        enemyEntity.InitRegisterEvents();
                    }
                    enemy.gameObject.SetActive(true);
                    bool isRedTank = Random.Range(0f, 1f) > 0.7f;
                    int tankTypeID = isRedTank ? Random.Range(301, 304) : Random.Range(201, 206);
                    string spriteName = DataTools.GetTankEnemy(tankTypeID).ResName;
                    enemy.GetComponentInChildren<SpriteRenderer>().sprite = enemyTankAtlas.GetSprite(spriteName);
                    enemy.GetOrAddComponent<EnemyEntity>().InitData(TankOwnerType.Enemy, tankTypeID, ++m_CurTankID);
                    enemy.name = "entmy" + m_CurTankID;
                    --RemainEnemyTankNum;
                    GameEntry.Event.DispatchEvent(GameEventType.EnemyTankGenerate, enemy);
                    Debugger.Log($"generate enemy tank {m_CurTankID}");
                },
                recycleObjCallback = (go) =>
                {
                    Debug.Log("回收坦克 " + go);
                    go.GetComponent<EnemyEntity>().RecycleTank();
                },
                preloadObjCallback = (go) =>
                {
                    go.transform.SetParent(NodePoolTankEnemy);
                    go.GetOrAddComponent<EnemyEntity>();
                },
                initCount = 1,
                maxCount = 50
            };
            PoolIdTankEnemy = GameEntry.Pool.CreatPool(new Pool(tankEnemyPoolDataInfo));

            var playerTankAtlas = await GameEntry.Resource.LoadAssetAsync<SpriteAtlas>(SystemConstantData.PATH_PREFAB_TEXTURE_ATLAS_ROOT + "playerTankAtlas.spriteatlas", false);
            var player1TankPrefab = await GameEntry.Resource.LoadAssetAsync<GameObject>(SystemConstantData.PATH_PREFAB_ENTITY_ROOT + "tank/Player1.prefab", false);
            PoolDataInfo tankPlayer1PoolDataInfo = new PoolDataInfo
            {
                templateObj = player1TankPrefab,
                getObjCallback = (playerObj, isNewObj) =>
                {
                    if (isNewObj)
                    {
                        playerObj.transform.SetParent(NodePoolPlayer1Enemy);
                        var playerEntity = playerObj.GetOrAddComponent<PlayerEntity>();
                        playerEntity.InitRegisterEvents();
                    }
                    playerObj.SetActive(true);

                    Player1Entity = playerObj.GetOrAddComponent<PlayerEntity>();
                    int tankTypeID = Player1Entity.IsExtendBeforeDataNextGenerate ? Player1Entity.TankTypeID : DataTools.GetConst("Player_Default_ID");
                    Player1Entity.InitData(TankOwnerType.Player1, tankTypeID, ++m_CurTankID);
                    playerObj.name = "Player1" + m_CurTankID;
                    Debugger.Log($"generate player tank {m_CurTankID}");
                },
                recycleObjCallback = (go) =>
                {
                    Debug.Log("回收坦克 " + go);
                    go.GetComponent<PlayerEntity>().RecycleTank();
                },
                preloadObjCallback = (go) =>
                {
                    go.transform.SetParent(NodePoolPlayer1Enemy);
                    go.GetOrAddComponent<PlayerEntity>();
                },
                initCount = 1,
                maxCount = 1
            };
            PoolIdTankPlayer = GameEntry.Pool.CreatPool(new Pool(tankPlayer1PoolDataInfo));


            var rewardAtlas = await GameEntry.Resource.LoadAssetAsync<SpriteAtlas>(SystemConstantData.PATH_PREFAB_TEXTURE_ATLAS_ROOT + "reward.spriteatlas", false);
            var rewardPrefab = await GameEntry.Resource.LoadAssetAsync<GameObject>(SystemConstantData.PATH_PREFAB_ENTITY_ROOT + "reward/Reward.prefab", false);
            PoolIdReward = GameEntry.Pool.CreatPool(new Pool(rewardPrefab, (go, b) =>
            {
                if (b)
                {
                    go.transform.SetParent(NodePoolReward);
                    //go.transform.localPosition = Vector3.zero;
                    go.GetOrAddComponent<RewardEntity>();
                }
                var rewardDatas = DataTools.GetRewardRewards();
                int id = Random.Range(rewardDatas[0].ID, rewardDatas[rewardDatas.Count - 1].ID + 1);
                go.GetComponent<RewardEntity>().Init(id);
                string spriteName = DataTools.GetRewardReward(id).Name;
                go.transform.Find<SpriteRenderer>("imgReward").sprite = rewardAtlas.GetSprite(spriteName);
                go.name = "reward" + id;
            }, (go) =>
            {
                go.GetComponent<RewardEntity>().ClreaTimer();
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
                    go = GameEntry.Pool.GetPool(m_PoolIdBulletEnemy).GetEntity();
                    break;
            }
            return go;
        }
    }
}
