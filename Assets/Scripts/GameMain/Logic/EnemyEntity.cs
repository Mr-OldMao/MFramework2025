using MFramework.Runtime;

namespace GameMain
{
    public class EnemyEntity : TankEntityBase
    {
        private void Awake()
        {

        }

        protected override void Init()
        {

        }

        protected override void OnTankDead()
        {
            Debugger.Log($"OnTankDead, id:{EntityID}, {this.gameObject.name}");
            Destroy(this.gameObject);
        }

        protected override void OnTankHit()
        {
            Debugger.Log($"OnTankHit, id:{EntityID}, {this.gameObject.name} , remain HP:{HP}");

            int nextID = DataTools.GetTankEnemy(TankTypeID).NextID;
            UpdateTankData(nextID);
        }
    }
}
