using MFramework.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMain
{
    public partial class PlayerEntity
    {
        public float moveSpeed = 5f;

        public Vector2 gridPos;
        public Vector2 mapPos;

        private Vector2 MaxGridPos;
        private Vector2 MaxMapPos;

        public bool IsCanMove { get; set; } = true;

        private void InitMove(Vector2 gridPos)
        {
            this.gridPos = gridPos;
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

        private void MovePC()
        {
            if (Input.GetKey(KeyCode.W))
            {
                imgTankIcon.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));
                player.transform.Translate(Vector3.forward * Time.deltaTime * moveSpeed);
                moveDirType = MoveDirType.Forward;
            }
            else if ( Input.GetKey(KeyCode.S) )
            {
                imgTankIcon.localRotation = Quaternion.Euler(new Vector3(90, 180, 0));
                player.transform.Translate(Vector3.back * Time.deltaTime * moveSpeed);
                moveDirType = MoveDirType.Back;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                imgTankIcon.localRotation = Quaternion.Euler(new Vector3(90, 270, 0));
                player.transform.Translate(Vector3.left * Time.deltaTime * moveSpeed);
                moveDirType = MoveDirType.Left;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                imgTankIcon.localRotation = Quaternion.Euler(new Vector3(90, 90, 0));
                player.transform.Translate(Vector3.right * Time.deltaTime * moveSpeed);
                moveDirType = MoveDirType.Right;
            }
        }
    }

    public enum MoveDirType
    {
        Forward,
        Back,
        Left,
        Right
    }
}
