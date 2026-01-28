using System;
using UnityEngine;
using static GameMain.UserData;
using Random = UnityEngine.Random;

namespace GameMain
{
    public partial class GameMainLogic
    {
        private UserData_SystemData userData_SystemData;
        private UserData_Base userData_Base;
        private UserData_Sidebar userData_Sidebar;

        public readonly string USERDATA_SIDEBAR = "UserData_Sidebar";
        public readonly string USERDATA_BASE = "UserData_Base";
        public readonly string USERDATA_SYSTEMDATA = "UserData_SystemData";
        public void InitUserData()
        {

            userData_SystemData = ReadData<UserData_SystemData>(USERDATA_SYSTEMDATA);
            if (userData_SystemData == null)
            {
                userData_SystemData = new UserData_SystemData();
            }
            userData_SystemData.IsFristEnterGameToday = userData_SystemData.LastEnterGameTime != DateTime.Now.Date.ToString();
            userData_SystemData.LastEnterGameTime = DateTime.Now.Date.ToString();
            SaveData(USERDATA_SYSTEMDATA, userData_SystemData);


            userData_Base = ReadData<UserData_Base>(USERDATA_BASE);
            if (userData_Base == null)
            {
                userData_Base = new UserData_Base();
                SaveData(USERDATA_BASE, userData_Base);
            }


            userData_Sidebar = ReadData<UserData_Sidebar>(USERDATA_SIDEBAR);
            if (userData_Sidebar == null)
            {
                userData_Sidebar = new UserData_Sidebar();
                SaveData(USERDATA_SIDEBAR, userData_Sidebar);
            }
            TrySetSidebarReward();
        }


        #region  玩家基本信息
        public void SetUserDataBase(int passedTopStage, int score, int destroyEnemyCount)
        {
            if (passedTopStage > userData_Base.passedTopStage)
            {
                userData_Base.passedTopStage = passedTopStage;
            }
            if (score > userData_Base.topScore)
            {
                userData_Base.topScore = score;
            }
            userData_Base.addUpPassedStage += passedTopStage;
            userData_Base.addUpDestroyEnemyCount += destroyEnemyCount;
            SaveData(USERDATA_BASE, userData_Base);
        }

        public UserData_Base GetUserDataBase()
        {
            return userData_Base;
        }
        #endregion

        #region 侧边栏

        private void TrySetSidebarReward()
        {
            if (userData_SystemData.IsFristEnterGameToday)
            {
                var datas = DataTools.GetRewardSidebars();
                int index = Random.Range(0, datas.Count);
                var data = datas[index];
                userData_Sidebar.rewardType = data.RewardType;
                userData_Sidebar.rewardValue = data.RewardValue(Random.Range(0, data.GetRewardValueArray().Length));
                SaveData(USERDATA_SIDEBAR, userData_Sidebar);
                Debug.LogError($"今日首次进入游戏，刷新侧边栏奖励 rewardType:{userData_Sidebar.rewardType},rewardValue:{userData_Sidebar.rewardValue}");
            }
        }

        public void SetLastEnterGameDateTime()
        {
            userData_Sidebar.lastEnterGameDateTime = DateTime.Now.Date.ToString();
            SaveData(USERDATA_SIDEBAR, userData_Sidebar);
        }
        /// <summary>
        /// 是否可领取今日奖励
        /// </summary>
        public bool IsCanGetTodayReward
        {
            get
            {
                bool res = userData_Sidebar.lastEnterGameDateTime == System.DateTime.Now.Date.ToString();
                return res;
            }
        }

        /// <summary>
        /// 是否已领取今日奖励
        /// </summary>
        public bool IsGetTodayReward
        {
            get
            {
                if (!IsCanGetTodayReward && userData_Sidebar.isGetTodayReward)
                {
                    userData_Sidebar.isGetTodayReward = false;
                    SaveData(USERDATA_SIDEBAR, userData_Sidebar);
                }
                return userData_Sidebar.isGetTodayReward;
            }
            set
            {
                userData_Sidebar.isGetTodayReward = value;
                SaveData(USERDATA_SIDEBAR, userData_Sidebar);
            }
        }

        public UserData_Sidebar GetUserDataSidebar()
        {
            return userData_Sidebar;
        }
        #endregion


        private void SaveData<T>(string key, T data)
        {
            string jsonContent = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(key, jsonContent);
        }

        private T ReadData<T>(string key)
        {
            string jsonContent = PlayerPrefs.GetString(key);
            return JsonUtility.FromJson<T>(jsonContent);
        }
    }

    public class UserData
    {
        public class UserData_Sidebar
        {
            //public DateTime lastEnterGameDateTime;
            /// <summary>
            /// 上一次通过侧边栏进入的时间
            /// </summary>
            public string lastEnterGameDateTime;
            /// <summary>
            /// 是否已领取今日奖励
            /// </summary>
            public bool isGetTodayReward;
            /// <summary>
            /// 今日奖励ID
            /// </summary>
            public int rewardType;
            public int rewardValue;
        }

        public class UserData_Base
        {
            /// <summary>
            /// 已通关的最高个关卡
            /// </summary>
            public int passedTopStage;
            /// <summary>
            /// 最高分
            /// </summary>
            public int topScore;
            /// <summary>
            /// 累计已通关场次
            /// </summary>
            public int addUpPassedStage;
            /// <summary>
            /// 累计销毁敌军坦克数量
            /// </summary>
            public int addUpDestroyEnemyCount;
        }

        public class UserData_SystemData
        {
            /// <summary>
            /// 最后一次进入游戏的时间
            /// </summary>
            public string LastEnterGameTime;
            /// <summary>
            /// 最后一次离开游戏的时间 TODO
            /// </summary>
            public string LastLevelGameTime;

            /// <summary>
            /// 今日首次进入游戏
            /// </summary>
            public bool IsFristEnterGameToday;
        }
    }

}