using System;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace MFramework.Runtime
{
    public abstract class UIViewBase : MonoBehaviour, IUIView
    {
        [SerializeField]
        private string m_ViewName;
        [SerializeField]
        private UILayerType m_Layer;

        [SerializeField]
        private string _ = "以下字段为运行时自动绑定，请勿手动修改";

        public UILayerType Layer => m_Layer;
        public bool IsActive => gameObject.activeInHierarchy;

        private Task m_TaskInit;

        public GameObject UIForm { get => this.gameObject; }

        public enum UILayerType
        {
            Background = 0,
            Normal = 500,
            Popup = 1000,
            Tips = 1500,
        }

        public enum UIStateProgressType
        {
            Unstart = 0,
            LoadResCompleted,
            InitCompleted,
            ShowBeforeCompleted,
            ShowCompleted,
            HideBeforeCompleted,
            HideCompleted,
            DestoryCompleted,
        }

        protected virtual async void Awake()
        {
            AutoBindComponents();
            SetLayer();
            m_TaskInit = Initialize();
            await m_TaskInit;
        }

        public virtual void OnDestory()
        {
            GameEntry.UI.DestroyView(this);
            Destroy(gameObject);
        }

        private void SetLayer()
        {
            var layerArr = GetType().GetCustomAttribute<UILayerAttribute>();
            m_Layer = layerArr != null ? layerArr.Layer : UILayerType.Background;
        }

        private void AutoBindComponents()
        {
            var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var field in fields)
            {
                BindComponent(field);
            }
        }

        private void BindComponent(FieldInfo field)
        {
            var goArr = transform.GetComponentsInChildren(field.FieldType, true);
            bool isBindSucc = false;
            for (int i = 0; i < goArr.Length; i++)
            {
                if (goArr[i].name == field.Name)
                {
                    field.SetValue(this, goArr[i]);
                    if (!isBindSucc)
                    {
                        isBindSucc = true;
                    }
                    else
                    {
                        Debug.LogError($"UI初始化绑定发现重复类型实体，UICanvas{this},Name:{field.Name},Type:{field.FieldType}");
                    }
#if !UNITY_EDITOR
                    break; 
#endif
                }
            }
        }

        public virtual void Shutdown()
        {

        }

        public abstract void ShowPanel(IUIModel uIModel);
        public abstract void HidePanel(IUIModel uIModel);
        public abstract void RefreshUI(IUIModel uIModel);
        public abstract Task Initialize();
    }
}