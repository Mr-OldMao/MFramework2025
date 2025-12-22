using GameMain.Generate.FlatBuffers;
using MFramework.Runtime;
using MFramework.Runtime.Extend;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace GameMain
{
    public abstract class TankEntityBase : MonoBehaviour
    {
        public TankOwnerType TankOwnerType { get; private set; }

        public SpriteRenderer NodeSpriteRenderer { get; private set; }

        public int TankTypeID { get; protected set; }
        public int HP { get; protected set; }
        public int EntityID { get; set; }

        public ETankState eTankState { get; protected set; }


        public void InitData(TankOwnerType tankOwnerType , int tankTypeID, int entityID)
        {
            TankOwnerType = tankOwnerType;
            TankTypeID = tankTypeID;
            EntityID = entityID;
            HP = tankOwnerType == TankOwnerType.Enemy ? DataTools.GetTankEnemy(tankTypeID).HP : DataTools.GetTankPlayer(tankTypeID).HP;
            NodeSpriteRenderer = transform.GetComponentInChildren<SpriteRenderer>();
            eTankState = ETankState.Idle;
            Init();
        }

        protected abstract void Init();

        public void TankBeAttacked(BulletEntity bulletEntity,Action<bool> tankDeadCallback)
        {
            if (bulletEntity?.BulletData != null)
            {
                HP -= bulletEntity.BulletData.BulletATK;
                if (HP <= 0)
                {
                    eTankState = ETankState.Dead;
                    OnTankDead();
                    GameEntry.Event.DispatchEvent(GameEventType.TankDead, EntityID);
                    tankDeadCallback?.Invoke(true);
                }
                else
                {
                    OnTankHit();
                    GameEntry.Event.DispatchEvent(GameEventType.TankHit, EntityID);
                    tankDeadCallback?.Invoke(false);
                }
            }
        }

        protected void UpdateTankData(int tankTypeID)
        {
            TankTypeID = tankTypeID;
            GameEntry.Resource.LoadAssetAsync<SpriteAtlas>(SystemConstantData.PATH_PREFAB_TEXTURE_ATLAS_ROOT + "enemyTankAtlas.spriteatlas",(p)=>
            {
                string spriteName = TankOwnerType == TankOwnerType.Enemy ? DataTools.GetTankEnemy(tankTypeID).ResName : DataTools.GetTankPlayer(tankTypeID).ResName;
                NodeSpriteRenderer.sprite = p.GetSprite(spriteName);
            },false);
        }

        protected abstract void OnTankDead();

        protected abstract void OnTankHit();
    }

    /// <summary>
    /// 坦克归属
    /// </summary>
    public enum TankOwnerType
    {
        Player1,
        Player2,
        Enemy
    }

    /// <summary>
    /// 坦克状态
    /// </summary>
    public enum ETankState
    {
        Idle,
        Attack,
        Move,
        Dead
    }

}
