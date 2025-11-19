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

        public override void RefreshUI(IUIModel model = null)
        {
            UIModelMain uIModelMain = GameEntry.UI.GetModel<UIModelMain>();
            Debugger.Log($"{this.GetType().Name},RefreshUI ");
            txtTest.text = uIModelMain.Title;
        }

        public override Task ShowPanel()
        {
            base.ShowPanel();
            Debugger.Log($"{this.GetType().Name},ShowPanel ");
            RegisterUIEvent();
            RefreshUI();
            return Task.CompletedTask;
        }

        public override Task HidePanel()
        {
            base.HidePanel();
            Debugger.Log($"{this.GetType().Name},HidePanel ");
            UnRegisterEvent();
            return Task.CompletedTask;
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
                ChangeSprite();
            });

            GameEntry.Event.RegisterEvent(GameEventType.TestUIEvent, () => RefreshUI());
        }

        public void ChangeSprite()
        {
            Debugger.Log($"btnChangeSprite click");
            SetSprite(imgItem1, EAtlasType.temp, "resFileImgPlane" + Random.Range(1, 7));
            SetSprite(imgItem2, EAtlasType.temp, "resFileImgPlane" + Random.Range(1, 7));
            SetSprite(imgItem3, EAtlasType.temp, "resFileImgPlane" + Random.Range(1, 7));
        }

        private void UnRegisterEvent()
        {
            Debugger.Log($"{this.GetType().Name},UnRegisterUIEvent ");
            btnChangeData.onClick.RemoveAllListeners();
            btnClose.onClick.RemoveAllListeners();
            btnDestory.onClick.RemoveAllListeners();
            btnChangeSprite.onClick.RemoveAllListeners();
            GameEntry.Event.UnRegisterEvent(GameEventType.TestUIEvent);
        }
    }
}