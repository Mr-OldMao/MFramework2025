using MFramework.Runtime;

namespace GameMain
{
    public partial class EnemyEntity
    {
        public override void OnTankDead(TankOwnerType killerTankOwnerType, bool isBombDead = false)
        {
            Debugger.Log($"OnTankDead, id:{EntityID}, {this.gameObject.name}");
            TryGenerateProp();
            GameEntry.Pool.GetPool(GameMainLogic.Instance.PoolIdTankEnemy).RecycleEntity(gameObject);

            base.OnTankDead(killerTankOwnerType,isBombDead);
        }

        protected override void OnTankHit(int hitValue)
        {
            Debugger.Log($"OnTankHit, id:{EntityID}, {this.gameObject.name} , remain HP:{HP}");
            var tankData = DataTools.GetTankEnemy(tankTypeID);
            TryGenerateProp();

            int nextID = tankData.NextID;
            UpdateTankData(nextID);

            m_TankEnemyData = DataTools.GetTankEnemy(tankTypeID);
            moveSpeed = m_TankEnemyData.MoveSpeed;
            UpdateBulletInterval();

        }


        private void TryGenerateProp()
        {
            var tankData = DataTools.GetTankEnemy(tankTypeID);
            if (tankData.IsReward)
            {
                GameEntry.Pool.GetPool(GameMainLogic.Instance.PoolIdReward).GetEntity();
            }
        }
    }
}
