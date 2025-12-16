using MFramework.Runtime;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GameMain
{
    public class UIControlMap : UIControllerBase
    {
        private UIModelMap model;
        private UIPanelMap view;

        private bool isGenerateMap = false;
        public override async UniTask Init(IUIView view, IUIModel model)
        {
            await base.Init(view, model);
            model = (UIModelMap)Model;
            view = (UIPanelMap)View;
            await InitMapEntity();
        }


        private async UniTask InitMapEntity()
        {
            await GenerateMap();
            Debugger.Log("InitMapEntity Completed ", LogType.Test);
        }


        public async UniTask GenerateMap()
        {
            if (isGenerateMap)
            {
                return;
            }
            isGenerateMap = true;
            ((UIModelMap)Model).GenerateMapData();
            await GenerateMapEntityByDataAsync();
            isGenerateMap = false;
        }

        private async UniTask GenerateMapEntityByDataAsync()
        {
            model = (UIModelMap)Model;
            view = (UIPanelMap)View;
            List<GridDataInfo> gridDataInfos = model.GetMapGridData();

            GameObject mapNode = view.NodeContainer.Find("mapNode")?.gameObject;
            if (mapNode != null)
            {
                GameObject.Destroy(mapNode);
            }
            mapNode = new GameObject("mapNode");
            mapNode.transform.SetParent(view.NodeContainer);
            mapNode.transform.localPosition = Vector3.zero;

            for (int i = 0; i < gridDataInfos.Count; i++)
            {
                for (int j = 0; j < gridDataInfos[i]?.entityDataInfos?.Count; j++)
                {
                    string assetPath = model.GetMapPropAssetPath(gridDataInfos[i].entityDataInfos[j].mapEntityType);
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        var go = await GameEntry.Resource.InstantiateAsset(assetPath);
                        go.transform.SetParent(mapNode.transform);
                        go.transform.localPosition = gridDataInfos[i].mapPos;
                        gridDataInfos[i].entityDataInfos[j].propEntity = go;
                    }
                }
            }
        }
    }



}
