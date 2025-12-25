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
                    imgTankIcon.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));
                    //player.transform.Translate(Vector3.forward * Time.deltaTime * moveSpeed);
                    m_Rigidbody.MovePosition(player.transform.position + Vector3.forward * Time.deltaTime * moveSpeed);
                    MoveDirType = MoveDirType.Forward;
                    IsMoving = true;
                }
                else if (Input.GetKey(KeyCode.S))
                {
                    imgTankIcon.localRotation = Quaternion.Euler(new Vector3(90, 180, 0));
                    //player.transform.Translate(Vector3.back * Time.deltaTime * moveSpeed);
                    m_Rigidbody.MovePosition(player.transform.position + Vector3.back * Time.deltaTime * moveSpeed);
                    MoveDirType = MoveDirType.Back;
                    IsMoving = true;
                }
                else if (Input.GetKey(KeyCode.A))
                {
                    imgTankIcon.localRotation = Quaternion.Euler(new Vector3(90, 270, 0));
                    //player.transform.Translate(Vector3.left * Time.deltaTime * moveSpeed);
                    m_Rigidbody.MovePosition(player.transform.position + Vector3.left * Time.deltaTime * moveSpeed);

                    MoveDirType = MoveDirType.Left;
                    IsMoving = true;
                }
                else if (Input.GetKey(KeyCode.D))
                {
                    imgTankIcon.localRotation = Quaternion.Euler(new Vector3(90, 90, 0));
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
