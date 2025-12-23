using MFramework.Runtime;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

namespace GameMain
{
    public partial class EnemyEntity
    {
        public bool IsCanAIMove = true;
        /// <summary>
        /// 判定到达目标点的距离
        /// </summary>
        public float arriveDis = 0.05f;
        /// <summary>
        /// 判定射线打点在当前位置偏移量
        /// </summary>
        public float rayOffsetDis = 0.1f;
        public EAIMoveTargetType MoveTargetType;
        public EAIMoveState MoveState;

        public float moveSpeed;
        public Vector3 targetPos;

        private Queue<AIMoveData> m_ListAiMoveData = new Queue<AIMoveData>();
        private Transform nodeRayPoint;


        private float m_NextAutoMoveTime;
        private bool m_IsArrivedTargetPoint;


        /// <summary>
        /// 单次移动的时间最长长度
        /// </summary>
        public float MoveOnceMaxTimer = 2f;
        /// <summary>
        /// 本次移动的时间长度
        /// </summary>
        private float m_CurMovingTimer;


        private void InitAIMove()
        {
            if (nodeRayPoint == null)
            {
                nodeRayPoint = transform.Find("nodeRayPoint");
            }

            SetAIMoveState(EAIMoveState.Idle);
            SetAIMoveTargetType(EAIMoveTargetType.None);
            moveSpeed = m_TankEnemyData.MoveSpeed;
            SetNextAutoMoveTime();
            m_IsArrivedTargetPoint = true;
            IsCanAIMove = true;
        }

        private void SetNextAutoMoveTime()
        {
            m_NextAutoMoveTime = Random.Range(m_TankEnemyData.AutoMoveInterval(0), m_TankEnemyData.AutoMoveInterval(1));
        }

        [ContextMenu("移动到玩家旁")]
        public void AutoMove()
        {
            Debugger.Log("MoveToPlayer");
            m_IsArrivedTargetPoint = false;

            EAIMoveTargetType eAIMoveTargetType = EAIMoveTargetType.None;

            //0-玩家,1-鸟窝,2-智能选择最近的玩家或者鸟巢,3-其它
            float randomNum = Random.Range(0, 1f);
            if (randomNum > m_TankEnemyData.TargetProbability(0))
            {
                eAIMoveTargetType = EAIMoveTargetType.Player;
            }
            else if (randomNum + m_TankEnemyData.TargetProbability(0) >= m_TankEnemyData.TargetProbability(1))
            {
                eAIMoveTargetType = EAIMoveTargetType.Brid;
            }
            else if (randomNum + m_TankEnemyData.TargetProbability(0) + m_TankEnemyData.TargetProbability(1) >= m_TankEnemyData.TargetProbability(2))
            {
                eAIMoveTargetType = EAIMoveTargetType.PlayerOrBrid;
            }
            else
            {
                eAIMoveTargetType = EAIMoveTargetType.Any;
            }

            //Debug.Log("eAIMoveTargetType " + eAIMoveTargetType);
            SetAIMoveTargetType(eAIMoveTargetType);
            SetAIMoveState(EAIMoveState.Moving);
        }

        //public void MoveToBrid()
        //{
        //    m_IsArrivedTargetPoint = false;
        //    SetAIMoveTargetType(EAIMoveTargetType.Player);
        //    SetAIMoveState(EAIMoveState.Moving);
        //}

        [ContextMenu("终止移动")]
        public void StopMove()
        {
            IsMoving = false;
            SetAIMoveTargetType(EAIMoveTargetType.None);
            SetAIMoveState(EAIMoveState.Idle);
        }

        public void RestartMove()
        {
            StopMove();
            SetNextAutoMoveTime();
            m_IsArrivedTargetPoint = true;
            IsCanAIMove = true;
        }

        public void SetAIMoveState(EAIMoveState state)
        {
            MoveState = state;
            switch (state)
            {
                case EAIMoveState.Idle:
                    IsCanAIMove = false;
                    break;
                case EAIMoveState.Moving:
                    IsCanAIMove = true;
                    FindTargetPosPath();
                    break;
                case EAIMoveState.Attack:
                    IsCanAIMove = false;
                    break;
                case EAIMoveState.MovingAndAttack:
                    IsCanAIMove = true;
                    FindTargetPosPath();
                    break;
            }
        }

        void AIMoveUpdate()
        {
            if (IsCanAIMove)
            {
                if (m_ListAiMoveData?.Count > 0)
                {
                    m_CurMovingTimer += Time.deltaTime;
                    MoveToTarget(m_ListAiMoveData.Peek(), () =>
                    {
                        RestartMove();
                        m_ListAiMoveData.Dequeue();
                        //Debugger.Log("单次正常移动完成");
                    });

                    if (m_CurMovingTimer >= MoveOnceMaxTimer)
                    {
                        m_CurMovingTimer = 0;
                        m_ListAiMoveData.Clear();
                        RestartMove();
                        //Debugger.Log("单词移动超时，强制重新移动");
                    }
                }

                if (m_IsArrivedTargetPoint)
                {
                    m_NextAutoMoveTime -= Time.deltaTime;
                    if (m_NextAutoMoveTime <= 0)
                    {
                        m_CurMovingTimer = 0;
                        AutoMove();
                    }
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            TankEntityBase tankEntityBase = collision.gameObject.GetComponent<TankEntityBase>();
            if (tankEntityBase != null)
            {
                //ResetMove();
                Debugger.Log("OnCollisionEnter EnemyTank: " + gameObject);
            }
        }

        public void SetAIMoveTargetType(EAIMoveTargetType type)
        {
            targetPos = Vector3.zero;
            switch (type)
            {
                case EAIMoveTargetType.None:
                    break;
                case EAIMoveTargetType.Any:
                    targetPos = new Vector3(GameEntry.UI.GetModel<UIModelMap>().ROW_NUM, 0, GameEntry.UI.GetModel<UIModelMap>().ROW_NUM);
                    break;
                case EAIMoveTargetType.Player:
                    targetPos = GameMainLogic.Instance.Player1Entity.transform.position;
                    break;
                case EAIMoveTargetType.Brid:
                    targetPos = GameEntry.UI.GetModel<UIModelMap>().GridPosBrid;
                    break;
                case EAIMoveTargetType.PlayerOrBrid:
                    var playerPos = GameMainLogic.Instance.Player1Entity.transform.position;
                    var bridPos = GameEntry.UI.GetModel<UIModelMap>().GridPosBrid;
                    targetPos = Vector3.Distance(playerPos, transform.position) < Vector3.Distance(bridPos, transform.position) ? playerPos : bridPos;
                    break;
            }
        }

        #region 射线打点移动

        private void FindTargetPosPath()
        {
            Debugger.Log("射线打点");
            m_ListAiMoveData.Clear();

            List<AIMoveData> aiMoveDataArr = new List<AIMoveData>();
            Vector3 dir = Vector3.back;
            RaycastHit hitInfo;

            LayerMask mask = ~(LayerMask.GetMask("MapSnow", "MapGrass"));
            int rayLength = 1000;

            if (Physics.Raycast(nodeRayPoint.position, dir, out hitInfo, rayLength, mask))
            {
                Vector3 subTargetPoint = new Vector3((int)hitInfo.point.x, 0, (int)(hitInfo.point.z + 0.5f));
                //Debugger.Log($"射线打点,dir:{dir} , {hitInfo.transform.parent.gameObject} , hitInfo.point:{hitInfo.point} , subTargetPoint:{subTargetPoint}");
                if (Vector3.Distance(subTargetPoint, transform.position) > rayOffsetDis)
                {
                    aiMoveDataArr.Add(new AIMoveData { MoveDir = MoveDirType.Back, Pos = subTargetPoint });
                }
            }
            dir = Vector3.forward;
            if (Physics.Raycast(nodeRayPoint.position, dir, out hitInfo, rayLength, mask))
            {
                Vector3 subTargetPoint = new Vector3((int)hitInfo.point.x, 0, (int)(hitInfo.point.z - 1));
                //Debugger.Log($"射线打点,dir:{dir} , {hitInfo.transform.parent.gameObject} , hitInfo.point:{hitInfo.point} , subTargetPoint:{subTargetPoint}");
                if (Vector3.Distance(subTargetPoint, transform.position) > rayOffsetDis)
                {
                    aiMoveDataArr.Add(new AIMoveData { MoveDir = MoveDirType.Forward, Pos = subTargetPoint });
                }
            }
            dir = Vector3.left;
            if (Physics.Raycast(nodeRayPoint.position, dir, out hitInfo, rayLength, mask))
            {
                //Vector3 subTargetPoint = new Vector3((int)(hitInfo.point.x + 0.5f), 0, (int)hitInfo.point.z);
                Vector3 subTargetPoint = new Vector3((int)(hitInfo.point.x + 0.8f), 0, (int)hitInfo.point.z);
                //Debugger.Log($"射线打点,dir:{dir} , {hitInfo.transform.parent.gameObject} , hitInfo.point:{hitInfo.point} , subTargetPoint:{subTargetPoint}");
                if (Vector3.Distance(subTargetPoint, transform.position) > rayOffsetDis)
                {
                    aiMoveDataArr.Add(new AIMoveData { MoveDir = MoveDirType.Left, Pos = subTargetPoint });
                }
            }
            dir = Vector3.right;
            if (Physics.Raycast(nodeRayPoint.position, dir, out hitInfo, rayLength, mask))
            {
                Vector3 subTargetPoint = new Vector3((int)hitInfo.point.x - 1, 0, (int)hitInfo.point.z);
                //Debugger.Log($"射线打点,dir:{dir} , {hitInfo.transform.parent.gameObject} , hitInfo.point:{hitInfo.point} , subTargetPoint:{subTargetPoint}");
                if (Vector3.Distance(subTargetPoint, transform.position) > rayOffsetDis)
                {
                    aiMoveDataArr.Add(new AIMoveData { MoveDir = MoveDirType.Right, Pos = subTargetPoint });
                }
            }

            if (aiMoveDataArr.Count == 0)
            {
                Debugger.LogError($"{gameObject} 无法移动 重新移动 MoveToPlayer");
                RestartMove();
            }
            else
            {
                m_ListAiMoveData.Enqueue(FindTargetSubPoint(aiMoveDataArr));
            }
        }

        private AIMoveData FindTargetSubPoint(List<AIMoveData> aiMoveDataArr)
        {
            AIMoveData res = aiMoveDataArr[0];
            for (int i = 1; i < aiMoveDataArr.Count; i++)
            {
                ////离目标点最近的点位方向
                float curDis = Vector3.Distance(aiMoveDataArr[i].Pos, targetPos);
                float resDis = Vector3.Distance(res.Pos, targetPos);

                if (Vector3.Distance(aiMoveDataArr[i].Pos, targetPos) < Vector3.Distance(res.Pos, targetPos))
                {
                    res = aiMoveDataArr[i];
                }
                //Debugger.Log($"寻找打点  MoveDir:{aiMoveDataArr[i].MoveDir},Pos:{aiMoveDataArr[i].Pos},curDis:{curDis},resDis:{resDis}");

                ////TODO 当前可走的最长路径
                //if (Vector3.Distance(aiMoveDataArr[i].Pos, transform.position) > Vector3.Distance(aiMoveDataArr[i].Pos, transform.position))
                //{
                //    aimoveDataInfo = aiMoveDataArr[i];
                //}
            }

            switch (res.MoveDir)
            {
                case MoveDirType.Forward:
                    res.Pos = new Vector3(res.Pos.x, 0, Random.Range((int)(transform.position.z) + 1, (int)res.Pos.z + 1));
                    break;
                case MoveDirType.Back:
                    res.Pos = new Vector3(res.Pos.x, 0, Random.Range((int)res.Pos.z, (int)(transform.position.z)));
                    break;
                case MoveDirType.Left:
                    res.Pos = new Vector3(Random.Range((int)res.Pos.x, (int)(transform.position.x)), 0, (int)res.Pos.z);
                    break;
                case MoveDirType.Right:
                    res.Pos = new Vector3(Random.Range((int)(transform.position.x) + 1, (int)res.Pos.x + 1), 0, (int)res.Pos.z);
                    break;
            }

            Debugger.Log($"射线打点 subTargetPoint:{res.Pos}");
            return res;
        }
        #endregion

        // 移动到目标点
        public void MoveToTarget(AIMoveData targetPosition, Action arriveCallback)
        {
            IsMoving = true;
            Vector3 moveDir = Vector3.zero;
            switch (targetPosition.MoveDir)
            {
                case MoveDirType.Forward:
                    moveDir = Vector3.forward;
                    NodeSpriteRenderer.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));
                    break;
                case MoveDirType.Back:
                    moveDir = Vector3.back;
                    NodeSpriteRenderer.transform.localRotation = Quaternion.Euler(new Vector3(90, 180, 0));
                    break;
                case MoveDirType.Left:
                    moveDir = Vector3.left;
                    NodeSpriteRenderer.transform.localRotation = Quaternion.Euler(new Vector3(90, 270, 0));
                    break;
                case MoveDirType.Right:
                    moveDir = Vector3.right;
                    NodeSpriteRenderer.transform.localRotation = Quaternion.Euler(new Vector3(90, 90, 0));
                    break;
            }
            m_Rigidbody.MovePosition(enemy.transform.position + moveDir * Time.deltaTime * moveSpeed);

            if (IsArriveTarget(transform.position, targetPosition.Pos))
            {
                //Debugger.Log("到达目标点");
                transform.position = targetPosition.Pos;
                arriveCallback?.Invoke();
            }
        }

        private bool IsArriveTarget(Vector3 curPos, Vector3 targetPosition)
        {
            float dis = Vector3.Distance(curPos, targetPosition);
            return dis < arriveDis;
        }

        public class AIMoveData
        {
            public MoveDirType MoveDir;
            public Vector3 Pos;
        }

    }

    public enum EAIMoveTargetType
    {
        /// <summary>
        /// 无目标
        /// </summary>
        None = 0,
        /// <summary>
        /// 任意目标
        /// </summary>
        Any,
        Player,
        Brid,
        PlayerOrBrid
    }

    public enum EAIMoveState
    {
        Idle,
        Moving,
        Attack,
        MovingAndAttack,
    }
}
