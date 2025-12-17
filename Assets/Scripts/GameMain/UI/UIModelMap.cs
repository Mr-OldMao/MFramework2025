using Cysharp.Threading.Tasks;
using GameMain.Generate.FlatBuffers;
using MFramework.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameMain
{
    public class UIModelMap : UIModelBase
    {
        public const int ROW_NUM = 13;
        public const int COLUMN_NUM = 13;
        /// <summary>
        /// 坐标映射比例 物理坐标 = 1格子坐标gridPos * posMappingRatio
        /// </summary>
        public const int POS_MAPPING_RATIO = 80;

        //地图网格数据
        private Dictionary<Vector2, GridDataInfo> m_DicMapGridData = new Dictionary<Vector2, GridDataInfo>();

        //网格边界
        private Dictionary<Vector2, EMapGridBorderType> m_DicMapGridBorderType = new Dictionary<Vector2, EMapGridBorderType>();

        public int LevelID { get; private set; }

        public List<Vector2> PosBornEnemyArr { get; private set; }
        public Vector2 PosBornPlayer1 { get; private set; }
        public Vector2 PosBornPlayer2 { get; private set; }
        public Vector2 PosBrid { get; private set; }
        //左、左上、上、右上、右
        public List<Vector2> PosBridWall { get; private set; }

        public UIModelMap(IUIController controller) : base(controller)
        {

        }
        public override async UniTask Init()
        {
            LevelID = 1;
            SetMapGridType();
            SetFixedPos();
            await UniTask.CompletedTask;
        }

        private void SetFixedPos()
        {
            PosBornEnemyArr = new List<Vector2>
            {
                m_DicMapGridBorderType.Where(p => p.Value == EMapGridBorderType.Border_LU).FirstOrDefault().Key,
                m_DicMapGridBorderType.Where(p => p.Value == EMapGridBorderType.Border_UM).FirstOrDefault().Key,
                m_DicMapGridBorderType.Where(p => p.Value == EMapGridBorderType.Border_RU).FirstOrDefault().Key
            };
            PosBrid = m_DicMapGridBorderType.Where(p => p.Value == EMapGridBorderType.Border_DM).FirstOrDefault().Key;
            PosBridWall = new List<Vector2>
            {
                new Vector2(PosBrid.x - 1,PosBrid.y),
                new Vector2(PosBrid.x - 1,PosBrid.y + 1),
                new Vector2(PosBrid.x,PosBrid.y + 1),
                new Vector2(PosBrid.x + 1,PosBrid.y + 1),
                new Vector2(PosBrid.x + 1,PosBrid.y),
            };
            PosBornPlayer1 = new Vector2(PosBrid.x - 2, PosBrid.y);
            PosBornPlayer2 = new Vector2(PosBrid.x + 2, PosBrid.y);
        }

        public void GenerateMapData(int mapTypeID)
        {
            SetMapGridData(mapTypeID);
        }

        private void SetMapGridData(int mapTypeID)
        {
            m_DicMapGridData.Clear();
            FB_map_mapType fB_Map_MapType = DataTools.GetMapType(mapTypeID);
            for (int i = 0; i < COLUMN_NUM; i++)
            {
                for (int j = 0; j < ROW_NUM; j++)
                {
                    Vector2 gridPos = new Vector2(i, j);
                    GridDataInfo gridDataInfo = new GridDataInfo
                    {
                        gridPos = gridPos,
                        mapPos = gridPos * POS_MAPPING_RATIO,
                        mapGridType = GetMapGridType(gridPos),
                        entityDataInfos = SetEntityDataInfos(gridPos, fB_Map_MapType)
                    };
                    m_DicMapGridData.Add(gridPos, gridDataInfo);
                }
            }

#if UNITY_EDITOR
            string des = string.Empty;
            for (int i = 0; i <  Enum.GetValues(typeof(EMapEntityType)).Length; i++)
            {
               var entityTypeArr = m_DicMapGridData.Values.Where(p => p.entityDataInfos.Find(k => k.mapEntityType == (EMapEntityType)i) != null).ToList();
                if (entityTypeArr.Count > 0)
                {
                    des += $"{(EMapEntityType)i}:{entityTypeArr.Count}\n";
                }
            }
            Debugger.Log($"---------------------- GenerateMapData，Des:{des}", LogType.Test);

#endif
            Debugger.Log("SetMapGridData Completed ", LogType.Test);
        }

        private List<EntityDataInfo> SetEntityDataInfos(Vector2 gridID, FB_map_mapType fB_Map_MapType)
        {
            List<EntityDataInfo> entityDataInfos = new List<EntityDataInfo>();
            if (PosBornEnemyArr.Contains(gridID) || PosBornPlayer1 == gridID || PosBornPlayer2 == gridID)
            {
                entityDataInfos.Add(new EntityDataInfo
                {
                    mapEntityType = EMapEntityType.None,
                    propEntity = null,
                });
            }
            else if (PosBrid == gridID)
            {
                entityDataInfos.Add(new EntityDataInfo
                {
                    mapEntityType = EMapEntityType.Brid,
                    propEntity = null,
                });
            }
            else if (PosBridWall.Contains(gridID))
            {
                if (gridID == PosBridWall.ElementAt(0))
                {
                    entityDataInfos.Add(new EntityDataInfo
                    {
                        mapEntityType = EMapEntityType.Wall_RU,
                        propEntity = null,
                    });
                    entityDataInfos.Add(new EntityDataInfo
                    {
                        mapEntityType = EMapEntityType.Wall_RD,
                        propEntity = null,
                    });
                }
                else if (gridID == PosBridWall.ElementAt(1))
                {
                    entityDataInfos.Add(new EntityDataInfo
                    {
                        mapEntityType = EMapEntityType.Wall_RD,
                        propEntity = null,
                    });
                }
                else if (gridID == PosBridWall.ElementAt(2))
                {
                    entityDataInfos.Add(new EntityDataInfo
                    {
                        mapEntityType = EMapEntityType.Wall_LD,
                        propEntity = null,
                    });
                    entityDataInfos.Add(new EntityDataInfo
                    {
                        mapEntityType = EMapEntityType.Wall_RD,
                        propEntity = null,
                    });
                }
                else if (gridID == PosBridWall.ElementAt(3))
                {
                    entityDataInfos.Add(new EntityDataInfo
                    {
                        mapEntityType = EMapEntityType.Wall_LD,
                        propEntity = null,
                    });
                }
                else if (gridID == PosBridWall.ElementAt(4))
                {
                    entityDataInfos.Add(new EntityDataInfo
                    {
                        mapEntityType = EMapEntityType.Wall_LD,
                        propEntity = null,
                    });
                    entityDataInfos.Add(new EntityDataInfo
                    {
                        mapEntityType = EMapEntityType.Wall_LU,
                        propEntity = null,
                    });
                }
            }
            else
            {
                entityDataInfos.Add(new EntityDataInfo
                {
                    mapEntityType = GetRandomEntityType(fB_Map_MapType),
                    propEntity = null,
                });
            }
            return entityDataInfos;
        }

        private EMapEntityType GetRandomEntityType(FB_map_mapType fB_Map_MapType)
        {
            EMapEntityType res = EMapEntityType.None;
            float randomValue = Random.Range(0f, 1f);
            if (randomValue < fB_Map_MapType.PWall)
            {
                res = EMapEntityType.Wall;
            }
            else if (randomValue < fB_Map_MapType.PWall + fB_Map_MapType.PStone)
            {
                res = EMapEntityType.Stone;
            }
            else if (randomValue < fB_Map_MapType.PWall + fB_Map_MapType.PStone + fB_Map_MapType.PGress)
            {
                res = EMapEntityType.Grass;
            }
            else if (randomValue < fB_Map_MapType.PWall + fB_Map_MapType.PStone + fB_Map_MapType.PGress + fB_Map_MapType.PWater)
            {
                res = EMapEntityType.Water;
            }
            else if (randomValue < fB_Map_MapType.PWall + fB_Map_MapType.PStone + fB_Map_MapType.PGress + fB_Map_MapType.PWater + fB_Map_MapType.PSnow)
            {
                res = EMapEntityType.Snow;
            }
            return res;
        }

        public List<GridDataInfo> GetMapGridData()
        {
            return m_DicMapGridData.Values.ToList();
        }

        public string GetMapPropAssetPath(EMapEntityType eMapEntityType)
        {
            if (eMapEntityType == EMapEntityType.None)
            {
                return string.Empty;
            }
            return "Assets/Download/prefab/entity/map/" + eMapEntityType.ToString() + ".prefab";
        }

        public EMapGridBorderType GetMapGridType(Vector2 gridPos)
        {
            if (m_DicMapGridBorderType.TryGetValue(gridPos, out EMapGridBorderType res))
            {
                return res;
            }
            return EMapGridBorderType.None;
        }

        private void SetMapGridType()
        {
            for (int i = 0; i < COLUMN_NUM; i++)
            {
                for (int j = 0; j < ROW_NUM; j++)
                {
                    if (i == 0)
                    {
                        if (j == 0)
                        {
                            m_DicMapGridBorderType.Add(new Vector2(i, j), EMapGridBorderType.Border_LD);
                        }
                        else if (j == ROW_NUM - 1)
                        {
                            m_DicMapGridBorderType.Add(new Vector2(i, j), EMapGridBorderType.Border_LU);
                        }
                        else if (j == ROW_NUM / 2)
                        {
                            m_DicMapGridBorderType.Add(new Vector2(i, j), EMapGridBorderType.Border_LM);
                        }
                        else
                        {
                            m_DicMapGridBorderType.Add(new Vector2(i, j), EMapGridBorderType.Border_L);
                        }
                    }
                    else if (i == COLUMN_NUM - 1)
                    {
                        if (j == 0)
                        {
                            m_DicMapGridBorderType.Add(new Vector2(i, j), EMapGridBorderType.Border_RD);
                        }
                        else if (j == ROW_NUM - 1)
                        {
                            m_DicMapGridBorderType.Add(new Vector2(i, j), EMapGridBorderType.Border_RU);
                        }
                        else if (j == ROW_NUM / 2)
                        {
                            m_DicMapGridBorderType.Add(new Vector2(i, j), EMapGridBorderType.Border_RM);
                        }
                        else
                        {
                            m_DicMapGridBorderType.Add(new Vector2(i, j), EMapGridBorderType.Border_R);
                        }
                    }
                    else
                    {
                        if (j == 0)
                        {
                            if (i == COLUMN_NUM / 2)
                            {
                                m_DicMapGridBorderType.Add(new Vector2(i, j), EMapGridBorderType.Border_DM);
                            }
                            else
                            {
                                m_DicMapGridBorderType.Add(new Vector2(i, j), EMapGridBorderType.Border_D);
                            }
                        }
                        else if (j == ROW_NUM - 1)
                        {
                            if (i == COLUMN_NUM / 2)
                            {
                                m_DicMapGridBorderType.Add(new Vector2(i, j), EMapGridBorderType.Border_UM);

                            }
                            else
                            {
                                m_DicMapGridBorderType.Add(new Vector2(i, j), EMapGridBorderType.Border_U);
                            }
                        }
                        else
                        {
                            m_DicMapGridBorderType.Add(new Vector2(i, j), EMapGridBorderType.None);
                        }
                    }
                }
            }
        }
    }


    public class GridDataInfo
    {
        public Vector2 gridPos;
        public Vector2 mapPos;
        public EMapGridBorderType mapGridType;
        public List<EntityDataInfo> entityDataInfos;
    }

    public class EntityDataInfo
    {
        public EMapEntityType mapEntityType;
        public GameObject propEntity;
    }

    public enum EMapEntityType
    {
        #region 常规地图道具
        None = 0,
        Wall,
        Stone,
        Grass,
        Water,
        Snow,
        #endregion

        #region 特殊地图道具
        /// <summary>
        /// 1/4砖墙，左上
        /// </summary>
        Wall_LU,
        /// <summary>
        /// 1/4砖墙，左下
        /// </summary>
        Wall_LD,
        /// <summary>
        /// 1/4砖墙，右上
        /// </summary>
        Wall_RU,
        /// <summary>
        /// 1/4砖墙，右下
        /// </summary>
        Wall_RD,
        /// <summary>
        /// 1/4石头，左上
        /// </summary>
        Stone_LU,
        /// <summary>
        /// 1/4石头，左下
        /// </summary>
        Stone_LD,
        /// <summary>
        /// 1/4石头，右上
        /// </summary>
        Stone_RU,
        /// <summary>
        /// 1/4石头，右下
        /// </summary>
        Stone_RD,
        #endregion

        Brid,
        DeadBrid,
    }

    /// <summary>
    /// 地图格子边界类型
    /// </summary>
    public enum EMapGridBorderType
    {
        None = 0,
        //Entity,
        //Any,
        Border_U,
        Border_D,
        Border_L,
        Border_R,
        Border_LU,
        Border_LD,
        Border_RU,
        Border_RD,
        Border_LM,
        Border_RM,
        Border_UM,
        Border_DM,
    }
}
