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
            MoveDirType = MoveDirType.Back;
            m_TankEnemyData = DataTools.GetTankEnemy(TankTypeID);

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
