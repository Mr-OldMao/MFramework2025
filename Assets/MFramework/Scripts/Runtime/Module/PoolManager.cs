using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
namespace MFramework.Runtime
{
    public class PoolManager : IPoolManager
    {
        private int m_PoolID = 0;

        private Dictionary<int, IPool> m_DicPoolCache = new Dictionary<int, IPool>();

        public int CreatPool(IPool pool)
        {
            m_DicPoolCache.Add(++m_PoolID, pool);
            return m_PoolID;
        }

        public void DestoryPool(int poolID)
        {
            if (m_DicPoolCache.ContainsKey(poolID))
            {
                m_DicPoolCache[poolID].DestroyPool();
                m_DicPoolCache.Remove(poolID);
            }
            else
            {
                Debugger.LogError("PoolManager DestoryPool poolID is not exist", LogType.FrameCore);
            }
        }

        public void DestoryAllPool()
        {
            for (int i = 0; i < m_DicPoolCache.Count; i++)
            {
                m_DicPoolCache.ElementAt(i).Value.DestroyPool();
            }
            m_DicPoolCache.Clear();
        }

        public IPool GetPool(int poolID)
        {
            if (m_DicPoolCache.ContainsKey(poolID))
            {
                return m_DicPoolCache[poolID];
            }
            else
            {
                Debugger.LogError($"PoolManager GetPool poolID is not exist , poolID:{poolID}", LogType.FrameCore);
            }
            return default;
        }

        public UniTask Init()
        {
            m_PoolID = 0;
            return UniTask.CompletedTask;
        }

        public void Shutdown()
        {
            Debugger.Log("Shutdown PoolManager", LogType.FrameNormal);
        }

        public class PoolInfo
        {
            public int poolID;
            public UnityEngine.Object templatePrefab;
        }
    }

    public class Pool : IPool
    {
        /// <summary>
        /// 创建对象池
        /// </summary>
        /// <param name="templateObj"></param>
        /// <param name="getObjCallback">p1实体，p2是否为新生成实体</param>
        /// <param name="recycleObjCallback"></param>
        /// <param name="initCount"></param>
        /// <param name="maxCount">对象池上限</param>
        public Pool(Object templateObj, Action<Object, bool> getObjCallback, Action<Object> recycleObjCallback, int initCount = 1, int maxCount = 0)
        {
            m_IsGameObject = false;
            this.getObjCallback = getObjCallback;
            this.recycleObjCallback = recycleObjCallback;
            CreatePool(templateObj, initCount, maxCount);
        }

        /// <summary>
        /// 创建对象池
        /// </summary>
        /// <param name="templateObj"></param>
        /// <param name="getObjCallback">p1实体，p2是否为新生成实体</param>
        /// <param name="recycleObjCallback"></param>
        /// <param name="initCount"></param>
        /// <param name="maxCount">对象池上限</param>
        public Pool(GameObject templateObj, Action<GameObject, bool> getObjCallback, Action<GameObject> recycleObjCallback, int initCount = 1, int maxCount = 0)
        {
            m_IsGameObject = true;
            this.getObjCallback = (obj, isCreate) => 
            {
                (obj as GameObject).SetActive(true);
                getObjCallback?.Invoke(obj as GameObject, isCreate);
            };
            this.recycleObjCallback = (obj) => 
            {
                (obj as GameObject).SetActive(false);
                recycleObjCallback?.Invoke(obj as GameObject);
            };
            CreatePool(templateObj, initCount, maxCount);
        }

        private Object TemplateObj;
        private List<Object> ListUsedObj;
        private List<Object> ListFreeObj;
        private Action<Object, bool> getObjCallback;
        private Action<Object> recycleObjCallback;
        private int maxCount = 0;
        private bool m_IsGameObject = false;

        private void CreatePool(Object templateObj, int initCount = 1, int maxCount = 0)
        {
            TemplateObj = templateObj;
            if (initCount < 0)
            {
                Debugger.LogError($"CreatePool initCount :{initCount}", LogType.FrameCore);
                initCount = 0;
            }

            if (m_IsGameObject)
            {
                (TemplateObj as GameObject).SetActive(false);
            }
            ListUsedObj = new List<Object>();
            ListFreeObj = new List<Object>();
            this.maxCount = maxCount;
            for (int i = 0; i < initCount; i++)
            {
                Object obj = GameObject.Instantiate<Object>(templateObj);
                ListUsedObj.Add(obj);
                getObjCallback.Invoke(obj, true);
            }
        }

        public void DestroyPool()
        {
            while (ListUsedObj.Count > 0)
            {
                Object obj = ListUsedObj[0];
                ListUsedObj.Remove(obj);
                GameObject.Destroy(obj);
            }
            while (ListFreeObj.Count > 0)
            {
                Object obj = ListFreeObj[0];
                ListFreeObj.Remove(obj);
                GameObject.Destroy(obj);
            }
            ListUsedObj = null;
            ListFreeObj = null;
        }

        public Object GetEntityObject()
        {
            Object res = null;
            if (ListFreeObj.Count > 0)
            {
                res = ListFreeObj[0];
                ListFreeObj.Remove(res);
                ListUsedObj.Add(res);
                getObjCallback?.Invoke(res, false);
            }
            else
            {
                if (maxCount <= 0 || ListUsedObj.Count + ListFreeObj.Count < maxCount)
                {
                    res = GameObject.Instantiate<Object>(TemplateObj);
                    getObjCallback?.Invoke(res, true);
                    ListUsedObj.Add(res);
                }
                else
                {
                    Debugger.LogError($"GetEntity faill, not create Instance , maxCount:{maxCount}", LogType.FrameCore);
                }
            }
            return res;
        }

        public GameObject GetEntity()
        {
            if (!m_IsGameObject)
            {
                return null;
            }
            GameObject res = null;
            if (ListFreeObj.Count > 0)
            {
                res = ListFreeObj[0] as GameObject;
                ListFreeObj.Remove(res);
                ListUsedObj.Add(res);
                getObjCallback?.Invoke(res, false);
            }
            else
            {
                if (maxCount <= 0 || ListUsedObj.Count + ListFreeObj.Count < maxCount)
                {
                    res = GameObject.Instantiate(TemplateObj) as GameObject;
                    getObjCallback?.Invoke(res, true);
                    ListUsedObj.Add(res);
                }
                else
                {
                    Debugger.LogError($"GetEntity faill, not create Instance , maxCount:{maxCount}", LogType.FrameCore);
                }
            }
            return res;
        }

        public void RecycleEntity(Object obj)
        {
            Object targetObj = ListUsedObj.Where((p) => { return p == obj; }).FirstOrDefault();
            if (targetObj != null)
            {
                ListUsedObj.Remove(targetObj);
                ListFreeObj.Add(targetObj);
                recycleObjCallback?.Invoke(targetObj);
            }
            else
            {
                Debugger.LogError($"RecycleObj faill , obj:{obj} is not in pool", LogType.FrameCore);
            }
        }
    }
}
