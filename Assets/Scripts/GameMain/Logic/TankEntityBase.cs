using MFramework.Runtime;
using System;
using UnityEngine;

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

        private Animator _animator;
        public bool IsMoving { get; protected set; } = false;
        public bool IsCanMove { get; set; } = true;
        protected Rigidbody m_Rigidbody;


        public void InitData(TankOwnerType tankOwnerType, int tankTypeID, int entityID)
        {
            TankOwnerType = tankOwnerType;
            TankTypeID = tankTypeID;
            EntityID = entityID;
            HP = tankOwnerType == TankOwnerType.Enemy ? DataTools.GetTankEnemy(tankTypeID).HP : DataTools.GetTankPlayer(tankTypeID).HP;
            NodeSpriteRenderer = transform.GetComponentInChildren<SpriteRenderer>();
            _animator = GetComponentInChildren<Animator>();
            m_Rigidbody = GetComponentInChildren<Rigidbody>();
            eTankState = ETankState.Idle;

            UpdateTankAnim();
            Init();
        }

        protected abstract void Init();

        protected virtual void FixedUpdate()
        {
            if (IsCanMove && IsMoving)
            {
                PauseAnim(false);
            }
            else
            {
                PauseAnim(true);
            }
        }

        public void TankBeAttacked(BulletEntity bulletEntity, Action<bool> tankDeadCallback)
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
            UpdateTankAnim();
        }



        public void UpdateTankAnim()
        {
            string animName = "move" + TankTypeID;
            _animator.Play(animName);
        }
        public void PauseAnim(bool isPause)
        {
            if (isPause)
            {
                _animator.speed = 0;
            }
            else
            {
                _animator.speed = 1;
            }
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


    public enum MoveDirType
    {
        Forward,
        Back,
        Left,
        Right
    }
}
