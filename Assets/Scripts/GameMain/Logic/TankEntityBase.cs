using Cysharp.Threading.Tasks;
using MFramework.Runtime;
using System;
using UnityEngine;

namespace GameMain
{
    public abstract class TankEntityBase : MonoBehaviour
    {
        public GameObject entity;
        public float bornTime = 1f;
        public TankOwnerType TankOwnerType { get; private set; }

        public MoveDirType MoveDirType { get; protected set; }

        public SpriteRenderer RectAnimTank { get; private set; }


        [SerializeField]
        protected int EntityID;
        [SerializeField]
        protected int HP;
        [SerializeField]
        protected int tankTypeID;
        public int TankTypeID { get => tankTypeID; }
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

        /// <summary>
        /// 下次坦克生成是否继承之前的坦克属性（子弹等级等）
        /// </summary>
        public bool IsExtendBeforeDataNextGenerate;

        public async UniTask InitData(TankOwnerType tankOwnerType, int tankTypeID, int entityID)
        {
            if (entity == null)
            {
                entity = this.gameObject;
                RectAnimTank = transform.Find<SpriteRenderer>("rectAnimTank");
                m_AnimTank = transform.Find<Animator>("rectAnimTank");
                m_AnimInvincible = transform.Find<Animator>("rectAnimInvincible");
                m_AnimBorn = transform.Find<Animator>("rectAnimBorn");
                m_Rigidbody = GetComponentInChildren<Rigidbody>();
            }

            TankOwnerType = tankOwnerType;
            this.tankTypeID = tankTypeID;
            EntityID = entityID;
            HP = tankOwnerType == TankOwnerType.Enemy ? DataTools.GetTankEnemy(tankTypeID).HP : DataTools.GetTankPlayer(tankTypeID).HP;
            InitBornBefore();
            await TankBorn(tankOwnerType);

            UpdateTankAnim();
            InitBornAfter();
        }

        public abstract void GameWinEvent();
        public abstract void GameFailEvent();

        protected abstract void InitBornBefore();

        protected abstract void InitBornAfter();

        protected async UniTask TankBorn(TankOwnerType tankOwnerType)
        {
            eTankState = ETankState.Born;

            Vector3 bornPos = Vector3.zero;
            switch (tankOwnerType)
            {
                case TankOwnerType.Player1:
                    bornPos = new Vector3(GameEntry.UI.GetModel<UIModelMap>().GridPosBornPlayer1.x, 0, GameEntry.UI.GetModel<UIModelMap>().GridPosBornPlayer1.y);
                    break;
                case TankOwnerType.Player2:
                    bornPos = new Vector3(GameEntry.UI.GetModel<UIModelMap>().GridPosBornPlayer2.x, 0, GameEntry.UI.GetModel<UIModelMap>().GridPosBornPlayer2.y);

                    break;
                case TankOwnerType.Enemy:
                    Vector2 posBornEnemy = GameEntry.UI.GetModel<UIModelMap>().GetRandomGridPosBornEnemy();
                    bornPos = new Vector3(posBornEnemy.x, 0, posBornEnemy.y);
                    break;
            }
            entity.transform.localPosition = bornPos;
            entity.SetActive(true);

            IsCanMove = false;
            m_AnimTank.gameObject.SetActive(false);
            m_AnimInvincible.gameObject.SetActive(false);
            m_AnimBorn.gameObject.SetActive(true);
            await UniTask.Delay((int)(bornTime * 1000));
            m_AnimBorn.gameObject.SetActive(false);
            m_AnimTank.gameObject.SetActive(true);
            SetInvincible(bornTime);
            IsCanMove = true;
            eTankState = ETankState.Idle;
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
                    //IsCanMove = false;
                    OnTankDead();
                    switch (TankOwnerType)
                    {
                        case TankOwnerType.Player1:
                            GameEntry.Event.DispatchEvent(GameEventType.Player1TankDead, entity);
                            break;
                        case TankOwnerType.Player2:
                            GameEntry.Event.DispatchEvent(GameEventType.Player2TankDead, entity);
                            break;
                        case TankOwnerType.Enemy:
                            GameEntry.Event.DispatchEvent(GameEventType.EnemyTankDead, entity);
                            GameEntry.UI.GetModel<UIModelSettlement>().AddScore(tankTypeID, bulletEntity.tankOwnerType);
                            if (GameMainLogic.Instance.RemainEnemyTankNum == 0 && GameEntry.Pool.GetPool(GameMainLogic.Instance.PoolIdTankEnemy).UsedCount == 0)
                            {
                                GameMainLogic.Instance.GameStateType = GameStateType.GameWin;
                            }
                            //GameMainLogic.Instance.TryJudgeGameWin();
                            break;
                    }
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
            this.tankTypeID = tankTypeID;
            UpdateTankAnim();
        }

        public void UpdateTankAnim()
        {
            string animName = "move" + tankTypeID;
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

            if (m_InvincibleTimerID > 0)
            {
                Debug.Log($"SetInvincible RemoveDelayTimer{gameObject.name} {durationTime}");
                GameEntry.Timer.RemoveDelayTimer(m_InvincibleTimerID);
            }
            m_InvincibleTimerID = GameEntry.Timer.AddDelayTimer(durationTime, () =>
            {
                m_InvincibleTimerID = -1;
                m_IsInvincible = false;
                m_AnimInvincible.gameObject.SetActive(false);
            });
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
