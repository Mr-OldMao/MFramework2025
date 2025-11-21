using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MFramework.Runtime
{
    public class PersistenceDataManager : IPersistenceDataManager
    {
        private string _persistenceFolder;

        private readonly string GAME_DATA = "GameData";//TODO 后面读配置文件
        public Task Init()
        {
            _persistenceFolder = Path.Combine(Application.persistentDataPath, GAME_DATA);
            if (!Directory.Exists(_persistenceFolder))
                Directory.CreateDirectory(_persistenceFolder);
            return Task.CompletedTask;
        }

        public void SaveData<T>(string key, T data, bool isBytesData = true) where T : class
        {
            SaveDataAsync<T>(key, data, isBytesData).Wait();
        }

        public async Task SaveDataAsync<T>(string key, T data, bool isBytesData = true) where T : class
        {
            try
            {
                string filePath = GetFilePath(key);
                string json = JsonUtility.ToJson(data);
                if (isBytesData)
                {
                    await File.WriteAllBytesAsync(filePath, Encoding.UTF8.GetBytes(json));
                }
                else
                {
                    await File.WriteAllTextAsync(filePath, json, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"保存数据失败: {key}, 错误: {ex.Message}");
            }
        }

        public T ReadData<T>(string key, bool isBytesData = true) where T : class
        {
            return ReadDataAsync<T>(key, isBytesData).Result;
        }

        public async Task<T> ReadDataAsync<T>(string key, bool isBytesData = true) where T : class
        {
            try
            {
                string filePath = GetFilePath(key);
                if (!File.Exists(filePath)) return null;

                if (isBytesData)
                {
                    var bytes = await File.ReadAllBytesAsync(filePath);
                    return JsonUtility.FromJson<T>(Encoding.UTF8.GetString(bytes));
                }
                else
                {
                    var json = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
                    return JsonUtility.FromJson<T>(json);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"加载数据失败: {key}, 错误: {ex.Message}");
                return null;
            }
        }

        public bool DeleteData(string key)
        {
            try
            {
                string filePath = GetFilePath(key);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"删除数据失败: {key}, 错误: {ex.Message}");
                return false;
            }
        }

        public bool HasData(string key)
        {
            string filePath = GetFilePath(key);
            return File.Exists(filePath);
        }

        private string GetFilePath(string key)
        {
            return Path.Combine(_persistenceFolder, $"{key}.json");
        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }

        public enum EPerDataType
        {
            bytes,
            json
        }
    }
}