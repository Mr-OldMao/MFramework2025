using MFramework.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMain
{
    public partial class PlayerEntity
    {
        protected override void OnTankDead()
        {
            Debugger.Log($"PlayerEntity_Hit OnTankDead, id:{EntityID}, {this.gameObject.name}");

            TryRevive();
        }

        protected override void OnTankHit(int hitValue)
        {
            Debugger.Log($"PlayerEntity_Hit OnTankHit, id:{EntityID}, {this.gameObject.name}");

            SubLevel(hitValue);
            
        }
    }
}
