using GameMain;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.UI;

namespace MFramework.Runtime.UI
{
    [UIBindControl( typeof(UIControlMain), typeof(UIModelMain))]
    [UILayer(UILayerType.Tips)]
    public class UIPanelMain : UIViewBase
    {
        public TextMeshProUGUI txtTest;
        public Button btnTest;
        public Button btnClose;

        public override async Task Initialize()
        {
            Debugger.Log($"{this.GetType()}, Initialize start  ");
            await Task.Delay(2000);
            Debugger.Log($"{this.GetType()}, Initialize end  ");
        }

        public override void RefreshUI(IUIModel model)
        {
            UIModelMain uIModelMain = model as UIModelMain;
            Debugger.Log($"{this.GetType().Name},RefreshUI ");
            txtTest.text = uIModelMain.Title;
        }

        public override void ShowPanel(IUIModel uIModel)
        {
            Debugger.Log($"{this.GetType().Name},ShowPanel , data: {uIModel}");
            RegisterUIEvent();
        }

        public override void HidePanel(IUIModel uIModel)
        {
            Debugger.Log($"{this.GetType().Name},HidePanel , data: {uIModel}");
            UnRegisterEvent();
        }

        private void RegisterUIEvent()
        {
            btnTest.onClick.AddListener(() =>
            {
                Debugger.Log("btnTest click");
            });
            btnClose.onClick.AddListener(() =>
            {
                Debugger.Log("btnClose click");
                DestoryUI();
            });
        }

        private void UnRegisterEvent()
        {
            Debugger.Log($"{this.GetType().Name},UnRegisterUIEvent ");
            btnTest.onClick.RemoveAllListeners();
            btnClose.onClick.RemoveAllListeners();
        }
    }
}