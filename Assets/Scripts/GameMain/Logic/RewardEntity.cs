using MFramework.Runtime;
using UnityEngine;

namespace GameMain
{
    public partial class RewardEntity : MonoBehaviour
    {
        public int id;

        public SpriteRenderer imgReward;

        private int m_TimerID;
        private int m_TimerCount;
        public void Init(int id = 0,int duration = 20)
        {
            this.id = id;
            m_TimerCount = 0;
            imgReward = transform.Find<SpriteRenderer>("imgReward");
            GameEntry.Audio.PlaySound("prop_award.mp3");
            SetPos();


            float loopSeconds = 0.5f;
            m_TimerID = GameEntry.Timer.AddDelayTimer(0, loopSeconds, () =>
            {
                ++m_TimerCount;
                imgReward.gameObject.SetActive(m_TimerCount % 3 != 0);
            }, () =>
            {
                GameEntry.Pool.GetPool(GameMainLogic.Instance.PoolIdReward).RecycleEntity(gameObject);
            }, (int)(duration/loopSeconds));
        }

        private void SetPos()
        {
            Vector2 targetPos = Vector2Int.zero;
            int randomX = 0;
            int randomY = 0;
            bool isFindPos = false;
            int remainLoopCount = 200;
            while (!isFindPos && remainLoopCount > 0)
            {
                randomX = Random.Range(0, GameEntry.UI.GetModel<UIModelMap>().COLUMN_NUM);
                randomY = Random.Range(0, GameEntry.UI.GetModel<UIModelMap>().ROW_NUM);
                targetPos = new Vector2(randomX, randomY);
                var gridData = GameEntry.UI.GetModel<UIModelMap>().GetMapGridDataInfo(targetPos);
                if (gridData?.entityDataInfos != null
                    && gridData?.entityDataInfos.Find(p =>
                        p.mapEntityType == EMapEntityType.Water
                        || p.mapEntityType == EMapEntityType.Brid
                        || p.mapEntityType == EMapEntityType.DeadBrid) == null)
                {
                    isFindPos = true;
                }
                --remainLoopCount;
            }
            transform.position = new Vector3(targetPos.x, 0, targetPos.y);
        }

        private void OnTriggerEnter(Collider other)
        {
            bool isPlayer = false;
            TankEntityBase tankEntityBase = other.GetComponent<TankEntityBase>();
            if (tankEntityBase != null)
            {
                isPlayer = tankEntityBase.TankOwnerType == TankOwnerType.Player1 || tankEntityBase.TankOwnerType == TankOwnerType.Player2;
            }
            if (!isPlayer)
            {
                return;
            }

            AddBuff();

        }

        private void AddBuff()
        {
            Debug.Log($"AddBuff {id}");

            switch (id)
            {
                case 1://星级道具：永久提升坦克火力，最高可击穿钢铁墙壁
                    GameMainLogic.Instance.Player1Entity.AddLevel();
                    GameEntry.Audio.PlaySound("prop_award.mp3");
                    break;
                case 2://定时道具：冻结所有敌方坦克行动
                    GameEntry.Event.DispatchEvent<float>(GameEventType.StopAllEnemyMove, 5f);
                    GameEntry.Audio.PlaySound("prop_award.mp3");
                    break;
                case 3://防护道具：为玩家坦克提供短暂无敌状态
                    GameEntry.Event.DispatchEvent<TankUnbeatableInfo>(GameEventType.TankUnbeatable, new TankUnbeatableInfo
                    {
                        tankEntityBase = GameMainLogic.Instance.Player1Entity,
                        durationTime = 5f
                    });
                    GameEntry.Audio.PlaySound("prop_award.mp3");
                    break;
                case 4://生命道具：增加玩家额外生命值
                    GameMainLogic.Instance.Player1Entity.AddLife();
                    break;
                case 5://手雷道具：一键摧毁场上所有敌方坦克
                    GameEntry.Event.DispatchEvent<TankOwnerType>(GameEventType.ClearAllEnemy, TankOwnerType.Player1);
                    GameEntry.Audio.PlaySound("prop_bomb.mp3");
                    break;
                case 6://保卫道具：鸟窝砖硬化为墙
                    GameEntry.Event.DispatchEvent<float>(GameEventType.BirdChangeStore, 20f);
                    GameEntry.Audio.PlaySound("prop_award.mp3");
                    break;
            }
            GameEntry.Pool.GetPool(GameMainLogic.Instance.PoolIdReward).RecycleEntity(gameObject);
        }

        public void ClreaTimer()
        {
            GameEntry.Timer.RemoveDelayTimer(m_TimerID);
        }
    }
}
