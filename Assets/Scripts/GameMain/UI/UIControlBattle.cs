using MFramework.Runtime;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameMain
{
    public class UIControlBattle : UIControllerBase
    {
        public ETCJoystick Joystick { get; private set; }

        public override async UniTask Init(IUIView view, IUIModel model)
        {
            await base.Init(view, model);


            Joystick = ((UIPanelBattle)(view)).gameObject.GetComponentInChildren<ETCJoystick>();
        }


    }
}
