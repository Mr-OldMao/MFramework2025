using MFramework.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMain
{
    public partial class PlayerEntity : MonoBehaviour
    {
        private GameObject player;
        private Transform imgTankIcon;

        private void Awake()
        {
            player = this.gameObject;
            //imgTankIcon = player.transform.Find<RectTransform>("imgTankIcon");
            imgTankIcon = player.transform.GetChild(0).GetComponent<Transform>();
        }

        private void Init(Vector2 gridPos, Vector2 mapPos)
        {
            InitMove(gridPos, mapPos);
        }

        private void FixedUpdate()
        {
            Move();
        }
    }
}
