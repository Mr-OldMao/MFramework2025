using MFramework.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMain
{
    public partial class PlayerEntity
    {

        private void OnCollisionEnter2D(Collision2D collision)
        {
            Debug.Log("OnCollisionEnter2D "+ collision.gameObject.name);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            Debug.Log("OnCollisionStay2D " + collision.gameObject.name);

        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            Debug.Log("OnCollisionExit2D " + collision.gameObject.name);

        }
    }
}
