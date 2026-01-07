using MFramework.Runtime;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameMain
{
    [UIBind(typeof(UIControlMap), typeof(UIModelMap))]
    [UILayer(UILayerType.Background)]
    public class UIPanelMap : UIViewBase
    {
        // UI字段
        public RectTransform rootNode;
        public Image imgBg;
        public RectTransform NodeContainer;
        public RectTransform nodeBornPlayer1;
        public RectTransform nodeBornPlayer2;
        public RectTransform nodeBornEnemy1;
        public RectTransform nodeBornEnemy2;
        public RectTransform nodeBornEnemy3;
        public RectTransform nodeHomeWall;
        public RectTransform nodeOther;

        public override async UniTask Init()
        {
            await base.Init();
            Debugger.Log("UIPanelMap Init Completed");
        }

        public override void RefreshUI(IUIModel model = null)
        {
            if (model is not null)
            {

            }


        }

        public override UniTask ShowPanel()
        {
            base.ShowPanel();

            Debug.Log("ShowPanelShowPanel");
            return UniTask.CompletedTask;
        }

        public override UniTask HidePanel()
        {
            base.HidePanel();
            return UniTask.CompletedTask;
        }

        protected override void RegisterEvent()
        {
            
        }

        protected override void UnRegisterEvent()
        {

         }

        // 私有方法
    }
}
