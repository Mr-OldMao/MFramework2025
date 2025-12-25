using MFramework.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMain
{
    public partial class PlayerEntity
    {
        public float moveSpeed;

        public Vector2 gridPos;
        public Vector2 mapPos;

        private Vector2 MaxGridPos;
        private Vector2 MaxMapPos;

        public bool IsCanMove { get; set; } = true;
        private const float TankMoveSpeedConst = 3f;

        private void InitMove(Vector2 gridPos)
        {
            this.gridPos = gridPos;
            UpdateTankMoveSpeed();
            //MaxGridPos = gridPos * UIModelMap.GRID_SIZE;
            //MaxMapPos = mapPos * UIModelMap.GRID_SIZE;
        }

        private void Move()
        {
            if (IsCanMove)
            {
                MovePC();
            }
        }

        private void UpdateTankMoveSpeed()
         {
            moveSpeed = DataTools.GetTankPlayer(TankTypeID).MoveSpeed * TankMoveSpeedConst;
        }

        private void MovePC()
        {
            if (IsCanMove)
            {
                if (Input.GetKey(KeyCode.W))
                {
                    RectAnimTank.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
                    //player.transform.Translate(Vector3.forward * Time.deltaTime * moveSpeed);
                    m_Rigidbody.MovePosition(player.transform.position + Vector3.forward * Time.deltaTime * moveSpeed);
                    MoveDirType = MoveDirType.Forward;
                    IsMoving = true;
                }
                else if (Input.GetKey(KeyCode.S))
                {
                    RectAnimTank.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 180));
                    //player.transform.Translate(Vector3.back * Time.deltaTime * moveSpeed);
                    m_Rigidbody.MovePosition(player.transform.position + Vector3.back * Time.deltaTime * moveSpeed);
                    MoveDirType = MoveDirType.Back;
                    IsMoving = true;
                }
                else if (Input.GetKey(KeyCode.A))
                {
                    RectAnimTank.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
                    //player.transform.Translate(Vector3.left * Time.deltaTime * moveSpeed);
                    m_Rigidbody.MovePosition(player.transform.position + Vector3.left * Time.deltaTime * moveSpeed);

                    MoveDirType = MoveDirType.Left;
                    IsMoving = true;
                }
                else if (Input.GetKey(KeyCode.D))
                {
                    RectAnimTank.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 270));
                    //player.transform.Translate(Vector3.right * Time.deltaTime * moveSpeed);
                    m_Rigidbody.MovePosition(player.transform.position + Vector3.right * Time.deltaTime * moveSpeed);
                    MoveDirType = MoveDirType.Right;
                    IsMoving = true;
                }
                else
                {
                    IsMoving = false;
                } 
            }
        }
    }
}
