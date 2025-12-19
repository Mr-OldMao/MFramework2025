using Cysharp.Threading.Tasks;
using GameMain.Generate.FlatBuffers;
using Google.FlatBuffers;
using MFramework.Runtime;
using MFramework.Runtime.Extend;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace GameMain
{
    public class DataTools
    {
        private static readonly Dictionary<int, FB_bullet_bullet> dicBulletBullet = new Dictionary<int, FB_bullet_bullet>();
        private static readonly Dictionary<int, FB_level_level> dicLevelLevel = new Dictionary<int, FB_level_level>();
        private static readonly Dictionary<int, FB_reward_reward> dicRewardReward = new Dictionary<int, FB_reward_reward>();
        private static readonly Dictionary<int, FB_tank_player> dicTankPlayer = new Dictionary<int, FB_tank_player>();
        private static readonly Dictionary<int, FB_tank_enemy> dicTankEnemy = new Dictionary<int, FB_tank_enemy>();
        private static readonly Dictionary<int, FB_map_mapType> dicMapMapType = new Dictionary<int, FB_map_mapType>();

        public static async UniTask Init()
        {
            await SetBulletBullet();
            await SetLevelLevel();
            await SetRewardReward();
            await SetTankPlayer();
            await SetTankEnemy();
            await SetMapType();
            Debugger.Log("数据加载完成");
        }

        private static string GetBytesFilePath(string name)
        {
            return SystemConstantData.PATH_CONFIG_DATA_ROOT + name;
        }

        public static async UniTask SetBulletBullet()
        {
            var bytesData = await GameEntry.Resource.LoadAssetAsync<TextAsset>(GetBytesFilePath("bullet_bullet"));
            ByteBuffer byteBuffer = new ByteBuffer(bytesData.bytes);
            var datas = FB_bullet_bullet_Array.GetRootAsFB_bullet_bullet_Array(byteBuffer);
            for (int i = 0; i < datas.DatasLength; i++)
            {
                dicBulletBullet.Add(datas.Datas(i).Value.ID, datas.Datas(i).Value);
            }
        }

        public static async UniTask SetLevelLevel()
        {
            var bytesData = await GameEntry.Resource.LoadAssetAsync<TextAsset>(GetBytesFilePath("level_level"));
            ByteBuffer byteBuffer = new ByteBuffer(bytesData.bytes);
            var datas = FB_level_level_Array.GetRootAsFB_level_level_Array(byteBuffer);
            for (int i = 0; i < datas.DatasLength; i++)
            {
                dicLevelLevel.Add(datas.Datas(i).Value.ID, datas.Datas(i).Value);
            }
        }
        public static async UniTask SetRewardReward()
        {
            var bytesData = await GameEntry.Resource.LoadAssetAsync<TextAsset>(GetBytesFilePath("reward_reward"));
            ByteBuffer byteBuffer = new ByteBuffer(bytesData.bytes);
            var datas = FB_reward_reward_Array.GetRootAsFB_reward_reward_Array(byteBuffer);
            for (int i = 0; i < datas.DatasLength; i++)
            {
                dicRewardReward.Add(datas.Datas(i).Value.ID, datas.Datas(i).Value);
            }
        }
        public static async UniTask SetTankPlayer()
        {
            var bytesData = await GameEntry.Resource.LoadAssetAsync<TextAsset>(GetBytesFilePath("tank_player"));
            ByteBuffer byteBuffer = new ByteBuffer(bytesData.bytes);
            var datas = FB_tank_player_Array.GetRootAsFB_tank_player_Array(byteBuffer);
            for (int i = 0; i < datas.DatasLength; i++)
            {
                dicTankPlayer.Add(datas.Datas(i).Value.ID, datas.Datas(i).Value);
            }
        }
        public static async UniTask SetTankEnemy()
        {
            var bytesData = await GameEntry.Resource.LoadAssetAsync<TextAsset>(GetBytesFilePath("tank_enemy"));
            ByteBuffer byteBuffer = new ByteBuffer(bytesData.bytes);
            var datas = FB_tank_enemy_Array.GetRootAsFB_tank_enemy_Array(byteBuffer);
            for (int i = 0; i < datas.DatasLength; i++)
            {
                dicTankEnemy.Add(datas.Datas(i).Value.ID, datas.Datas(i).Value);
            }
        }
        public static async UniTask SetMapType()
        {
            var bytesData = await GameEntry.Resource.LoadAssetAsync<TextAsset>(GetBytesFilePath("map_mapType"));
            ByteBuffer byteBuffer = new ByteBuffer(bytesData.bytes);
            var datas = FB_map_mapType_Array.GetRootAsFB_map_mapType_Array(byteBuffer);
            for (int i = 0; i < datas.DatasLength; i++)
            {
                dicMapMapType.Add(datas.Datas(i).Value.ID, datas.Datas(i).Value);
            }
        }

        #region GetData

        public static FB_reward_reward GetRewardReward(string name)
        {
            return dicRewardReward.Values.Where(x => x.Name == name).FirstOrDefault();
        }

        public static FB_map_mapType GetMapType(int id)
        {
            return dicMapMapType.Values.Where(x => x.ID == id).FirstOrDefault();
        }

        public static List<FB_map_mapType> GetMapTypeList()
        {
            return dicMapMapType.Values.ToList();
        }

        public static int GetMapTypeIDByLevelID(int levelID)
        {
            int mapTypeID = 1;
            if (dicLevelLevel.ContainsKey(levelID))
            {
                mapTypeID = dicLevelLevel[levelID].MapTypeID;
            }
            return mapTypeID;
        }

        public static FB_bullet_bullet GetBulletBullet(int id)
        {
            return dicBulletBullet.Values.Where(x => x.ID == id).FirstOrDefault();
        }

        public static FB_tank_player GetTankPlayer(int id)
        {
            return dicTankPlayer.Values.Where(x => x.ID == id).FirstOrDefault();
        }
        #endregion

    }
}
