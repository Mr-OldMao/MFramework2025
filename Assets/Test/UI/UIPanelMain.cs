using GameMain;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MFramework.Runtime.UI
{
    [UIBind(typeof(UIControlMain), typeof(UIModelMain))]
    [UILayer(UILayerType.Tips)]
    public class UIPanelMain : UIViewBase
    {
        public TextMeshProUGUI txtTest;
        public Button btnChangeData;
        public Button btnClose;
        public Button btnDestory;
        public Button btnChangeSprite;

        public Image imgItem1;
        public Image imgItem2;
        public Image imgItem3;
        public override async Task Initialize()
        {
            Debugger.Log($"{this.GetType()}, Initialize start (delay 500ms)");
            await Task.Delay(500);
            Debugger.Log($"{this.GetType()}, Initialize end  ");
        }

        public override void RefreshUI(IUIModel model)
        {
            UIModelMain uIModelMain = model as UIModelMain;
            Debugger.Log($"{this.GetType().Name},RefreshUI ");
            txtTest.text = uIModelMain.Title;
        }

        public override void ShowPanel(IUIModel uIModel = null)
        {
            base.ShowPanel(uIModel);
            Debugger.Log($"{this.GetType().Name},ShowPanel , data: {uIModel}");
            RegisterUIEvent();

            RefreshUI(uIModel);
        }

        public override void HidePanel(IUIModel uIModel = null)
        {
            base.HidePanel(uIModel);
            Debugger.Log($"{this.GetType().Name},HidePanel , data: {uIModel}");
            UnRegisterEvent();
        }

        public override void OnDestory()
        {
            base.OnDestory();
            UnRegisterEvent();
        }

        private void RegisterUIEvent()
        {
            btnChangeData.onClick.AddListener(() =>
            {
                Debugger.Log("btnChangeData click 修改数据");
                int data = Random.Range(1000, 9999);
                (Controller as UIControlMain).SetTitleData($"{data}");
            });
            btnDestory.onClick.AddListener(() =>
            {
                Debugger.Log("btnDestory click");
                OnDestory();
            });
            btnClose.onClick.AddListener(() =>
            {
                Debugger.Log("btnClose click");
                HidePanel();
            });
            btnChangeSprite.onClick.AddListener(() =>
            {
                var spriteName = "resFileImgPlane" + Random.Range(1, 7);
                Debugger.Log($"btnChangeSprite click , spriteName :{spriteName}");
                SetSprite(imgItem1, EAtlasType.temp, spriteName);
            });
        }

        private void UnRegisterEvent()
        {
            Debugger.Log($"{this.GetType().Name},UnRegisterUIEvent ");
            btnChangeData.onClick.RemoveAllListeners();
            btnClose.onClick.RemoveAllListeners();
        }
    }
}