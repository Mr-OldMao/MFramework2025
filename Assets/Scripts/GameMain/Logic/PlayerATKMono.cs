using GameMain;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerATKMono : MonoBehaviour
{
    
    public void ATK()
    {
        GameMainLogic.Instance.Player1Entity?.FireByTouch();
    }
}
