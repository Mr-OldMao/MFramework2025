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

        private GameObject rectNodeMap;
        private GameObject rectNodePlayer;
        private GameObject rectNodeEnemy;
        private GameObject rectNodeBorder;

        private Transform MapNode2D;

        public override async UniTask Init(IUIView view, IUIModel model)
        {
            await base.Init(view, model);
            model = (UIModelMap)Model;
            view = (UIPanelMap)View;
            await InitMapEntity();
        }

        private async UniTask InitMapEntity()
        {
            int mapTypeID = DataTools.GetMapTypeIDByLevelID(((UIModelMap)Model).LevelID);

            MapNode2D = GameObject.Find("MapNode2D").transform;
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
            isGenerateMap = false;
        }

        private async UniTask GeneragetPlayerTank()
        {
            model = (UIModelMap)Model;

            var player1 = await GameEntry.Resource.InstantiateAsset("Assets/Download/prefab/entity/tank/Player1.prefab", false);
            player1.gameObject.SetActive(false);
            player1.transform.SetParent(rectNodePlayer.transform);
            player1.transform.localPosition = new Vector3(model.GridPosBornPlayer1.x, 0, model.GridPosBornPlayer1.y);
            player1.gameObject.SetActive(true);
        }

        private async UniTask GeneragetEnemyTank()
        {
            model = (UIModelMap)Model;

            var enemy = await GameEntry.Resource.InstantiateAsset("Assets/Download/prefab/entity/tank/Enemy.prefab", false);
            enemy.gameObject.SetActive(false);
            enemy.transform.SetParent(rectNodeEnemy.transform);
            enemy.transform.localPosition = new Vector3(model.GridPosBornEnemyArr[0].x, 0, model.GridPosBornEnemyArr[0].y);
            enemy.gameObject.SetActive(true);
        }

        private async UniTask GenerateMapAirBorder()
        {
            rectNodeBorder = MapNode2D.Find("rectNodeBorder")?.gameObject;
            if (rectNodeBorder != null)
            {
                return;
            }
            rectNodeBorder = new GameObject("rectNodeBorder");
            rectNodeBorder.transform.SetParent(MapNode2D);
            rectNodeBorder.transform.localPosition = Vector3.zero;

            model = (UIModelMap)Model;
            List<Vector2> borderPos = model.GetAirBorder();
            for (int i = 0; i < borderPos.Count; i++)
            {
                string assetPath = model.GetMapPropAssetPath(EMapEntityType.AirBorder);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    var go = await GameEntry.Resource.InstantiateAsset(assetPath, false);
                    go.transform.SetParent(rectNodeBorder.transform);
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
                        var go = await GameEntry.Resource.InstantiateAsset(assetPath,false);
                        go.SetActive(false);
                        go.transform.SetParent(rectNodeMap.transform);
                        go.transform.localPosition = new Vector3(gridDataInfos[i].gridPos.x, 0, gridDataInfos[i].gridPos.y);
                        gridDataInfos[i].entityDataInfos[j].propEntity = go;
                        go.SetActive(true);
                    }
                }
            }


        }

        private void ResetNodeContainer()
        {
            view = (UIPanelMap)View;

            rectNodeMap = MapNode2D.Find("rectNodeMap")?.gameObject;
            if (rectNodeMap != null)
            {
                GameObject.Destroy(rectNodeMap);
            }
            rectNodeMap = new GameObject("rectNodeMap");
            rectNodeMap.transform.SetParent(MapNode2D);
            rectNodeMap.transform.localPosition = Vector3.zero;

            rectNodePlayer = MapNode2D.Find("rectNodePlayer")?.gameObject;
            if (rectNodePlayer != null)
            {
                GameObject.Destroy(rectNodePlayer);
            }
            rectNodePlayer = new GameObject("rectNodePlayer");
            rectNodePlayer.transform.SetParent(MapNode2D);
            rectNodePlayer.transform.localPosition = Vector3.zero;

            rectNodeEnemy = MapNode2D.Find("rectNodeEnemy")?.gameObject;
            if (rectNodeEnemy != null)
            {
                GameObject.Destroy(rectNodeEnemy);
            }
            rectNodeEnemy = new GameObject("rectNodeEnemy");
            rectNodeEnemy.transform.SetParent(MapNode2D);
            rectNodeEnemy.transform.localPosition = Vector3.zero;
        }
    }
}
