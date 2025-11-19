using GameMain;
using System;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace MFramework.Runtime
{
    public abstract class UIViewBase : MonoBehaviour, IUIView
    {
        [Header("以下字段为运行时自动绑定，请勿手动修改")]
        [SerializeField]
        private UILayerType m_Layer;
        public UILayerType Layer => m_Layer;
        public bool IsActive => gameObject.activeInHierarchy;

        private Task m_TaskInit;

        public GameObject UIForm { get => this.gameObject; }
        public IUIController Controller { get; set; }

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


        /// <summary>
        /// 设置精灵图片
        /// </summary>
        /// <param name="img"></param>
        /// <param name="atlasName"></param>
        /// <param name="spriteName"></param>
        /// <param name="callback"></param>
        public void SetSprite(Image img, EAtlasType atlasType, string spriteName, Action<Sprite> callback = null)
        {
            string atlasPath = $"{SystemConstantData.PATH_ATLAS_ROOT}{atlasType}.spriteatlas";
            GameEntry.Resource.LoadAssetAsync<SpriteAtlas>(atlasPath, (atlas) =>
            {
                if (atlas != null)
                {
                    Sprite sprite = atlas.GetSprite(spriteName);
                    img.sprite = sprite;
                    callback?.Invoke(sprite);
                }
                else
                {
                    Debugger.LogError($"atlas is null,atlasPath:{atlasPath},spriteName:{spriteName},img:{img},uiform:{this}");
                }
            }, false);
        }

        public async Task<Sprite> SetSpriteAsync(Image img, EAtlasType atlasType, string spriteName)
        {
            string atlasPath = $"{SystemConstantData.PATH_ATLAS_ROOT}{atlasType}.spriteatlas";

            var atlas = await GameEntry.Resource.LoadAssetAsync<SpriteAtlas>(atlasPath, false);
            Sprite sprite = atlas.GetSprite(spriteName);
            img.sprite = sprite;
            return sprite;
        }

        public virtual void OnDestory()
        {
            Destroy(gameObject);
            GameEntry.UI.Clear(this);
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

        public virtual void ShowPanel(IUIModel uIModel = null)
        {
            gameObject.SetActive(true);
        }
        public virtual  void HidePanel(IUIModel uIModel = null)
        {
            gameObject.SetActive(false);
        }
        public abstract void RefreshUI(IUIModel uIModel = null);
        public abstract Task Initialize();

    }
}