using Cysharp.Threading.Tasks;
using MFramework.Runtime;
using Unity.VisualScripting;
using UnityEngine.UI;

namespace GameMain
{
    [UIBind(typeof(UIControlBattle), typeof(UIModelBattle))]
    [UILayer(UILayerType.Tips)]
    public class UIPanelBattle : UIViewBase
    {
        public Button btnFire;

        public override UniTask Init()
        {
            return base.Init();
        }

        public override UniTask ShowPanel()
        {
            return base.ShowPanel();
        }

        public override UniTask HidePanel()
        {
            return base.HidePanel();
        }

        public override void RefreshUI(IUIModel uIModel = null)
        {

        }

        protected override void RegisterEvent()
        {
            btnFire.GetOrAddComponent<UIEvents>().AddListenerLongPressEvent((p) =>
            {
                GameMainLogic.Instance.Player1Entity.FireByTouch();
            }, 0.1f);
        }

        protected override void UnRegisterEvent()
        {
            btnFire.GetComponent<UIEvents>().RemoveListenerLongPressEvent();
        }
    }
}
