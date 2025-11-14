using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using static MFramework.Runtime.UIBase;

namespace MFramework.Runtime
{
    public class UIManager : GameModuleBase, IUIManager
    {
        private Transform m_UIRoot;
        private Dictionary<Type, UIBase> m_ActiveViews = new Dictionary<Type, UIBase>();
        private Dictionary<string, GameObject> m_ViewPrefabs = new Dictionary<string, GameObject>();
        private Dictionary<UILayerType, Transform> m_LayerContainer = new Dictionary<UILayerType, Transform>();
        public override int Priority => 30;

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

        public async Task<T> OpenView<T>(object data = null) where T : UIBase
        {
            var viewType = typeof(T);

            if (m_ActiveViews.TryGetValue(viewType, out var existingView))
            {
                existingView.Show(data);
                return existingView as T;
            }

            var viewName = GetViewName(viewType);
            var view = await CreateView<T>(viewName);
            if (view != null)
            {
                m_ActiveViews[viewType] = view;
                view.Show(data);
            }

            return view as T;
        }

        private async Task<T> CreateView<T>(string viewName) where T : UIBase
        {
            var prefab = await LoadViewPrefab(viewName);
            if (prefab == null)
            {
                Debugger.LogError($"UI预制体加载失败: {viewName}", LogType.FrameCore);
                return null;
            }
            var go = GameObject.Instantiate(prefab);
            var view = go.AddComponent<T>();
            go.transform.SetParent(m_LayerContainer[view.Layer]);
            go.transform.localScale = Vector3.one;
            return view;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="viewName">资源形参addressable全路径 Assets/xxx/xxx.prefab</param>
        /// <returns></returns>
        private async Task<GameObject> LoadViewPrefab(string viewName)
        {
            if (m_ViewPrefabs.TryGetValue(viewName, out var prefab))
            {
                return prefab;
            }
            // 实际应该使用ResourcesManager异步加载
            prefab = await GameEntry.Resource.LoadAssetAsync<GameObject>(viewName, false);
            //prefab = await GameEntry.Resource.LoadAssetAsync<GameObject>($"UI/{viewName}.prefab");
            //prefab = Resources.Load<GameObject>($"UI/{viewName}");
            if (prefab != null)
            {
                m_ViewPrefabs[viewName] = prefab;
            }
            return prefab;
        }

        public void CloseView<T>() where T : UIBase
        {
            var viewType = typeof(T);
            CloseView(viewType);
        }

        public T GetView<T>() where T : UIBase
        {
            m_ActiveViews.TryGetValue(typeof(T), out var view);
            return view as T;
        }

        public void CloseAll()
        {
            List<UIBase> uIBases = m_ActiveViews.Values.ToList();
            for (int i = 0; i < uIBases.Count; i++)
            {
                uIBases[i].Close();
            }
            m_ActiveViews.Clear();
        }

        private string GetViewName(Type viewType)
        {
            var name = viewType.Name;
            //TODO AddressableUI根节点路径
            string rootPath = "Assets/Download/prefab/ui/";
            return $"{rootPath}{name}.prefab";
        }

        public void OnUpdate(float deltaTime)
        {
            // 可更新UI的逻辑
        }

        protected override void OnShutdown()
        {
            CloseAll();
            m_ViewPrefabs.Clear();
        }

        public void CloseView(Type viewType)
        {
            if (m_ActiveViews.TryGetValue(viewType, out var view))
            {
                view.Close();
                m_ActiveViews.Remove(viewType);
            }
        }

        public void CloseView(IUIView view)
        {
            CloseView(view.GetType());
        }
    }
}