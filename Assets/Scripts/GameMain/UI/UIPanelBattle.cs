using Cysharp.Threading.Tasks;
using MFramework.Runtime;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace GameMain
{
    [UIBind(typeof(UIControlBattle), typeof(UIModelBattle))]
    [UILayer(UILayerType.Normal)]
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

        public override UniTask Init()
        {
            return base.Init();
        }

        public override UniTask ShowPanel()
        {
            return base.ShowPanel();
        }

        private void RefreshEnemyIcon()
        {
            (Controller as UIControlBattle).RefreshEnemyIcon();
        }

        private void RefreshPlayerLife()
        {
            txtLifePlayer1.text = $"{Math.Max(GameMainLogic.Instance.Player1Entity.remainLife, 0)}";
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
                GameMainLogic.Instance.Player1Entity.FireByTouch();
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
        }

        protected override void UnRegisterEvent()
        {
            btnFire.GetComponent<UIEvents>().RemoveListenerLongPressEvent();

            GameEntry.Event.UnRegisterEvent(GameEventType.Player1TankDead);
            GameEntry.Event.UnRegisterEvent(GameEventType.EnemyTankDead);
            GameEntry.Event.UnRegisterEvent(GameEventType.EnemyTankGenerate);
        }
    }
}
