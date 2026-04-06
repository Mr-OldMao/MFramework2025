using Cysharp.Threading.Tasks;
using MFramework.Runtime;
using MiniGameSDK;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace GameMain
{
    [UIBind(typeof(UIControlBattle), typeof(UIModelBattle))]
    [UILayer(UILayerType.Background)]
    public class UIPanelBattle : UIViewBase
    {
        public Button btnFire;

        public RectTransform rectBattleDataGroup;
        public RectTransform rectEnemyGroup;
        public RectTransform imgEnemy;
        public RectTransform rectLifePlayer1;
        public RectTransform rectLifePlayer2;
        public TextMeshProUGUI txtLifePlayer1;
        public TextMeshProUGUI txtLifePlayer2;
        public TextMeshProUGUI txtStage;

        public Button btnGameParse;
        public Button btnTankLevelAdd;
        public Button btnAddLife;
        public Button btnPlayerUnbeatable;
        public Button btnRandomReward;

        private Image imgMask;
        public override UniTask Init()
        {
            return base.Init();
        }

        public override UniTask ShowPanel()
        {
            imgMask.gameObject.SetActive(false);

            return base.ShowPanel();
        }

        private void RefreshEnemyIcon()
        {
            (Controller as UIControlBattle).RefreshEnemyIcon();
        }

        private void RefreshPlayerLife()
        {
            txtLifePlayer1.text = GameMainLogic.Instance.Player1Entity != null ?
                $"{Math.Max(GameMainLogic.Instance.Player1Entity.remainLife, 0)}" : DataTools.GetConst("Player_Tank_Life").ToString();
        }

        private void RefreshStage()
        {
            txtStage.text = GameMainLogic.Instance.StageID.ToString();
        }


        public override UniTask HidePanel()
        {
            return base.HidePanel();
        }

        public override void RefreshUI(IUIModel uIModel = null)
        {
            RefreshStage();
            RefreshPlayerLife();
            RefreshEnemyIcon();
        }

        protected override void RegisterEvent()
        {
            btnFire.GetOrAddComponent<UIEvents>().AddListenerLongPressEvent((p) =>
            {
                GameMainLogic.Instance.Player1Entity?.FireByTouch();
            }, 0.1f);

            GameEntry.Event.RegisterEvent<GameObject>(GameEventType.Player1TankDead, (entity) =>
            {
                RefreshPlayerLife();
            });
            GameEntry.Event.RegisterEvent<GameObject>(GameEventType.EnemyTankDead, (entity) =>
            {
                RefreshEnemyIcon();
            });
            GameEntry.Event.RegisterEvent<GameObject>(GameEventType.EnemyTankGenerate, (entity) =>
            {
                RefreshEnemyIcon();
            });

            btnGameParse.onClick.AddListener(() =>
            {
                GameMainLogic.Instance.GameParseAutoShowInsertAdv();
            });
            btnTankLevelAdd.onClick.AddListener(() =>
            {
                //if (GameMainLogic.Instance.IsDebugger)
                //{
                //    GameMainLogic.Instance.Player1Entity.AddLevel();
                //    return;
                //}
                ShowMask(true);
                SDKManager.Instance.ShowAdvReward((isPlayed) =>
                {
                    if (isPlayed)
                    {
                        GameMainLogic.Instance.Player1Entity.AddLevel();
                    }                    
                    ShowMask(false);
                }, (msg) =>
                {
                    ShowMask(false);
                    SDKManager.GetSpecial<DouyinSDK>().ShotToast("奖励正在准备中......");
                });
            });
            btnAddLife.onClick.AddListener(() =>
            {
                ShowMask(true);
                SDKManager.Instance.ShowAdvReward((isPlayed) =>
                {
                    if (isPlayed)
                    {
                        GameMainLogic.Instance.Player1Entity.AddLife();
                    }
                    ShowMask(false);
                }, (msg) =>
                {
                    GameMainLogic.Instance.Player1Entity.AddLife();
                    ShowMask(false);
                    SDKManager.GetSpecial<DouyinSDK>().ShotToast("奖励已发放");
                });
            });
            btnPlayerUnbeatable.onClick.AddListener(() =>
            {
                ShowMask(true);
                SDKManager.Instance.ShowAdvReward((isPlayed) =>
                {
                    if (isPlayed)
                    {
                        GameEntry.Event.DispatchEvent<TankUnbeatableInfo>(GameEventType.TankUnbeatable, new TankUnbeatableInfo
                        {
                            tankEntityBase = GameMainLogic.Instance.Player1Entity,
                            durationTime = 10f * 1
                        });
                    }
                    ShowMask(false);
                }, ( msg) =>
                {
                    GameEntry.Event.DispatchEvent<TankUnbeatableInfo>(GameEventType.TankUnbeatable, new TankUnbeatableInfo
                    {
                        tankEntityBase = GameMainLogic.Instance.Player1Entity,
                        durationTime = 10f
                    });
                    ShowMask(false);
                    SDKManager.GetSpecial<DouyinSDK>().ShotToast("奖励已发放");
                });
            });

            btnRandomReward.onClick.AddListener(() =>
            {
                ShowMask(true);
                SDKManager.Instance.ShowAdvReward((isPlayed) =>
                {
                    if (isPlayed)
                    {
                        GameEntry.Pool.GetPool(GameMainLogic.Instance.PoolIdReward).GetEntity();
                    }
                    ShowMask(false);
                }, (msg) =>
                {
                    GameEntry.Pool.GetPool(GameMainLogic.Instance.PoolIdReward).GetEntity();
                    ShowMask(false);
                    SDKManager.GetSpecial<DouyinSDK>().ShotToast("奖励已发放");
                });
            });
        }

        protected override void UnRegisterEvent()
        {
            if (btnFire != null)
            {
                btnFire.GetComponent<UIEvents>()?.RemoveListenerLongPressEvent();
            }

            GameEntry.Event.UnRegisterEvent(GameEventType.Player1TankDead);
            GameEntry.Event.UnRegisterEvent(GameEventType.EnemyTankDead);
            GameEntry.Event.UnRegisterEvent(GameEventType.EnemyTankGenerate);
        }

        private void ShowMask(bool isMask)
        {
#if !UNITY_EDITOR
            imgMask.gameObject.SetActive(isMask);
#endif
            GameMainLogic.Instance.GameParse(isMask, false);
        }
    }
}
