using MFramework.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMain
{
    public partial class PlayerEntity : MonoBehaviour
    {
        private GameObject player;
        private RectTransform imgTankIcon;

        private void Awake()
        {
            player = this.gameObject;
            imgTankIcon = player.transform.Find<RectTransform>("imgTankIcon");
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
