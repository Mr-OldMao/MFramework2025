using Cysharp.Threading.Tasks;
using MFramework.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameMain
{
    [UIBind(typeof(UIControlMain), typeof(UIModelMain))]
    [UILayer(UILayerType.Tips)]
    public class UIPanelMain : UIViewBase
    {
        public TextMeshProUGUI txtTest;
        public Button btnChangeData;
        public Button btnClose;
        public Button btnChangeSprite;

        public Image imgItem1;
        public Image imgItem2;
        public Image imgItem3; 
        public override async UniTask Init()
        {
            await base.Init();
            Debugger.Log($"{this.GetType()}, Initialize start (delay 500ms)");
            await UniTask.Delay(500);
            Debugger.Log($"{this.GetType()}, Initialize end  ");
        }

        public override void RefreshUI(IUIModel model = null)
        {
            UIModelMain uIModelMain = GameEntry.UI.GetModel<UIModelMain>();
            Debugger.Log($"{this.GetType().Name},RefreshUI ");
            txtTest.text = uIModelMain.Title;
        }

        public override UniTask ShowPanel()
        {
            base.ShowPanel();
            Debugger.Log($"{this.GetType().Name},ShowPanel ");
            RefreshUI();
            return UniTask.CompletedTask;
        }

        public override UniTask HidePanel()
        {
            base.HidePanel();
            Debugger.Log($"{this.GetType().Name},HidePanel ");
            UnRegisterEvent();
            return UniTask.CompletedTask;
        }

        public override void Shutdown()
        {
            base.Shutdown();
        }
         
        public void ChangeSprite()
        {
            Debugger.Log($"btnChangeSprite click");
            SetSprite(imgItem1, EAtlasType.temp, "resFileImgPlane" + Random.Range(1, 7));
            SetSprite(imgItem2, EAtlasType.temp, "resFileImgPlane" + Random.Range(1, 7));
            SetSprite(imgItem3, EAtlasType.temp, "resFileImgPlane" + Random.Range(1, 7));
        }

        protected override void RegisterEvent()
        {
            btnChangeData.onClick.AddListener(() =>
            {
                Debugger.Log("btnChangeData click 修改数据");
                int data = Random.Range(1000, 9999);
                (Controller as UIControlMain).SetTitleData($"{data}");
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

        protected override void UnRegisterEvent()
        {
            Debugger.Log($"{this.GetType().Name},UnRegisterUIEvent ");
            btnChangeData.onClick.RemoveAllListeners();
            btnClose.onClick.RemoveAllListeners();
            btnChangeSprite.onClick.RemoveAllListeners();
            GameEntry.Event.UnRegisterEvent(GameEventType.TestUIEvent);
        }
    }
}