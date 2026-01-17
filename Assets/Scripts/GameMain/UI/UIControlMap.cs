using Cysharp.Threading.Tasks;
using GameMain.Generate.FlatBuffers;
using MFramework.Runtime;
using MFramework.Runtime.Extend;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D;

namespace GameMain
{
    public class UIControlMap : UIControllerBase
    {
        private UIModelMap model;
        private UIPanelMap view;

        //private bool isGenerateMap = false;

        private GameObject NodeMap;
        private GameObject NodePlayer;
        private GameObject NodeEnemy;
        private GameObject NodeBorder;
        private GameObject NodeBomb;

        private Transform MapNode2D;

        private FB_stage_stage m_StageData;

        private SpriteAtlas m_ItemAtlas;

        public override async UniTask Init(IUIView view, IUIModel model)
        {
            await base.Init(view, model);
            model = (UIModelMap)Model;
            view = (UIPanelMap)View;
            GameMainLogic.Instance = new GameMainLogic();

            GameEntry.Event.RegisterEvent(GameEventType.GameStart, OnGameStart);
            GameEntry.Event.RegisterEvent(GameEventType.GameSettlement, OnGameSettlement);
            GameEntry.Event.RegisterEvent<int>(GameEventType.BirdChangeStore, OnBirdChangeStore);

            await GameMainLogic.Instance.Init();
            await InitMapEntity();
        }

        private void OnGameStart()
        {
            GameMainLogic.Instance.GameStateType = GameStateType.GameRunning;
            GeneragetPlayerTank();
#pragma warning disable CS4014
            AutoGeneragetEnemyTank(GameMainLogic.Instance.RemainEnemyTankNum);
#pragma warning restore CS4014
        }

        private async UniTask InitMapEntity()
        {
            MapNode2D = GameObject.Find("MapNode2D").transform;
            MapNode2D.SetParent(GameMainLogic.Instance.RootNode);
            await GenerateMapFirstStage();
            Debugger.Log("InitMapEntity Completed ", LogType.Test);
        }

        private void OnGameSettlement()
        {
            GameEntry.Pool.GetPool(GameMainLogic.Instance.PoolIdTankEnemy).RecycleAllEntity();
            GameEntry.Pool.GetPool(GameMainLogic.Instance.PoolIdTankPlayer).RecycleAllEntity();
            GameEntry.Pool.GetPool(GameMainLogic.Instance.PoolIdReward).RecycleAllEntity();
        }


        #region GenerateMap
        public async UniTask GenerateMapNextStage()
        {
            await GenerateMapByStageID(++GameMainLogic.Instance.StageID);
        }

        public async UniTask GenerateMapFirstStage()
        {
            GameMainLogic.Instance.StageID = 1;
            await GenerateMapByStageID(GameMainLogic.Instance.StageID);
        }


        public async UniTask GenerateMapByStageID(int stageID)
        {
            int mapTypeID = DataTools.GetMapTypeIDByStageID(stageID);
            GameMainLogic.Instance.RemainEnemyTankNum = DataTools.GetStageData(stageID).EnemyTankNum;
            await GenerateMapByMapTypeID(mapTypeID);
        }

        public async UniTask GenerateMapByMapTypeID(int mapTypeID)
        {
            if (GameMainLogic.Instance.GameStateType == GameStateType.GameMapGenerating)
            {
                return;
            }
            if (mapTypeID <= 0 || mapTypeID > DataTools.GetMapTypeList().Count)
            {
                mapTypeID = 1;
            }
            GameMainLogic.Instance.GameStateType = GameStateType.GameMapGenerating;

            ((UIModelMap)Model).GenerateMapData(mapTypeID);
            ResetNodeContainer();
            m_StageData = DataTools.GetStageData(GameMainLogic.Instance.StageID);
            await GenerateMapEntityByDataAsync();
            await GenerateMapAirBorder();
            await GameEntry.UI.ShowViewAsync<UIPanelBattle>();

            GameMainLogic.Instance.GameStateType = GameStateType.GameMapGenerated;


            //await GeneragetFirstEnemyTank(1);
        }
        #endregion

        private void GeneragetPlayerTank()
        {
            GameEntry.Pool.GetPool(GameMainLogic.Instance.PoolIdTankPlayer).GetEntity();
        }

        private async UniTask AutoGeneragetEnemyTank(int count)
        {
            var enemyTankAtlas = await GameEntry.Resource.LoadAssetAsync<SpriteAtlas>(SystemConstantData.PATH_PREFAB_TEXTURE_ATLAS_ROOT + "enemyTankAtlas.spriteatlas", false);

            for (int i = 0; i < count; i++)
            {
                if (GameMainLogic.Instance.GameStateType == GameStateType.GameRunning)
                {
                    GameEntry.Pool.GetPool(GameMainLogic.Instance.PoolIdTankEnemy).GetEntity();
                    await UniTask.Delay(1000);
                }
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
                    go.GetOrAddComponent<MapEntity>().SetMapEntityType(EMapEntityType.AirBorder);
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


        public async void OnBirdChangeStore(int duration)
        {
            if (m_ItemAtlas == null)
            {
                m_ItemAtlas = await GameEntry.Resource.LoadAssetAsync<SpriteAtlas>(SystemConstantData.PATH_PREFAB_TEXTURE_ATLAS_ROOT + "itemAtlas.spriteatlas", false);
            }

            for (int i = 0; i < model.GridPosBridWallArr.Count; i++)
            {
                GridDataInfo gridDataInfo = model.GetMapGridDataInfo(model.GridPosBridWallArr[i]);
                for (int j = 0; j < gridDataInfo.entityDataInfos.Count; j++)
                {
                    MapEntity mapEntity = gridDataInfo.entityDataInfos[j].propEntity.GetComponent<MapEntity>();
                    EMapEntityType targetType = EMapEntityType.None;
                    switch (mapEntity.mapEntityType)
                    {
                        case EMapEntityType.Wall:
                            targetType = EMapEntityType.Stone;
                            break;
                        case EMapEntityType.Stone:
                            targetType = EMapEntityType.Wall;
                            break;
                        case EMapEntityType.Wall_LU:
                            targetType = EMapEntityType.Stone_LU;
                            break;
                        case EMapEntityType.Wall_LD:
                            targetType = EMapEntityType.Stone_LD;
                            break;
                        case EMapEntityType.Wall_RU:
                            targetType = EMapEntityType.Stone_RU;
                            break;
                        case EMapEntityType.Wall_RD:
                            targetType = EMapEntityType.Stone_RD;
                            break;
                        case EMapEntityType.Stone_LU:
                            targetType = EMapEntityType.Wall_LU;
                            break;
                        case EMapEntityType.Stone_LD:
                            targetType = EMapEntityType.Wall_LD;
                            break;
                        case EMapEntityType.Stone_RU:
                            targetType = EMapEntityType.Wall_RU;
                            break;
                        case EMapEntityType.Stone_RD:
                            targetType = EMapEntityType.Wall_RD;
                            break;
                    }
                    mapEntity.SetMapEntityType(targetType);
                    mapEntity.SetSprite(targetType, m_ItemAtlas);
                }
            }
        }
    }
}
