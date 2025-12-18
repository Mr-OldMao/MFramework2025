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

        private void InitMove(Vector2 gridPos, Vector2 mapPos)
        {
            this.gridPos = gridPos;
            this.mapPos = mapPos;
            MaxGridPos = gridPos * UIModelMap.GRID_SIZE;
            MaxMapPos = mapPos * UIModelMap.GRID_SIZE;
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
            //if (Input.GetKeyDown(KeyCode.W))
            //{
            //    imgTankIcon.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
            //}
            //else if (Input.GetKeyDown(KeyCode.S))
            //{
            //    imgTankIcon.localRotation = Quaternion.Euler(new Vector3(0, 0, 180));
            //}
            //else if (Input.GetKeyDown(KeyCode.A))
            //{
            //    imgTankIcon.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
            //}
            //else if (Input.GetKeyDown(KeyCode.D))
            //{
            //    imgTankIcon.localRotation = Quaternion.Euler(new Vector3(0, 0, 270));
            //}


            if (Input.GetKey(KeyCode.W))
            {
                imgTankIcon.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
                player.transform.Translate(Vector3.up * Time.deltaTime * moveSpeed);
            }
            else if ( Input.GetKey(KeyCode.S) )
            {
                imgTankIcon.localRotation = Quaternion.Euler(new Vector3(0, 0, 180));
                player.transform.Translate(Vector3.down * Time.deltaTime * moveSpeed);
            }
            else if (Input.GetKey(KeyCode.A))
            {
                imgTankIcon.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
                player.transform.Translate(Vector3.left * Time.deltaTime * moveSpeed);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                imgTankIcon.localRotation = Quaternion.Euler(new Vector3(0, 0, 270));
                player.transform.Translate(Vector3.right * Time.deltaTime * moveSpeed);
            }
        }
    }
}
