using MFramework.Runtime;

namespace GameMain
{
    public partial class EnemyEntity
    {
        public override void OnTankDead(TankOwnerType killerTankOwnerType, bool isBombDead = false)
        {
            Debugger.Log($"OnTankDead, id:{EntityID}, {this.gameObject.name}");
            GameEntry.Pool.GetPool(GameMainLogic.Instance.PoolIdTankEnemy).RecycleEntity(gameObject);

            base.OnTankDead(killerTankOwnerType,isBombDead);
        }

        protected override void OnTankHit(int hitValue)
        {
            Debugger.Log($"OnTankHit, id:{EntityID}, {this.gameObject.name} , remain HP:{HP}");

            int nextID = DataTools.GetTankEnemy(tankTypeID).NextID;
            UpdateTankData(nextID);

            m_TankEnemyData = DataTools.GetTankEnemy(tankTypeID);
            moveSpeed = m_TankEnemyData.MoveSpeed;
            UpdateBulletInterval();
        }

    }
}
