using MFramework.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GameMain
{
    public partial class PlayerEntity
    {
        public float moveSpeed;

        public Vector2 gridPos;
        public Vector2 mapPos;

        private Vector2 MaxGridPos;
        private Vector2 MaxMapPos;

        //public bool IsCanMove { get; set; } = true;
        private const float TankMoveSpeedConst = 3f;


        private ETCJoystick joystick;

        private bool m_InputTypeKeyword;
        private bool m_InputTypeTouch;
        private void InitMove(Vector2 gridPos)
        {
            MoveDirType = MoveDirType.Forward;

            this.gridPos = gridPos;
            UpdateTankMoveSpeed();
            //MaxGridPos = gridPos * UIModelMap.GRID_SIZE;
            //MaxMapPos = mapPos * UIModelMap.GRID_SIZE;
            joystick = GameEntry.UI.GetController<UIControlBattle>().Joystick;
            RectAnimTank.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }

        private void OnMoveEndHandler()
        {
            Debug.Log("joystick != null" + joystick);

        }

        private void OnMoveHandler()
        {
            Debug.Log("joystick != null" + joystick);
        }

        private void Move()
        {
            if (IsCanMove)
            {
                if (!m_InputTypeTouch)
                {
                    MovePC();
                }
                if (!m_InputTypeKeyword)
                {
                    MoveTouch();
                }
            }

            if (IsCanMove)
            {
                GameEntry.Audio.PlayBGM(IsMoving ? "engineMoving.mp3" : "engineIdle.mp3", 0.3f);
            }
        }

        private void UpdateTankMoveSpeed()
        {
            moveSpeed = DataTools.GetTankPlayer(tankTypeID).MoveSpeed * TankMoveSpeedConst;
        }

        private void MovePC()
        {
            IsMoving = Input.GetKey(KeyCode.W)
               || Input.GetKey(KeyCode.S)
               || Input.GetKey(KeyCode.A)
               || Input.GetKey(KeyCode.D);
            m_InputTypeKeyword = IsMoving;

            if (Input.GetKey(KeyCode.W))
            {
                Move(MoveDirType.Forward);
            }
            else if (Input.GetKey(KeyCode.S))
            {
                Move(MoveDirType.Back);
            }
            else if (Input.GetKey(KeyCode.A))
            {
                Move(MoveDirType.Left);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                Move(MoveDirType.Right);
            }
        }

        private void Move(MoveDirType moveDirType)
        {
            IsMoving = true;
            MoveDirType = moveDirType;
            Vector3 moveDir = Vector3.zero;
            Vector3 rotateDir = Vector3.zero;
            switch (moveDirType)
            {
                case MoveDirType.Forward:
                    rotateDir = new Vector3(0, 0, 0);
                    moveDir = Vector3.forward;

                    break;
                case MoveDirType.Back:
                    rotateDir = new Vector3(0, 0, 180);
                    moveDir = Vector3.back;

                    break;
                case MoveDirType.Left:
                    rotateDir = new Vector3(0, 0, 90);
                    moveDir = Vector3.left;
                    break;
                case MoveDirType.Right:
                    rotateDir = new Vector3(0, 0, 270);
                    moveDir = Vector3.right;
                    break;
            }
            RectAnimTank.transform.localRotation = Quaternion.Euler(rotateDir);
            m_Rigidbody.MovePosition(entity.transform.position + moveDir * Time.deltaTime * moveSpeed);
        }

        private void MoveTouch()
        {
            if (joystick != null)
            {
                IsMoving = joystick.axisY.axisValue != 0 || joystick.axisX.axisValue != 0;
                m_InputTypeTouch = IsMoving;

                if (joystick.axisY.axisValue != 0 && joystick.axisX.axisValue != 0)
                {
                    bool isForward = Math.Abs(joystick.axisY.axisValue) > Math.Abs(joystick.axisX.axisValue);
                    if (isForward)
                    {
                        Move(joystick.axisY.axisValue > 0 ? MoveDirType.Forward : MoveDirType.Back);
                    }
                    else
                    {
                        Move(joystick.axisX.axisValue > 0 ? MoveDirType.Right : MoveDirType.Left);
                    }
                }
                else if (joystick.axisY.axisValue != 0)
                {
                    Move(joystick.axisY.axisValue > 0 ? MoveDirType.Forward : MoveDirType.Back);
                }
                else if (joystick.axisX.axisValue != 0)
                {
                    Move(joystick.axisX.axisValue > 0 ? MoveDirType.Right : MoveDirType.Left);
                }
            }
        }
    }
}
