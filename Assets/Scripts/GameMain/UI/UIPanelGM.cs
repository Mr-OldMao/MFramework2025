using MFramework.Runtime;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameMain
{
    //[UIBind(typeof(UIControlGM), typeof(UIModelGM))]
    [UILayer(UILayerType.Tips)]
    public  class UIPanelGM : UIViewBase
    {
        // UI字段
        private RectTransform rootNode;
        private TMP_InputField inputTxtMapTypeID;
        private Button btnRegenerateMap;
        private Button btnTankLevelAdd;
        private Button btnTankLevelSub;
        private Button btnGenerateEntmyTank;



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
            PlayerEntity = GameObject.Find("EntityPlayer1").GetComponent<PlayerEntity>();

            GameEntry.Event.RegisterEvent(GameEventType.GameStart, () =>
            {
                PlayerEntity = GameObject.Find("EntityPlayer1").GetComponent<PlayerEntity>();
            });

            btnRegenerateMap.onClick.AddListener(() =>
            {
                if (int.TryParse(inputTxtMapTypeID.text , out int mapTypeID))
                {
#pragma warning disable CS4014
                    GameEntry.UI.GetController<UIControlMap>().GenerateMap(mapTypeID);
#pragma warning restore CS4014
                }
                else
                {
                    Debugger.LogError("MapTypeID is not a number");
                }
            });

            btnTankLevelAdd.onClick.AddListener(() =>
            {
                PlayerEntity.AddLevel();
            });

            btnTankLevelSub.onClick.AddListener(() =>
            {
                PlayerEntity.SubLevel();
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
