using MFramework.Runtime;
using System;
using UnityEngine;

namespace GameMain
{
    public abstract class TankEntityBase : MonoBehaviour
    {
        public TankOwnerType TankOwnerType { get; private set; }

        public MoveDirType MoveDirType { get; protected set; }

        public SpriteRenderer RectAnimTank { get; private set; }
    

        [SerializeField]
        protected int EntityID;
        [SerializeField]
        protected int HP;
        [SerializeField]
        protected int TankTypeID;
        [SerializeField]
        public bool IsCanMove = true;

        protected ETankState eTankState;

        private Animator m_Animator;
        public bool IsMoving { get; protected set; } = false;

        protected Rigidbody m_Rigidbody;


        public void InitData(TankOwnerType tankOwnerType, int tankTypeID, int entityID)
        {
            TankOwnerType = tankOwnerType;
            TankTypeID = tankTypeID;
            EntityID = entityID;
            HP = tankOwnerType == TankOwnerType.Enemy ? DataTools.GetTankEnemy(tankTypeID).HP : DataTools.GetTankPlayer(tankTypeID).HP;
            RectAnimTank = transform.Find<SpriteRenderer>("rectAnimTank");
            m_Animator = GetComponentInChildren<Animator>();
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

        public void TankBeHit(BulletEntity bulletEntity, Action<bool> tankDeadCallback)
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
                    OnTankHit(bulletEntity.BulletData.BulletATK);
                    GameEntry.Event.DispatchEvent(GameEventType.TankBeHit, EntityID);
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
            m_Animator.Play(animName);
        }
        public void PauseAnim(bool isPause)
        {
            if (isPause)
            {
                m_Animator.speed = 0;
            }
            else
            {
                m_Animator.speed = 1;
            }
        }

        protected abstract void OnTankDead();

        protected abstract void OnTankHit(int hitValue);
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
