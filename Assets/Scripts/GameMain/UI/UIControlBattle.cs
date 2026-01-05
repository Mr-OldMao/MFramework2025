using MFramework.Runtime;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GameMain
{
    public class UIControlBattle : UIControllerBase
    {
        public ETCJoystick Joystick { get; private set; }

        private int m_PoolEnemyIcon;

        private Queue<GameObject> m_EnemyIconArr = new Queue<GameObject>();
        public override async UniTask Init(IUIView view, IUIModel model)
        {
            await base.Init(view, model);


            Joystick = ((UIPanelBattle)(view)).gameObject.GetComponentInChildren<ETCJoystick>();

            UIPanelBattle uIPanelBattle = (UIPanelBattle)view;
            m_PoolEnemyIcon = GameEntry.Pool.CreatPool(new Pool(uIPanelBattle.imgEnemy.gameObject, (go, b) =>
            {
                if (b)
                {
                    //go.SetActive(false);
                    go.transform.SetParent(uIPanelBattle.rectEnemyGroup.transform);
                }
                m_EnemyIconArr.Enqueue(go);
            }, (go) =>
            {
                m_EnemyIconArr.Dequeue();
            }, 10, 30));
        }

        public void GetEnemyIcon()
        {
            GameEntry.Pool.GetPool(m_PoolEnemyIcon).GetEntity();
        }

        public void HideEnemyIcon(GameObject go = null)
        {
            if (go != null)
            {
                GameEntry.Pool.GetPool(m_PoolEnemyIcon).RecycleEntity(go);

            }
            else
            {
                GameEntry.Pool.GetPool(m_PoolEnemyIcon).RecycleAllEntity();
            }
        }


        public void RefreshEnemyIcon()
        {
            int enemyEntityCount = GameEntry.Pool.GetPool(GameMainLogic.Instance.PoolIdTankEnemy).UsedCount;
            int enemyIconCount = GameEntry.Pool.GetPool(m_PoolEnemyIcon).UsedCount;

            if (enemyEntityCount > enemyIconCount)
            {
                for (int i = 0; i < enemyEntityCount - enemyIconCount; i++)
                {
                    GameEntry.Pool.GetPool(m_PoolEnemyIcon).GetEntity();
                }
            }
            else if (enemyIconCount > enemyEntityCount)
            {
                for (int i = 0; i < enemyIconCount - enemyEntityCount; i++)
                {
                    GameEntry.Pool.GetPool(m_PoolEnemyIcon).RecycleEntity(m_EnemyIconArr.Peek());
                }
            }
        }
    }
}
