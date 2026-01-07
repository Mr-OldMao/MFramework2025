using MFramework.Runtime;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameMain
{
    [UIBind(typeof(UIControlMenu), typeof(UIModelMenu))]
    [UILayer(UILayerType.Tips)]
    public class UIPanelMenu : UIViewBase
    {
        // UI字段
        private RectTransform rootNode;
        private Image imgBg;
        private Button btnGameStart;

        public override async UniTask Init()
        {
            await base.Init();
        }

        public override void RefreshUI(IUIModel model = null)
        {
            if (model is not null)
            {

            }
        }

        public override async UniTask ShowPanel()
        {
            base.ShowPanel();

            if (GameMainLogic.Instance.GameStateType == GameStateType.GameSettlement)
            {
                ShowGameStartBtn(false);
                await GameEntry.UI.GetController<UIControlMap>().GenerateMapFirstStage();
                ShowGameStartBtn(true);
            }
            else
            {
                ShowGameStartBtn(true);
            }
            //return UniTask.CompletedTask;
        }


        public override UniTask HidePanel()
        {
            base.HidePanel();
            return UniTask.CompletedTask;
        }

        protected override void RegisterEvent()
        {
            btnGameStart.onClick.AddListener(async () =>
            {
                HidePanel();

                var UIPanelLoad = await GameEntry.UI.ShowViewAsync<UIPanelLoad>();
                UIPanelLoad.ShowLoadStage(() =>
                {
                    GameMainLogic.Instance.GameStateType = GameStateType.GameStart;
                });
            });
        }

        protected override void UnRegisterEvent()
        {
            btnGameStart.onClick.RemoveAllListeners();
        }



        public void ShowGameStartBtn(bool isShow)
        {
            btnGameStart.gameObject.SetActive(isShow);
        }
    }
}
