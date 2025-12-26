using MFramework.Runtime;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using MFramework.Runtime.Extend;

namespace GameMain
{
    public class UIControlMap : UIControllerBase
    {
        private UIModelMap model;
        private UIPanelMap view;

        private bool isGenerateMap = false;

        private GameObject NodeMap;
        private GameObject NodePlayer;
        private GameObject NodeEnemy;
        private GameObject NodeBorder;
        private GameObject NodeBomb;

        private Transform MapNode2D;


        private int m_CurEnemyPlayerID;
        private int m_CurEnemyEntityID;

        public override async UniTask Init(IUIView view, IUIModel model)
        {
            await base.Init(view, model);
            model = (UIModelMap)Model;
            view = (UIPanelMap)View;

            GameMainLogic.Instance = new GameMainLogic();
            await GameMainLogic.Instance.Init();
            await InitMapEntity();
        }

        private async UniTask InitMapEntity()
        {
            int mapTypeID = DataTools.GetMapTypeIDByLevelID(((UIModelMap)Model).LevelID);

            MapNode2D = GameObject.Find("MapNode2D").transform;
            MapNode2D.SetParent(GameMainLogic.Instance.RootNode);
            await GenerateMap(mapTypeID);
            Debugger.Log("InitMapEntity Completed ", LogType.Test);
        }
        public async UniTask GenerateMap(int mapTypeID = 1)
        {
            if (isGenerateMap)
            {
                return;
            }
            if (mapTypeID <= 0 || mapTypeID > DataTools.GetMapTypeList().Count)
            {
                mapTypeID = 1;
            }

            isGenerateMap = true;
            ((UIModelMap)Model).GenerateMapData(mapTypeID);

            ResetNodeContainer();
            await GenerateMapEntityByDataAsync();
            await GenerateMapAirBorder();
            await GeneragetPlayerTank();
            await GeneragetFirstEnemyTank(1);

            GameEntry.Event.DispatchEvent(GameEventType.GameStart);
            isGenerateMap = false;

//#pragma warning disable CS4014
//            AutoGeneragetEnemyTank(5);
//#pragma warning restore CS4014
        }

        private async UniTask GeneragetPlayerTank()
        {
            model = (UIModelMap)Model;

            var player1 = await GameEntry.Resource.InstantiateAsset("Assets/Download/prefab/entity/tank/Player1.prefab", false);
            player1.gameObject.SetActive(true);
            //player1.transform.SetParent(NodePlayer.transform);
            //player1.transform.localPosition = new Vector3(model.GridPosBornPlayer1.x, 0, model.GridPosBornPlayer1.y);

            m_CurEnemyPlayerID = 1000;
            int tankTypeID = Random.Range(101, 105);
            player1.AddComponent<PlayerEntity>().InitData(TankOwnerType.Player1, tankTypeID, ++m_CurEnemyPlayerID);
            GameMainLogic.Instance.Player1Entity = player1.GetComponent<PlayerEntity>();
            player1.name = "EntityPlayer1";
        }

        private async UniTask GeneragetFirstEnemyTank(int count)
        {
            model = (UIModelMap)Model;
            m_CurEnemyEntityID = 1000;
            await AutoGeneragetEnemyTank(count);
        }

        private async UniTask AutoGeneragetEnemyTank(int count)
        {
            var enemyTankAtlas = await GameEntry.Resource.LoadAssetAsync<SpriteAtlas>(SystemConstantData.PATH_PREFAB_TEXTURE_ATLAS_ROOT + "enemyTankAtlas.spriteatlas", false);


            for (int i = 0; i < count; i++)
            {
                var enemy = GameMainLogic.Instance.GetPoolTankEnemy();
                await UniTask.Delay(1000);
            }
        }

        private async UniTask GenerateMapAirBorder()
        {
            NodeBorder = MapNode2D.Find("NodeBorder")?.gameObject;
            if (NodeBorder != null)
            {
                return;
            }
            NodeBorder = new GameObject("NodeBorder");
            NodeBorder.transform.SetParent(MapNode2D);
            NodeBorder.transform.localPosition = Vector3.zero;

            model = (UIModelMap)Model;
            List<Vector2> borderPos = model.GetAirBorder();
            for (int i = 0; i < borderPos.Count; i++)
            {
                string assetPath = model.GetMapPropAssetPath(EMapEntityType.AirBorder);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    var go = await GameEntry.Resource.InstantiateAsset(assetPath, false);
                    go.transform.SetParent(NodeBorder.transform);
                    go.transform.localPosition = new Vector3(borderPos[i].x, 0, borderPos[i].y);
                }
            }
        }

        private async UniTask GenerateMapEntityByDataAsync()
        {
            model = (UIModelMap)Model;
            List<GridDataInfo> gridDataInfos = model.GetMapGridData();

            for (int i = 0; i < gridDataInfos.Count; i++)
            {
                for (int j = 0; j < gridDataInfos[i]?.entityDataInfos?.Count; j++)
                {
                    string assetPath = model.GetMapPropAssetPath(gridDataInfos[i].entityDataInfos[j].mapEntityType);
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        var go = await GameEntry.Resource.InstantiateAsset(assetPath, false);
                        go.SetActive(false);
                        go.transform.SetParent(NodeMap.transform);
                        go.transform.localPosition = new Vector3(gridDataInfos[i].gridPos.x, 0, gridDataInfos[i].gridPos.y);
                        gridDataInfos[i].entityDataInfos[j].propEntity = go;
                        go.AddComponent<MapEntity>().SetMapEntityType(gridDataInfos[i].entityDataInfos[j].mapEntityType);
                        go.SetActive(true);
                        go.name = $"{gridDataInfos[i].entityDataInfos[j].mapEntityType}_{gridDataInfos[i].gridPos.x}_{gridDataInfos[i].gridPos.y}";
                    }
                }
            }
        }

        private void ResetNodeContainer()
        {
            view = (UIPanelMap)View;

            NodeMap = MapNode2D.Find("NodeMap")?.gameObject;
            if (NodeMap != null)
            {
                GameObject.Destroy(NodeMap);
            }
            NodeMap = new GameObject("NodeMap");
            NodeMap.transform.SetParent(MapNode2D);
            NodeMap.transform.localPosition = Vector3.zero;

            NodePlayer = MapNode2D.Find("NodePlayer")?.gameObject;
            if (NodePlayer != null)
            {
                GameObject.Destroy(NodePlayer);
            }
            NodePlayer = new GameObject("NodePlayer");
            NodePlayer.transform.SetParent(MapNode2D);
            NodePlayer.transform.localPosition = Vector3.zero;

            NodeEnemy = MapNode2D.Find("NodeEnemy")?.gameObject;
            if (NodeEnemy != null)
            {
                GameObject.Destroy(NodeEnemy);
            }
            NodeEnemy = new GameObject("NodeEnemy");
            NodeEnemy.transform.SetParent(MapNode2D);
            NodeEnemy.transform.localPosition = Vector3.zero;


            NodeBomb = MapNode2D.Find("NodeBomb")?.gameObject;
            if (NodeBomb != null)
            {
                GameObject.Destroy(NodeBomb);
            }
            NodeBomb = new GameObject("NodeBomb");
            NodeBomb.transform.SetParent(MapNode2D);
            NodeBomb.transform.localPosition = Vector3.zero;
        }
    }
}
