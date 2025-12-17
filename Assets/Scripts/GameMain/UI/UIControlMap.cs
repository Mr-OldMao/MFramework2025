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
            await GeneragetPlayerTank();
            await GeneragetEnemyTank();
            isGenerateMap = false;
        }

        private async UniTask GeneragetPlayerTank()
        {
            model = (UIModelMap)Model;
            
            var player1 = await GameEntry.Resource.InstantiateAsset("Assets/Download/prefab/entity/tank/Player1.prefab", false);
            player1.transform.SetParent(rectNodePlayer.transform);
            player1.transform.localPosition = model.GridPosBornPlayer1 * UIModelMap.GRID_SIZE;
        }

        private async UniTask GeneragetEnemyTank()
        {
            model = (UIModelMap)Model;

            var enemy = await GameEntry.Resource.InstantiateAsset("Assets/Download/prefab/entity/tank/Enemy.prefab", false);
            enemy.transform.SetParent(rectNodeEnemy.transform);
            enemy.transform.localPosition = model.GridPosBornEnemyArr[0] * UIModelMap.GRID_SIZE;
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
                        var go = await GameEntry.Resource.InstantiateAsset(assetPath);
                        go.transform.SetParent(rectNodeMap.transform);
                        go.transform.localPosition = gridDataInfos[i].mapPos;
                        gridDataInfos[i].entityDataInfos[j].propEntity = go;
                    }
                }
            }
        }

        private void ResetNodeContainer()
        {
            view = (UIPanelMap)View;

            rectNodeMap = view.NodeContainer.Find("rectNodeMap")?.gameObject;
            if (rectNodeMap != null)
            {
                GameObject.Destroy(rectNodeMap);
            }
            rectNodeMap = new GameObject("rectNodeMap");
            rectNodeMap.transform.SetParent(view.NodeContainer);
            rectNodeMap.transform.localPosition = Vector3.zero;

            rectNodePlayer = view.NodeContainer.Find("rectNodePlayer")?.gameObject;
            if (rectNodePlayer != null)
            {
                GameObject.Destroy(rectNodePlayer);
            }
            rectNodePlayer = new GameObject("rectNodePlayer");
            rectNodePlayer.transform.SetParent(view.NodeContainer);
            rectNodePlayer.transform.localPosition = Vector3.zero;

            rectNodeEnemy = view.NodeContainer.Find("rectNodeEnemy")?.gameObject;
            if (rectNodeEnemy != null)
            {
                GameObject.Destroy(rectNodeEnemy);
            }
            rectNodeEnemy = new GameObject("rectNodeEnemy");
            rectNodeEnemy.transform.SetParent(view.NodeContainer);
            rectNodeEnemy.transform.localPosition = Vector3.zero;
        }
    }
}
