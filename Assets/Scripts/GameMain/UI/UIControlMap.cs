using MFramework.Runtime;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

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
            await GeneragetEnemyTank();

            GameEntry.Event.DispatchEvent(GameEventType.GameStart);
            isGenerateMap = false;
        }

        private async UniTask GeneragetPlayerTank()
        {
            model = (UIModelMap)Model;

            var player1 = await GameEntry.Resource.InstantiateAsset("Assets/Download/prefab/entity/tank/Player1.prefab", false);
            player1.gameObject.SetActive(false);
            player1.transform.SetParent(NodePlayer.transform);
            player1.transform.localPosition = new Vector3(model.GridPosBornPlayer1.x, 0, model.GridPosBornPlayer1.y);
            player1.AddComponent<PlayerEntity>();
            player1.name = "EntityPlayer1";
            player1.gameObject.SetActive(true);
        }

        private async UniTask GeneragetEnemyTank()
        {
            model = (UIModelMap)Model;

            var enemy = await GameEntry.Resource.InstantiateAsset("Assets/Download/prefab/entity/tank/Enemy.prefab", false);
            enemy.gameObject.SetActive(false);
            enemy.transform.SetParent(NodeEnemy.transform);
            enemy.transform.localPosition = new Vector3(model.GridPosBornEnemyArr[0].x, 0, model.GridPosBornEnemyArr[0].y);
            enemy.AddComponent<EnemyEntity>();
            enemy.gameObject.SetActive(true);
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
