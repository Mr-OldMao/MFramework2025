using Cysharp.Threading.Tasks;
using MFramework.Runtime;
using MFramework.Runtime.Extend;
using System.Collections.Generic;
using UnityEngine;

namespace GameMain
{
    public class TestPoolManager : MonoBehaviour
    {
        private void ShowAPI()
        {
            GameObject prefab = null;
            int poolID = GameEntry.Pool.CreatPool(new Pool(prefab,
                 (obj, isNewObj) =>
                 {
                     Debugger.Log($"获取对象实体 obj:{obj.name} isNewObj:{isNewObj}");
                 },
                 (obj) =>
                 {

                 }, 3));

            Material mat = null;
           int matPoolID = GameEntry.Pool.CreatPool(new Pool(mat,
                 (obj, isNewObj) =>
                 {
                     Debugger.Log($"获取对象实体 obj:{obj.name} isNewObj:{isNewObj}");
                 },
                 (obj) =>
                 {

                 }, 3));

            GameEntry.Pool.DestoryPool(poolID);
            GameEntry.Pool.DestoryAllPool();

            IPool pool = GameEntry.Pool.GetPool(poolID);
            GameObject clone = pool.GetEntity() as GameObject;
            pool.RecycleEntity(clone);

            Material matClone = GameEntry.Pool.GetPool(matPoolID).GetEntityObject() as Material;
        }

        GameObject preafbA = null;
        GameObject preafbB = null;
        int poolIDA;
        int poolIDB;
        List<GameObject> listCloneA = new List<GameObject>();
        List<GameObject> listCloneB = new List<GameObject>();

        private async UniTask Awake()
        {
            preafbA = await GameEntry.Resource.LoadAssetAsync<GameObject>(SystemConstantData.PATH_PREFAB_ENTITY_ROOT + "Entity_Cube");
            preafbB = await GameEntry.Resource.LoadAssetAsync<GameObject>(SystemConstantData.PATH_PREFAB_ENTITY_ROOT + "Entity_Sphere");
        }

        private async void OnGUI()
        {
            if (preafbA != null)
            {
                GUIStyle style = new GUIStyle("Button");
                style.fontSize = 36;

                int width = 500;
                int height = 100;

                int curWidth = 0;
                int curHeight = 0;


                if (poolIDA == 0)
                {
                    if (GUI.Button(new Rect(curWidth, curHeight, width, height), "初始化对象池A,init=3,max=0", style))
                    {
                        poolIDA = GameEntry.Pool.CreatPool(new Pool(preafbA,
                         (obj, isNewObj) =>
                         {
                             Debugger.Log($"poolID:{poolIDA},获取对象实体 obj:{obj.name} isNewObj:{isNewObj}");
                             if (isNewObj)
                             {
                                 obj.GetComponentInChildren<MeshRenderer>().material.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                             }
                             obj.transform.position = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                             listCloneA.Add(obj);
                         },
                         (obj) =>
                         {
                             Debugger.Log($"poolID:{poolIDA},回收对象实体 obj:{obj.name}");
                         }, 3));
                    }
                }
                else if (poolIDA > 0)
                {
                    if (GUI.Button(new Rect(curWidth, curHeight, width, height), "获取对象池A实体", style))
                    {
                        IPool pool = GameEntry.Pool.GetPool(poolIDA);
                        GameObject clone = pool.GetEntity();
                    }
                    curHeight += height;

                    if (listCloneA?.Count > 0)
                    {
                        if (GUI.Button(new Rect(curWidth, curHeight, width, height), "回收对象池A实体", style))
                        {
                            GameEntry.Pool.GetPool(poolIDA).RecycleEntity(listCloneA[0]);
                            listCloneA.Remove(listCloneA[0]);
                        }
                        curHeight += height;
                    }

                    if (GUI.Button(new Rect(curWidth, curHeight, width, height), "销毁对象池A", style))
                    {
                        GameEntry.Pool.DestoryPool(poolIDA);
                        poolIDA = 0;
                    }
                }

                curHeight += height;
                curHeight += height;
                curHeight += height;

                if (poolIDB == 0)
                {
                    if (GUI.Button(new Rect(curWidth, curHeight, width, height), "初始化对象池B,init=0,max=5", style))
                    {
                        poolIDB = GameEntry.Pool.CreatPool(new Pool(preafbB,
                         (obj, isNewObj) =>
                         {
                             Debugger.Log($"poolID:{poolIDB},获取对象实体 obj:{obj.name} isNewObj:{isNewObj}");
                             if (isNewObj)
                             {
                                 obj.GetComponentInChildren<MeshRenderer>().material.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                             }
                             obj.transform.position = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                             listCloneB.Add(obj);
                         },
                         (obj) =>
                         {
                             Debugger.Log($"poolID:{poolIDB},回收对象实体 obj:{obj.name}");
                         }, 0, 5));
                    }
                }
                else if (poolIDB > 0)
                {
                    if (GUI.Button(new Rect(curWidth, curHeight, width, height), "获取对象池B实体", style))
                    {
                        IPool pool = GameEntry.Pool.GetPool(poolIDB);
                        GameObject clone = pool.GetEntity();
                    }
                    curHeight += height;

                    if (listCloneB?.Count > 0)
                    {
                        if (GUI.Button(new Rect(curWidth, curHeight, width, height), "回收对象池B实体", style))
                        {
                            GameEntry.Pool.GetPool(poolIDB).RecycleEntity(listCloneB[0]);
                            listCloneB.Remove(listCloneB[0]);
                        }
                        curHeight += height;
                    }

                    if (GUI.Button(new Rect(curWidth, curHeight, width, height), "销毁对象池B", style))
                    {
                        GameEntry.Pool.DestoryPool(poolIDB);
                        poolIDB = 0;
                    }
                }
            }
        }
    }
}
