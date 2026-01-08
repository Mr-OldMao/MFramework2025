using GameMain.Generate.FlatBuffers;
using MFramework.Runtime;
using UnityEngine;

namespace GameMain
{
    public partial class EnemyEntity : TankEntityBase
    {
        private FB_tank_enemy m_TankEnemyData;

        protected override void InitBornBefore()
        {
            
        }

        public void RecycleTank()
        {
            //分数+1
            Debug.Log("EnemyTankDead--------------"+ entity);
            IsAutoFire = false;
            IsCanMove = false;
            MoveTargetType =  EAIMoveTargetType.None;
            entity.SetActive(false);
        }

        protected override void InitBornAfter()
        {
            MoveDirType = MoveDirType.Back;
            m_TankEnemyData = DataTools.GetTankEnemy(tankTypeID);

            InitAIMove();
            InitFire();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            AIMoveUpdate();
            AutoFireUpdate();
        }
    }
}
