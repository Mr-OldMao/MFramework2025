using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using static MFramework.Runtime.UIViewBase;

namespace MFramework.Runtime
{
    public class UIManager : GameModuleBase, IUIManager
    {
        private Transform m_UIRoot;
        private Dictionary<UILayerType, Transform> m_LayerContainer = new Dictionary<UILayerType, Transform>();
        private Dictionary<Type, UIDataInfo> m_DicUIDataInfos = new Dictionary<Type, UIDataInfo>();

        public class UIDataInfo
        {
            public GameObject formPrefab;
            public IUIView view;
            public IUIController control;
            public IUIModel model;
            public UIStateProgressType stateProgressType;
            public UILayerType uiLayerType;
        }

        protected override async Task OnInitialize()
        {
            CreateUIRoot();
            await LoadUIConfig();
            Debugger.Log("UI管理器初始化完成", LogType.FrameNormal);
        }

        private void CreateUIRoot()
        {
            m_UIRoot = new GameObject("UIRoot").transform;
            m_UIRoot.SetParent(null);
            GameObject.DontDestroyOnLoad(m_UIRoot.gameObject);

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
            eventSystem.transform.SetParent(m_UIRoot);

            var layerValues = Enum.GetValues(typeof(UILayerType));
            var layerNames = Enum.GetNames(typeof(UILayerType));
            // 创建层级
            for (int i = 0; i < layerValues.Length; i++)
            {
                var res = Enum.Parse(typeof(UILayerType), layerNames[i]);
                var go = CreateLayer(layerNames[i], (int)res);
                m_LayerContainer.Add((UILayerType)res, go);
            }
        }

        private Transform CreateLayer(string name, int sortingOrder)
        {
            GameObject layer = new GameObject(name);
            layer.transform.SetParent(m_UIRoot);

            var canvas = layer.gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = sortingOrder;

            layer.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            return layer.transform;
        }

        private async Task LoadUIConfig()
        {
            // 可以从配置表或Addressables加载UI配置
            // 这里简单实现，实际项目中可以从JSON/ScriptableObject加载
            await Task.CompletedTask;
        }

        public UIDataInfo GetUIDataInfo<T>() where T : IUIView
        {
            return m_DicUIDataInfos.GetValueOrDefault(typeof(T));
        }

        public UIDataInfo GetUIDataInfo(Type type)
        {
            return m_DicUIDataInfos.GetValueOrDefault(type);
        }

        public void ShowView<T>(object showBeforeData = null, object showAfterData = null) where T : UIViewBase
        {
#pragma warning disable CS4014
            ShowViewAsync<T>(showBeforeData, showAfterData);
#pragma warning restore CS4014
        }

        public async Task<T> ShowViewAsync<T>(object showBeforeData = null, object showAfterData = null) where T : UIViewBase
        {
            var viewType = typeof(T);
            UIDataInfo uiDataInfo = GetUIDataInfo<T>();
            if (uiDataInfo != null)
            {
                await uiDataInfo.control.Show(showBeforeData, showAfterData);
                return uiDataInfo.view as T;
            }

            UIDataInfo newUIDataInfo = new UIDataInfo();
            m_DicUIDataInfos.Add(viewType, newUIDataInfo);

            var viewName = GetViewName(viewType);
            var view = await CreateView<T>(viewName);
            if (view != null)
            {
                var bindControl = view.GetType().GetCustomAttribute<UIBindAttribute>();
                var newControl = bindControl.uiControllerBase;
                var newModel = bindControl.uiModelBase;
                view.Controller = newControl;
                newUIDataInfo.stateProgressType = UIStateProgressType.LoadResCompleted;
                newUIDataInfo.control = newControl;
                newUIDataInfo.model = newModel;
                newUIDataInfo.view = view;
                await newControl.Initialize(view, newModel);
                await newControl.Show(showBeforeData, showAfterData);
            }
            return view;
        }

        public void HideView<T>(object hideAfterData = null, object showBeforeData = null) where T : UIViewBase
        {
#pragma warning disable CS4014
            HideViewAsync<T>(hideAfterData, showBeforeData);
#pragma warning restore CS4014
        }

        public async Task HideViewAsync<T>(object hideAfterData = null, object hideBoforeData = null) where T : UIViewBase
        {
            UIDataInfo uiDataInfo = GetUIDataInfo<T>();

            if (uiDataInfo != null)
            {
                await uiDataInfo.control.Hide(hideAfterData, hideBoforeData);
            }
            else
            {
                Debugger.LogError($"隐藏UI面板失败，未创建初始化面板，viewType:{typeof(T)}");
            }
        }

        public void SetState(IUIView type, UIStateProgressType stateProgressType)
        {
            var uiDataInfo = GetUIDataInfo(type.GetType());
            if (uiDataInfo != null)
            {
                uiDataInfo.stateProgressType = stateProgressType;
            }
        }

        private async Task<T> CreateView<T>(string viewName) where T : UIViewBase
        {
            var prefab = await LoadUIViewPrefab<T>(viewName);

            if (prefab == null)
            {
                Debugger.LogError($"UI预制体加载失败: {viewName}", LogType.FrameCore);
                return null;
            }
            var go = GameObject.Instantiate(prefab);
            var viewBaseScript = go.AddComponent<T>();

            go.transform.SetParent(m_LayerContainer[viewBaseScript.Layer]);
            go.transform.localScale = Vector3.one;
            return viewBaseScript;
        }

        /// <summary>
        /// 加载UI预制体
        /// </summary>
        /// <param name="viewName">资源形参addressable全路径 Assets/xxx/xxx.prefab</param>
        /// <returns></returns>
        private async Task<GameObject> LoadUIViewPrefab<T>(string viewName) where T : UIViewBase
        {
            UIDataInfo uiDataInfo = GetUIDataInfo<T>();
            if (uiDataInfo.formPrefab != null)
            {
                return uiDataInfo.formPrefab;
            }
            var formPrefab = await GameEntry.Resource.LoadAssetAsync<GameObject>(viewName, false);
            if (formPrefab != null)
            {
                uiDataInfo.formPrefab = formPrefab;
            }
            return formPrefab;
        }


        private string GetViewName(Type viewType)
        {
            var name = viewType.Name;
            //TODO AddressableUI根节点路径
            string rootPath = "Assets/Download/prefab/ui/";
            return $"{rootPath}{name}.prefab";
        }



        public void Clear<T>() where T : UIViewBase
        {
            var viewType = typeof(T);
            RemoveContainer(viewType);
        }

        public void ClearAll()
        {
            List<UIDataInfo> uiDataInfos = m_DicUIDataInfos.Values.ToList();
            for (int i = 0; i < uiDataInfos.Count; i++)
            {
                uiDataInfos[i].view.OnDestory();
                uiDataInfos[i].control.OnDestory();
                GameEntry.Resource.ReleaseInstance(uiDataInfos[i].formPrefab);
            }
            m_DicUIDataInfos.Clear();
        }
        public void Clear(IUIView view)
        {
            RemoveContainer(view.GetType());
        }

        private void RemoveContainer(Type type)
        {
            var uiDataInfo = GetUIDataInfo(type);
            if (uiDataInfo != null)
            {
                //uiDataInfo.view.OnDestory();
                //uiDataInfo.control.OnDestory();
                uiDataInfo.stateProgressType = UIStateProgressType.DestoryCompleted;
                GameEntry.Resource.ReleaseInstance(uiDataInfo.formPrefab);
                m_DicUIDataInfos.Remove(type);
            }
        }

        public T GetView<T>() where T : UIViewBase
        {
            return GetUIDataInfo<T>().view as T;
        }

        public T GetModel<T>() where T : UIModelBase
        {
            var dataInfo = m_DicUIDataInfos.Values.Where(p => p.model.GetType() == typeof(T)).FirstOrDefault();
            return dataInfo?.model as T;
        }

        public UIModelBase GetModel(IUIModel uiModel)
        {
            var dataInfo = m_DicUIDataInfos.Values.Where(p => p.model == uiModel).FirstOrDefault();
            return dataInfo?.model as UIModelBase;
        }

        public T GetController<T>() where T : UIControllerBase
        {
            var dataInfo = m_DicUIDataInfos.Values.Where(p => p.control.GetType() == typeof(T)).FirstOrDefault();
            return dataInfo?.control as T;
        }

        public UIControllerBase GetController(IUIController uiController)
        {
            var dataInfo = m_DicUIDataInfos.Values.Where(p => p.control == uiController).FirstOrDefault();
            return dataInfo?.control as UIControllerBase;
        }

        public void OnUpdate(float deltaTime)
        {
            // 可更新UI的逻辑
        }

        protected override void OnShutdown()
        {

        }
    }
}