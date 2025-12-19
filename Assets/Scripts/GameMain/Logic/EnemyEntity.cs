using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMain
{
    public class EnemyEntity : TankEntityBase
    {
        public override TankOwnerType TankOwnerType => TankOwnerType.Enemy;
    }
}
