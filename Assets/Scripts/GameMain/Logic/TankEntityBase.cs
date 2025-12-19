using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMain
{
    public abstract class TankEntityBase : MonoBehaviour
    {
        public abstract TankOwnerType TankOwnerType { get; }
        public ETankType TankType { get; protected set; }
    }

    public enum TankOwnerType
    {
        Player1,
        Player2,
        Enemy
    }

    /// <summary>
    /// 坦克类型
    /// </summary>
    public enum ETankType
    {
        TankType1 = 101,
        TankType2 = 102,
        TankType3 = 103,
        TankType4 = 104,
    }
}
