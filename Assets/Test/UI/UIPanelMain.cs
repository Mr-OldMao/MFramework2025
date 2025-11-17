// ExampleUI.cs
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MFramework.Runtime.UI
{
    [UILayer(UILayerType.Tips)]
    public class UIPanelMain : UIViewBase
    {
        public TextMeshProUGUI txtTest;
        public Button btnTest;
        public Button btnClose;

        public override async Task Initialize()
        {
            Debugger.Log($"{this.GetType()}, init start  ");
            await Task.Delay(2000);
            Debugger.Log($"{this.GetType()}, init end  ");
        }

        protected override Task OnShowBefore(object data)
        {
            Debugger.Log($"{this.GetType().Name}, OnShowBefore ,data: {data}");
            return base.OnShowBefore(data);
        }

        protected override Task OnHideBefore(object data)
        {
            Debugger.Log($"{this.GetType().Name}, OnHideBefore ,data: {data}");
            return base.OnHideBefore(data);
        }

        protected override void OnShow(object data)
        {
            Debugger.Log($"{this.GetType().Name},OnShow , data: {data}");
            if (data is string title)
            {
                txtTest.text = title;
            }

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

        protected override void OnHide(object data)
        {
            Debugger.Log($"{this.GetType().Name},OnHide , data: {data}");

            btnTest.onClick.RemoveAllListeners();
            btnClose.onClick.RemoveAllListeners();
        }

    }
}