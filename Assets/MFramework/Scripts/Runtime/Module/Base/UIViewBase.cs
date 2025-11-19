using GameMain;
using MFramework.Runtime.Extend;
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

        public GameObject UIForm { get => this.gameObject; }
        public IUIController Controller { get; set; }

        private bool m_IsRegisteredEvent;

        protected virtual void Awake()
        {
            AutoBindComponents();
            SetLayer();
        }

        public virtual Task Init()
        {
            m_IsRegisteredEvent = false;
            return Task.CompletedTask;
        }

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

        public virtual Task ShowPanel()
        {
            gameObject.SetActive(true);
            if (!m_IsRegisteredEvent)
            {
                m_IsRegisteredEvent = !m_IsRegisteredEvent;
                RegisterEvent();
            }
            return Task.CompletedTask;
        }

        public virtual Task HidePanel()
        {
            gameObject.SetActive(false);
            if (m_IsRegisteredEvent)
            {
                m_IsRegisteredEvent = !m_IsRegisteredEvent;
                UnRegisterEvent();
            }
            return Task.CompletedTask;
        }

        public abstract void RefreshUI(IUIModel uIModel = null);

        public async Task<Sprite> SetSpriteAsync(Image img, EAtlasType atlasType, string spriteName)
        {
            string atlasPath = $"{SystemConstantData.PATH_ATLAS_ROOT}{atlasType}.spriteatlas";
            var atlas = await GameEntry.Resource.LoadAssetAsync<SpriteAtlas>(atlasPath, false);
            Sprite sprite = atlas.GetSprite(spriteName);
            img.sprite = sprite;
            return sprite;
        }

        protected abstract void RegisterEvent();

        protected abstract void UnRegisterEvent();

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
            if (m_IsRegisteredEvent)
            {
                m_IsRegisteredEvent = !m_IsRegisteredEvent;
                UnRegisterEvent();
            }
            Destroy(gameObject);
        }
    }
}