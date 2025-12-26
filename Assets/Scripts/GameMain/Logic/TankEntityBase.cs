using Cysharp.Threading.Tasks;
using MFramework.Runtime;
using System;
using System.Threading.Tasks;
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
        public bool IsCanMove;

        protected ETankState eTankState;

        private Animator m_AnimTank;
        private Animator m_AnimInvincible;
        private Animator m_AnimBorn;
        public bool IsMoving { get; protected set; } = false;

        protected Rigidbody m_Rigidbody;

        [SerializeField]
        protected bool m_IsInvincible;
        private int m_InvincibleTimerID;

        public async UniTask InitData(TankOwnerType tankOwnerType, int tankTypeID, int entityID)
        {
            TankOwnerType = tankOwnerType;
            TankTypeID = tankTypeID;
            EntityID = entityID;
            HP = tankOwnerType == TankOwnerType.Enemy ? DataTools.GetTankEnemy(tankTypeID).HP : DataTools.GetTankPlayer(tankTypeID).HP;
            RectAnimTank = transform.Find<SpriteRenderer>("rectAnimTank");
            m_AnimTank = transform.Find<Animator>("rectAnimTank");
            m_AnimInvincible = transform.Find<Animator>("rectAnimInvincible");
            m_AnimBorn = transform.Find<Animator>("rectAnimBorn");
            m_Rigidbody = GetComponentInChildren<Rigidbody>();
            eTankState = ETankState.Born;

            IsCanMove = false;
            await TankBorn();
            IsCanMove = true;
            eTankState = ETankState.Idle;

            UpdateTankAnim();
            Init();
        }

        protected abstract void Init();

        private async UniTask TankBorn()
        {
            m_AnimTank.gameObject.SetActive(false);
            m_AnimInvincible.gameObject.SetActive(false);
            m_AnimBorn.gameObject.SetActive(true);
            await UniTask.Delay(2000);
            m_AnimBorn.gameObject.SetActive(false);
            m_AnimTank.gameObject.SetActive(true);
            SetInvincible(2f);
        }

        protected virtual void FixedUpdate()
        {
            if (IsCanMove && IsMoving)
            {
                PauseMoveAnim(false);
            }
            else
            {
                PauseMoveAnim(true);
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
                    tankDeadCallback?.Invoke(true);
                    OnTankDead();
                    GameEntry.Event.DispatchEvent(GameEventType.TankDead, EntityID);
                }
                else
                {
                    tankDeadCallback?.Invoke(false);
                    OnTankHit(bulletEntity.BulletData.BulletATK);
                    GameEntry.Event.DispatchEvent(GameEventType.TankBeHit, EntityID);
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
            m_AnimTank.Play(animName);
        }
        public void PauseMoveAnim(bool isPause)
        {
            if (isPause)
            {
                m_AnimTank.speed = 0;
            }
            else
            {
                m_AnimTank.speed = 1;
            }
        }

        public void SetInvincible(float durationTime)
        {
            Debug.Log($"SetInvincible {gameObject.name} {durationTime}");
            m_IsInvincible = true;
            m_AnimInvincible.gameObject.SetActive(true);

            if (m_InvincibleTimerID >0)
            {
                Debug.Log($"SetInvincible RemoveDelayTimer{gameObject.name} {durationTime}");
                GameEntry.Timer.RemoveDelayTimer(m_InvincibleTimerID);
            }
            m_InvincibleTimerID = GameEntry.Timer.AddDelayTimer(durationTime, () =>
            {
                m_InvincibleTimerID = -1;
                m_IsInvincible = false;
                m_AnimInvincible.gameObject.SetActive(false);
            } );
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
        Born,
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
