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
        private int tankLevel;
        private MoveDirType moveDirType = MoveDirType.Forward;

        private void Awake()
        {
            player = this.gameObject;
            //imgTankIcon = player.transform.Find<RectTransform>("imgTankIcon");
            imgTankIcon = player.transform.GetChild(0).GetComponent<Transform>();

            tankLevel = 1;
            Init(Vector2.zero);
        }

        private void Init(Vector2 gridPos)
        {
            InitMove(gridPos);
            InitFire();
        }

        private void FixedUpdate()
        {
            Move();

            Fire();
        }
    }
}
