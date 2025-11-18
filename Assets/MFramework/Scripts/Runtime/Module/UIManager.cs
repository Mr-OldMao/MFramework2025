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
        private Dictionary<Type, UIViewBase> m_UIViews = new Dictionary<Type, UIViewBase>();
        private Dictionary<Type, UIControllerBase> m_UIControllers = new Dictionary<Type, UIControllerBase>();
        private Dictionary<Type, UIModelBase> m_UIModels = new Dictionary<Type, UIModelBase>();
        private Dictionary<string, GameObject> m_ViewPrefabs = new Dictionary<string, GameObject>();
        private Dictionary<UILayerType, Transform> m_LayerContainer = new Dictionary<UILayerType, Transform>();

        //todo
        private Dictionary<Type, UIStateProgressType> m_StateProgressType = new Dictionary<Type, UIStateProgressType>();


        private Dictionary<Type, UIDataInfo> m_DicUIDataInfos = new Dictionary<Type, UIDataInfo>();
        public class UIDataInfo
        {
            public GameObject formPrefab;
            public UIViewBase uiView;
            public UIControllerBase uiController;
            public UIModelBase uiModel;
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

        public void ShowView<T>(object showData = null, object showBeforeData = null) where T : UIViewBase
        {
            ShowViewAsync<T>(showData, showBeforeData);
        }

        public async Task<T> ShowViewAsync<T>(object showData = null, object showBeforeData = null) where T : UIViewBase
        {
            var viewType = typeof(T);

            if (m_UIControllers.TryGetValue(viewType, out var control))
            {
                if (control != null)
                {
                    await control.Show(showData, showBeforeData);
                    return control as T;
                }
                else
                {
                    m_UIControllers.Remove(viewType);
                    m_UIViews.Remove(viewType);
                    //if (m_UIViews[viewType] != null)
                    //{
                    //    m_UIViews[viewType].OnDestory();
                    //}
                }
            }

            var viewName = GetViewName(viewType);
            var view = await CreateView<T>(viewName);
            if (view != null)
            {
                m_UIViews[viewType] = view;

                var bindControl = view.GetType().GetCustomAttribute<UIBindAttribute>();
                var newControl = bindControl.uiControllerBase;
                newControl.SetStateProgress(UIStateProgressType.LoadResCompleted);
                m_UIControllers[viewType] = newControl;

                var newModel = bindControl.uiModelBase;
                m_UIModels[viewType] = newModel;

                await newControl.Initialize(view, newModel);
                await newControl.Show(showData, showBeforeData);
            }
            return view;
        }

        public void HideView<T>(object showData = null, object showBeforeData = null) where T : UIViewBase
        {
            HideViewAsync<T>(showData, showBeforeData);
        }

        public async Task HideViewAsync<T>(object hideData = null, object hideBoforeData = null) where T : UIViewBase
        {
            var viewType = typeof(T);

            if (m_UIControllers.TryGetValue(viewType, out var control))
            {
                await control.Hide(hideData, hideBoforeData);
            }
            else
            {
                Debugger.LogError($"隐藏UI面板失败，未创建初始化面板，viewType:{viewType}");
            }
        }


        private async Task<T> CreateView<T>(string viewName) where T : UIViewBase
        {
            var prefab = await LoadUIViewPrefab(viewName);

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
        private async Task<GameObject> LoadUIViewPrefab(string viewName)
        {
            if (m_ViewPrefabs.TryGetValue(viewName, out var prefab))
            {
                return prefab;
            }
            prefab = await GameEntry.Resource.LoadAssetAsync<GameObject>(viewName, false);
            if (prefab != null)
            {
                m_ViewPrefabs[viewName] = prefab;
            }
            return prefab;
        }


        public T GetView<T>() where T : UIViewBase
        {
            m_UIViews.TryGetValue(typeof(T), out var view);
            return view as T;
        }


        private string GetViewName(Type viewType)
        {
            var name = viewType.Name;
            //TODO AddressableUI根节点路径
            string rootPath = "Assets/Download/prefab/ui/";
            return $"{rootPath}{name}.prefab";
        }



        public void DestroyView<T>() where T : UIViewBase
        {
            var viewType = typeof(T);
            RemoveContainer(viewType);
        }

        public void DestroyAll()
        {
            List<UIViewBase> uiViewBases = m_UIViews.Values.ToList();
            for (int i = 0; i < uiViewBases.Count; i++)
            {
                uiViewBases[i].OnDestory();
            }
            m_UIViews.Clear();

            List<UIControllerBase> uiControlBases = m_UIControllers.Values.ToList();
            for (int i = 0; i < uiControlBases.Count; i++)
            {
                uiControlBases[i].OnDestory();
            }
            m_UIControllers.Clear();
        }
        public void DestroyView(IUIView view)
        {
            RemoveContainer(view.GetType());
        }
        private void RemoveContainer(Type type)
        {
            if (m_UIViews.TryGetValue(type, out var view))
            {
                m_UIViews.Remove(type);
                view.OnDestory();
            }
            if (m_UIControllers.TryGetValue(type, out var control))
            {
                m_UIControllers.Remove(type);
                control.OnDestory();
            }
            if (m_StateProgressType.TryGetValue(type, out var state))
            {
                state = UIStateProgressType.DestoryCompleted;
            }
        }



        public T GetModel<T>() where T : UIModelBase
        {
            return m_UIModels.Values.Where(p => p.GetType() == typeof(T)).FirstOrDefault() as T;
        }

        public T GetController<T>() where T : UIControllerBase
        {
            return m_UIControllers.Values.Where(p => p.GetType() == typeof(T)).FirstOrDefault() as T;
        }


        public void OnUpdate(float deltaTime)
        {
            // 可更新UI的逻辑
        }

        protected override void OnShutdown()
        {
            //DestroyAll();
            m_ViewPrefabs.Clear();
        }
    }
}