using MFramework.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMain
{
    public partial class PlayerEntity
    {
        private Animator _animator;

        private void InitAnim()
        {
            _animator = GetComponentInChildren<Animator>();
        }

        private void UpdateTankAnim()
        {
            string animName = "move" + (int)TankType;
            _animator.Play(animName);
        }

        private void PauseAnim(bool isPause)
        {
            if (isPause)
            {
                _animator.speed = 0;
            }
            else
            {
                _animator.speed = 1;
            }
        }
    }
}
