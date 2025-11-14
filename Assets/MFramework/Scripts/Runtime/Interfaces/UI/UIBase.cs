using System.Reflection;
using UnityEngine;

namespace MFramework.Runtime
{
    public abstract class UIBase : MonoBehaviour, IUIView
    {
        [SerializeField]
        private string m_ViewName;
        [SerializeField]
        private UILayerType m_Layer;

        [SerializeField]
        private string _ = "以下字段为自动绑定，请勿手动修改";

        public string ViewName => m_ViewName;
        public UILayerType Layer => m_Layer;
        public bool IsActive => gameObject.activeInHierarchy;

        public enum UILayerType
        {
            Background = 0,
            Normal = 500,
            Popup = 1000,
            Tips = 1500,
        }

        protected virtual void Awake()
        {
            AutoBindComponents();
            SetLayer();
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

        public virtual void Show(object data = null)
        {
            gameObject.SetActive(true);
            OnShow(data);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
            OnHide();
        }

        public virtual void Close()
        {
            OnClose();
            //GameEntry.UI.CloseView(this);
            Destroy(gameObject);
        }

        protected virtual void OnShow(object data) { }
        protected virtual void OnHide() { }
        protected virtual void OnClose() { }

        // IGameModule 实现
        public virtual int Priority => 0;
        public virtual System.Threading.Tasks.Task Initialize() => System.Threading.Tasks.Task.CompletedTask;
        public virtual void Shutdown() => Close();
    }
}