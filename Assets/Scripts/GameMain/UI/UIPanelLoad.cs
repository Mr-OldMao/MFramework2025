using MFramework.Runtime;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

namespace GameMain
{
    [UIBind(typeof(UIControlLoad), typeof(UIModelLoad))]
    [UILayer(UILayerType.Tips)]
    public class UIPanelLoad : UIViewBase
    {
        // UI字段
        public RectTransform rootNode;
        public Image imgBg;
        public RectTransform txtLoading;
        public Slider sdrLoading;

        public RectTransform rectLoadSlider;
        public RectTransform rectLoadStage;
        public TextMeshProUGUI txtStage;

        public Animator animLoadStage;
        public override async UniTask Init()
        {
            await base.Init();
            sdrLoading.value = 0;
        }

        public override void RefreshUI(IUIModel model = null)
        {
            UIModelLoad modelLoad = model as UIModelLoad;
            if (model is not null)
            {
                modelLoad = model as UIModelLoad;
            }
            else
            {
                modelLoad = Controller.Model as UIModelLoad;
            }
            sdrLoading.value = modelLoad.LoadingProgress;
        }

        public void ShowLoadSlider()
        {
            rectLoadSlider.gameObject.SetActive(true);
            rectLoadStage.gameObject.SetActive(false);
        }

        public void ShowLoadStage(Action callback)
        {
            txtStage.text = GameMainLogic.Instance.StageID.ToString();
            rectLoadSlider.gameObject.SetActive(false);
            rectLoadStage.gameObject.SetActive(true);
            if (animLoadStage == null)
            {
                animLoadStage = rectLoadStage.GetComponent<Animator>();

            }
            string animName = "loadStage";
            animLoadStage.Play(animName);
            //等待当前动画播放完成
            float animTime = animLoadStage.runtimeAnimatorController.animationClips.Where(p => p.name == animName).FirstOrDefault().length;
            GameEntry.Timer.AddDelayTimer(animTime, () =>
            {
                callback?.Invoke();
                HidePanel();
            });
        }


        public override UniTask ShowPanel()
        {
            base.ShowPanel();
            return UniTask.CompletedTask;
        }

        public override UniTask HidePanel()
        {
            base.HidePanel();
            return UniTask.CompletedTask;
        }

        protected override void RegisterEvent()
        {
            GameEntry.Event.RegisterEvent(GameEventType.LoadingProgress, () =>
                {
                    RefreshUI();
                });
        }

        protected override void UnRegisterEvent()
        {
            GameEntry.Event.UnRegisterEvent(GameEventType.LoadingProgress);

        }
    }
}
