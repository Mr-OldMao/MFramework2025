using MFramework.Runtime;

namespace GameMain
{
    public class EnemyEntity : TankEntityBase
    {
        private void Awake()
        {
            IsMoving = true;
        }

        protected override void Init()
        {

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
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
        }
    }
}
