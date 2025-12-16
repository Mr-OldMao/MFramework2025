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

        public Button btnRegenerateMap;

        public override async UniTask Init()
        {
            await base.Init();
            //var dataReward = DataTools.GetRewardReward("RewardBomb");
            //var stone = await GameEntry.Resource.InstantiateAsset("Assets/Download/prefab/entity/map/stone/Stone.prefab", false);
            //stone.transform.SetParent(nodeOther);
            //stone.transform.localPosition = Vector3.zero;

            //var homeWall = await GameEntry.Resource.InstantiateAsset("Assets/Download/prefab/entity/map/MapHomeWall.prefab", false);
            //homeWall.transform.SetParent(nodeHomeWall);
            //homeWall.transform.localPosition = Vector3.zero;

            //var player1 = await GameEntry.Resource.InstantiateAsset("Assets/Download/prefab/entity/tank/Player1.prefab", false);
            //player1.transform.SetParent(nodeBornPlayer1);
            //player1.transform.localPosition = Vector3.zero;

            //var enemy = await GameEntry.Resource.InstantiateAsset("Assets/Download/prefab/entity/tank/Enemy.prefab", false);
            //enemy.transform.SetParent(nodeBornEnemy1);
            //enemy.transform.localPosition = Vector3.zero;

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
            btnRegenerateMap.onClick.AddListener(() =>
            {
#pragma warning disable CS4014 
                (Controller as UIControlMap).GenerateMap();
#pragma warning restore CS4014
            });
        }

        protected override void UnRegisterEvent()
        {
            btnRegenerateMap.onClick.RemoveAllListeners();
        }

        // 私有方法
    }
}
