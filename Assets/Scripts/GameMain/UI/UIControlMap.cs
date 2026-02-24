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


        private int m_TimerIDBridStoneWall;
        private int m_EnemyGenerateMaxCount;
        private int m_EnemyGenerateIntervalMS;
        public override async UniTask Init(IUIView view, IUIModel model)
        {
            await base.Init(view, model);
            model = (UIModelMap)Model;
            view = (UIPanelMap)View;
            GameMainLogic.Instance = new GameMainLogic();

            GameEntry.Event.RegisterEvent(GameEventType.GameStart, OnGameStart);
            GameEntry.Event.RegisterEvent(GameEventType.GameSettlement, OnGameSettlement);
            GameEntry.Event.RegisterEvent<float>(GameEventType.BirdChangeStore, OnBirdChangeStore);

            m_EnemyGenerateMaxCount = DataTools.GetConst("EnemyGenerateMaxCount");
            m_EnemyGenerateIntervalMS = DataTools.GetConst("EnemyGenerateIntervalMS");
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
        public async UniTask GenerateMapNextStage(bool isGenerateNormalMap = true)
        {
            if (isGenerateNormalMap)
            {
                await GenerateNormalStageByMapTypeID(++GameMainLogic.Instance.StageID);
            }
            else
            {
                await GenerateRandomMapByStageID(++GameMainLogic.Instance.StageID);
            }
        }

        public async UniTask GenerateMapFirstStage(bool isGenerateNormalMap = true)
        {
            GameMainLogic.Instance.StageID = 1;

            if (isGenerateNormalMap)
            {
                await GenerateNormalStageByMapTypeID(GameMainLogic.Instance.StageID);

            }
            else
            {
                await GenerateRandomMapByStageID(GameMainLogic.Instance.StageID);
            }
        }

        /// <summary>
        /// 生成经典关卡地图
        /// </summary>
        /// <param name="stageID"></param>
        /// <returns></returns>
        public async UniTask GenerateNormalStageByMapTypeID(int stageID)
        {
            GameMainLogic.Instance.RemainEnemyTankNum = DataTools.GetStageData(stageID).EnemyTankNum;
            if (GameMainLogic.Instance.GameStateType == GameStateType.GameMapGenerating)
            {
                return;
            }
            if (stageID <= 0)
            {
                stageID = 1;
            }
            GameMainLogic.Instance.GameStateType = GameStateType.GameMapGenerating;

            ((UIModelMap)Model).GenerateNormalMapData(stageID);
            ResetNodeContainer();
            m_StageData = DataTools.GetStageData(GameMainLogic.Instance.StageID);
            await GenerateMapEntityByDataAsync();
            await GenerateMapAirBorder();
            await GameEntry.UI.ShowViewAsync<UIPanelBattle>();

            GameMainLogic.Instance.GameStateType = GameStateType.GameMapGenerated;
        }


        /// <summary>
        /// 生成随机地图
        /// </summary>
        /// <param name="stageID"></param>
        /// <returns></returns>
        public async UniTask GenerateRandomMapByStageID(int stageID)
        {
            int mapTypeID = DataTools.GetMapTypeIDByStageID(stageID);
            GameMainLogic.Instance.RemainEnemyTankNum = DataTools.GetStageData(stageID).EnemyTankNum;
            await GenerateRandomMapByMapTypeID(mapTypeID);
        }

        /// <summary>
        /// 生成随机地图
        /// </summary>
        /// <param name="mapTypeID"></param>
        /// <returns></returns>
        public async UniTask GenerateRandomMapByMapTypeID(int mapTypeID)
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

            ((UIModelMap)Model).GenerateRandomMapData(mapTypeID);
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

            for (int i = 0; i < count;)
            {
                if (GameMainLogic.Instance.GameStateType == GameStateType.GameRunning)
                {
                    int enemyEntityCount = GameEntry.Pool.GetPool(GameMainLogic.Instance.PoolIdTankEnemy).UsedCount;
                    if (enemyEntityCount < m_EnemyGenerateMaxCount )
                    {
                        GameEntry.Pool.GetPool(GameMainLogic.Instance.PoolIdTankEnemy).GetEntity();
                        i++;
                    }
                    await UniTask.Delay(m_EnemyGenerateIntervalMS);
                }
                else
                {
                    break;
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
                    Vector2 gridPos = borderPos[i];
                    go.GetOrAddComponent<MapEntity>().SetMapEntityType(gridPos, EMapEntityType.AirBorder);
                    go.transform.SetParent(NodeBorder.transform);
                    go.transform.localPosition = new Vector3(gridPos.x, 0, gridPos.y);
                }
            }
        }

        private async UniTask GenerateMapEntityByDataAsync()
        {
            model = (UIModelMap)Model;
            List<GridDataInfo> gridDataInfos = model.GetMapGridData();
            await GenerateMapEntity(gridDataInfos);
        }

        private async UniTask GenerateMapEntity(List<GridDataInfo> gridDataInfos)
        {
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
                        go.AddComponent<MapEntity>().SetMapEntityType(gridDataInfos[i].gridPos, gridDataInfos[i].entityDataInfos[j].mapEntityType);
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


        public async void OnBirdChangeStore(float duration)
        {
            if (m_TimerIDBridStoneWall > 0)
            {
                GameEntry.Timer.RemoveDelayTimer(m_TimerIDBridStoneWall);
            }

            FixedBridWall();
            ChangeBirdWallToStone();
            m_TimerIDBridStoneWall = GameEntry.Timer.AddDelayTimer(duration, () =>
            {
                ChangeBirdStoneToWall();
                m_TimerIDBridStoneWall = 0;
            });
            return;
        }


        public void FixedBridWall()
        {
            for (int i = 0; i < model.GridPosBridWallArr.Count; i++)
            {
                GridDataInfo gridDataInfo = model.GetMapGridDataInfo(model.GridPosBridWallArr[i]);
                for (int j = 0; j < gridDataInfo.entityDataInfos.Count; j++)
                {
                    MapEntity mapEntity = gridDataInfo.entityDataInfos[j].propEntity.GetComponent<MapEntity>();
                    if (mapEntity != null)
                    {
                        mapEntity.FixedBridWall();
                    }
                }
            }
        }

        public void ChangeBirdWallToStone()
        {
            for (int i = 0; i < model.GridPosBridWallArr.Count; i++)
            {
                GridDataInfo gridDataInfo = model.GetMapGridDataInfo(model.GridPosBridWallArr[i]);
                for (int j = 0; j < gridDataInfo.entityDataInfos.Count; j++)
                {
                    MapEntity mapEntity = gridDataInfo.entityDataInfos[j].propEntity.GetComponent<MapEntity>();
                    if (mapEntity != null)
                    {
                        mapEntity.ChangeBirdWallToStone();
                    }
                }
            }
        }
        public void ChangeBirdStoneToWall()
        {
            for (int i = 0; i < model.GridPosBridWallArr.Count; i++)
            {
                GridDataInfo gridDataInfo = model.GetMapGridDataInfo(model.GridPosBridWallArr[i]);
                for (int j = 0; j < gridDataInfo.entityDataInfos.Count; j++)
                {
                    MapEntity mapEntity = gridDataInfo.entityDataInfos[j].propEntity.GetComponent<MapEntity>();
                    if (mapEntity != null)
                    {
                        mapEntity.ChangeBirdStoneToWall();
                    }
                }
            }
        }
    }
}
