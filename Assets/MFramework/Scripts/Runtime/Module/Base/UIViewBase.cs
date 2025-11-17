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

        public UIStateProgressType StateProgress { get; private set; } = UIStateProgressType.Unstart;

        public UILayerType Layer => m_Layer;
        public bool IsActive => gameObject.activeInHierarchy;


        private Task m_TaskInit;


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
            StateProgress = UIStateProgressType.InitCompleted;
        }

        public virtual async Task Show(object showData = null, object showBeforeData = null)
        {
            await m_TaskInit;
            await OnShowBefore(showBeforeData);
            StateProgress = UIStateProgressType.ShowBeforeCompleted;
            gameObject.SetActive(true);
            OnShow(showData);
            StateProgress = UIStateProgressType.ShowCompleted;
        }

        public virtual async Task Hide(object hideData = null, object hideBoforeData = null)
        {
            await m_TaskInit;
            await OnHideBefore(hideBoforeData);
            StateProgress = UIStateProgressType.HideBeforeCompleted;
            gameObject.SetActive(false);
            OnHide(hideData);
            StateProgress = UIStateProgressType.HideCompleted;
        }

        public virtual void DestoryUI()
        {
            OnClose();
            //GameEntry.UI.CloseView(this);
            Destroy(gameObject);
            StateProgress = UIStateProgressType.DestoryCompleted;
        }

        public void SetStateProgress(UIStateProgressType stateProgress)
        {
            StateProgress = stateProgress;
        }

        protected virtual void OnShow(object data) { }
        protected virtual Task OnShowBefore(object data) => Task.CompletedTask;
        protected virtual void OnHide(object data) { }
        protected virtual Task OnHideBefore(object data) => Task.CompletedTask;
        protected virtual void OnClose() { }

        // IGameModule 实现
        public virtual int Priority => 0;


        public abstract Task Initialize();
        public virtual void Shutdown() => DestoryUI();


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
    }
}