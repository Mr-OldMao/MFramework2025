using MFramework.Runtime;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameMain
{
    //[UIBind(typeof(UIControlGM), typeof(UIModelGM))]
    [UILayer(UILayerType.Background)]
    public class UIPanelGM : UIViewBase
    {
        // UI字段
        private RectTransform rootNode;
        private TMP_InputField inputTxtMapTypeID;
        private Button btnRegenerateMap;
        private Button btnTankLevelAdd;
        private Button btnTankLevelSub;
        private Button btnGenerateEntmyTank;
        private Button btnGameParse;


        private PlayerEntity PlayerEntity;
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

        public override UniTask ShowPanel()
        {
            Debug.Log("UIPanelGM ShowPanel");
            base.ShowPanel();
            return UniTask.CompletedTask;
        }

        public override UniTask HidePanel()
        {
            base.HidePanel();
            return UniTask.CompletedTask;
        }

        protected override void RegisterEvent()
        {
            Debugger.Log("UIPanelGM RegisterEvent");

            GameEntry.Event.RegisterEvent(GameEventType.GameStart, () =>
            {

            });

            btnRegenerateMap.onClick.AddListener(() =>
            {
                if (int.TryParse(inputTxtMapTypeID.text, out int mapTypeID))
                {
#pragma warning disable CS4014
                    GameEntry.UI.GetController<UIControlMap>().GenerateMapByMapTypeID(mapTypeID);
#pragma warning restore CS4014
                }
                else
                {
                    Debugger.LogError("MapTypeID is not a number");
                }
            });

            btnTankLevelAdd.onClick.AddListener(() =>
            {
                GameMainLogic.Instance.Player1Entity.AddLevel();
            });

            btnTankLevelSub.onClick.AddListener(() =>
            {
                GameMainLogic.Instance.Player1Entity.SubLevel();
            });

            btnGenerateEntmyTank.onClick.AddListener(() =>
            {
                GameEntry.Pool.GetPool(GameMainLogic.Instance.PoolIdTankEnemy).GetEntity();
            });

            btnGameParse.onClick.AddListener(() =>
            {
                GameMainLogic.Instance.GameParse();
            });
        }

        protected override void UnRegisterEvent()
        {
            btnRegenerateMap.onClick.RemoveAllListeners();
            btnTankLevelAdd.onClick.RemoveAllListeners();
            btnTankLevelSub.onClick.RemoveAllListeners();
        }

    }
}
