// ExampleUI.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MFramework.Runtime.UI
{
    [UILayer( UILayerType.Tips)]
    public class UIPanelMain : UIBase
    {
        public TextMeshProUGUI txtTest;

        public Button btnTest;

        public Button btnClose;

        protected override void OnShow(object data)
        {
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
                Close();
            });
        }

        protected override void OnHide()
        {
            btnTest.onClick.RemoveAllListeners();
            btnClose.onClick.RemoveAllListeners();
        }
    }
}