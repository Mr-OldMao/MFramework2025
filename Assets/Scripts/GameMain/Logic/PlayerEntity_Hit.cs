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
            Debugger.Log($"OnTankDead, id:{EntityID}, {this.gameObject.name}");
        }

        protected override void OnTankHit()
        {
            Debugger.Log($"OnTankHit, id:{EntityID}, {this.gameObject.name}");
        }
    }
}
