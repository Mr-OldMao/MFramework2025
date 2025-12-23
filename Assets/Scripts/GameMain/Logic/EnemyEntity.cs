using GameMain.Generate.FlatBuffers;
using MFramework.Runtime;
using UnityEngine;

namespace GameMain
{
    public partial class EnemyEntity : TankEntityBase
    {
        public GameObject enemy;

        private FB_tank_enemy m_TankEnemyData;


        private void Awake()
        {
            enemy = gameObject;
        }

        protected override void Init()
        {
            m_TankEnemyData = DataTools.GetTankEnemy(TankTypeID);

            InitAIMove();
        }

        protected override void OnTankDead()
        {
            Debugger.Log($"OnTankDead, id:{EntityID}, {this.gameObject.name}");

            GameEntry.Pool.GetPool(GameMainLogic.Instance.PoolIdTankEnemy).RecycleEntity(gameObject);
        }

        protected override void OnTankHit()
        {
            Debugger.Log($"OnTankHit, id:{EntityID}, {this.gameObject.name} , remain HP:{HP}");

            int nextID = DataTools.GetTankEnemy(TankTypeID).NextID;
            UpdateTankData(nextID);

            m_TankEnemyData = DataTools.GetTankEnemy(TankTypeID);
            moveSpeed = m_TankEnemyData.MoveSpeed;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            AIMoveUpdate();
        }
    }
}
