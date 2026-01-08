using MFramework.Runtime;

namespace GameMain
{
    public partial class EnemyEntity
    {
        protected override void OnTankDead()
        {
            Debugger.Log($"OnTankDead, id:{EntityID}, {this.gameObject.name}");

            GameEntry.Pool.GetPool(GameMainLogic.Instance.PoolIdTankEnemy).RecycleEntity(gameObject);
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
