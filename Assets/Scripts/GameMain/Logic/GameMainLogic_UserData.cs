using System;
using UnityEngine;
using static GameMain.UserData;

namespace GameMain
{
    public partial class GameMainLogic
    {
        private UserData_Sidebar userData_Sidebar;
        private UserData_Base userData_Base;

        public readonly string USERDATA_SIDEBAR = "UserData_Sidebar";
        public readonly string USERDATA_BASE = "UserData_Base";
        public void InitUserData()
        {
            userData_Sidebar = ReadData<UserData_Sidebar>(USERDATA_SIDEBAR);
            if (userData_Sidebar == null)
            {
                userData_Sidebar = new UserData_Sidebar();
                SaveData(USERDATA_SIDEBAR, userData_Sidebar);
            }

            userData_Base = ReadData<UserData_Base>(USERDATA_BASE);
            if (userData_Base == null)
            {
                userData_Base = new UserData_Base();
                SaveData(USERDATA_BASE, userData_Base);
            }
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
            public string lastEnterGameDateTime;
            /// <summary>
            /// 是否已领取今日奖励
            /// </summary>
            public bool isGetTodayReward;
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
    }

}