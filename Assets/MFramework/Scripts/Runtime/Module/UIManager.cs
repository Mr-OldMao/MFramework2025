using MFramework.Runtime.Extend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MFramework.Runtime
{
    public class UIManager : GameModuleBase, IUIManager
    {
        private Dictionary<UILayerType, Transform> m_LayerContainer = new Dictionary<UILayerType, Transform>();
        private Dictionary<Type, UIDataInfo> m_DicUIDataInfos = new Dictionary<Type, UIDataInfo>();

        public class UIDataInfo
        {
            public GameObject formPrefab;
            public IUIView view;
            public IUIController control;
            public IUIModel model;
            public UIStateProgressType state;
            public UILayerType uiLayerType;
        }

        #region public
        public void ShowView<T>() where T : UIViewBase
        {
#pragma warning disable CS4014
            ShowViewAsync<T>();
#pragma warning restore CS4014
        }

        public async Task<T> ShowViewAsync<T>() where T : UIViewBase
        {
            try
            {
                var viewType = typeof(T);
                UIDataInfo uiDataInfo = GetUIDataInfo<T>();
                if (uiDataInfo != null)
                {
                    if (GetState(uiDataInfo.view) >= UIStateProgressType.InitEnd)
                    {
                        SetState(uiDataInfo.view, UIStateProgressType.ShowStart);
                        await uiDataInfo.view.ShowPanel();
                        SetState(uiDataInfo.view, UIStateProgressType.ShowEnd);
                        return uiDataInfo.view as T;
                    }
                    else
                    {
                        //等待初始化完成返回
                        Debugger.LogError($"显示窗体失败,{uiDataInfo.state}");
                        return default;
                        //await ShowViewAsync<T>(); //未测试性能
                    }
                }
                UIDataInfo newUIDataInfo = new UIDataInfo();
                m_DicUIDataInfos.Add(viewType, newUIDataInfo);
                newUIDataInfo.state = UIStateProgressType.LoadResStart;

                var viewName = GetViewName(viewType);
                var view = await CreateView<T>(viewName);
                SetState(view, UIStateProgressType.LoadResEnd);
                if (view != null)
                {
                    var bindAttr = view.GetType().GetCustomAttribute<UIBindAttribute>();
                    if (bindAttr == null)
                    {
                        Debugger.LogError($"UIView {viewType.Name} 缺少UIBindAttribute", LogType.FrameCore);
                        return default;
                    }
                    var newControl = Activator.CreateInstance(bindAttr.ControllerType) as UIControllerBase;
                    var newModel = Activator.CreateInstance(bindAttr.ModelType, newControl) as UIModelBase;
                    view.Controller = newControl;
                    newUIDataInfo.control = newControl;
                    newUIDataInfo.model = newModel;
                    newUIDataInfo.view = view;
                    SetState(view, UIStateProgressType.InitStart);
                    await newControl.Init(view, newModel);
                    SetState(view, UIStateProgressType.InitEnd);
                    SetState(view, UIStateProgressType.ShowStart);
                    await view.ShowPanel();
                    SetState(view, UIStateProgressType.ShowEnd);
                }
                return view;
            }
            catch (Exception ex)
            {
                HandleUIError($"打开UI失败: {typeof(T).Name}", ex);
                return default;
            }
        }

        public void HideView<T>() where T : UIViewBase
        {
#pragma warning disable CS4014
            HideViewAsync<T>();
#pragma warning restore CS4014
        }

        public async Task HideViewAsync<T>() where T : UIViewBase
        {
            try
            {
                UIDataInfo uiDataInfo = GetUIDataInfo<T>();

                if (uiDataInfo != null)
                {
                    SetState(uiDataInfo.view, UIStateProgressType.HideStart);
                    await uiDataInfo.view.HidePanel();
                    SetState(uiDataInfo.view, UIStateProgressType.HideEnd);
                }
                else
                {
                    Debugger.LogError($"隐藏UI面板失败，未创建初始化面板，viewType:{typeof(T)}");
                }
            }
            catch (Exception ex)
            {
                HandleUIError($"隐藏UI失败: {typeof(T).Name}", ex);
            }
        }

        public void RemoveView<T>() where T : UIViewBase
        {
            var viewType = typeof(T);
            Remove(viewType);
        }

        public void RemoveAllView()
        {
            List<UIDataInfo> uiDataInfos = m_DicUIDataInfos.Values.ToList();
            for (int i = 0; i < uiDataInfos.Count; i++)
            {
                RemoveView(uiDataInfos[i].view);
            }
            m_DicUIDataInfos.Clear();
        }

        public void RemoveView(IUIView view)
        {
            Remove(view.GetType());
        }

        public UIDataInfo GetUIDataInfo<T>() where T : IUIView
        {
            return m_DicUIDataInfos.GetValueOrDefault(typeof(T));
        }

        public UIDataInfo GetUIDataInfo(Type type)
        {
            return m_DicUIDataInfos.GetValueOrDefault(type);
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

        public UIStateProgressType GetState(IUIView type)
        {
            if (type != null)
            {
                var uiDataInfo = GetUIDataInfo(type.GetType());
                if (uiDataInfo != null)
                {
                    return uiDataInfo.state;
                }
            }
            return UIStateProgressType.Unstart;
        }

        public void PrintDebugInfo()
        {
            Debugger.Log($"=== UI Manager Debug Info ===", LogType.FrameCore);
            Debugger.Log($"Active UI Count: {m_DicUIDataInfos.Count}", LogType.FrameCore);

            foreach (var kvp in m_DicUIDataInfos)
            {
                Debugger.Log($"{kvp.Key.Name}: {kvp.Value.state}", LogType.FrameCore);
            }
        }

        public long GetMemoryUsage()
        {
            long total = 0;
            foreach (var info in m_DicUIDataInfos.Values)
            {
                if (info.formPrefab != null)
                    total += UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(info.formPrefab);
            }
            return total;
        }
        #endregion


        protected override async Task OnInitialize()
        {
            CreateUIRoot();
            await LoadUIConfig();
            Debugger.Log("UI管理器初始化完成", LogType.FrameNormal);
        }

        protected override void OnShutdown()
        {
            RemoveAllView();
        }

        private void CreateUIRoot()
        {
            Transform uiRoot = new GameObject("UIRoot").transform;
            uiRoot.SetParent(null);
            GameObject.DontDestroyOnLoad(uiRoot.gameObject);

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
            eventSystem.transform.SetParent(uiRoot);

            var layerValues = Enum.GetValues(typeof(UILayerType));
            var layerNames = Enum.GetNames(typeof(UILayerType));
            // 创建层级
            for (int i = 0; i < layerValues.Length; i++)
            {
                var res = Enum.Parse(typeof(UILayerType), layerNames[i]);
                var go = CreateLayer(layerNames[i], (int)res, uiRoot);
                m_LayerContainer.Add((UILayerType)res, go);
            }
        }

        private Transform CreateLayer(string name, int sortingOrder, Transform parent)
        {
            GameObject layer = new GameObject(name);
            layer.transform.SetParent(parent);

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

        private void SetState(IUIView type, UIStateProgressType stateProgressType)
        {
            var uiDataInfo = GetUIDataInfo(type.GetType());
            if (uiDataInfo != null)
            {
                uiDataInfo.state = stateProgressType;
                Debugger.Log($"当前ui状态：{stateProgressType}", LogType.Test);
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
            return $"{SystemConstantData.PATH_PREFAB_UI_ROOT}{viewType.Name}.prefab";
        }

        private void Remove(Type type)
        {
            var uiDataInfo = GetUIDataInfo(type);
            if (uiDataInfo != null)
            {
                uiDataInfo.state = UIStateProgressType.DestoryStart;
                uiDataInfo.view.Shutdown();
                uiDataInfo.model.Shutdown();
                GameEntry.Resource.ReleaseInstance(uiDataInfo.formPrefab);
                m_DicUIDataInfos.Remove(type);
                uiDataInfo.state = UIStateProgressType.DestoryEnd;
            }
        }

        private void HandleUIError(string message, Exception ex = null)
        {
            Debugger.LogError($"UI错误: {message}", LogType.FrameCore);
            if (ex != null)
            {
                Debugger.LogError($"异常详情: {ex}", LogType.FrameCore);
            }
        }
    }
}