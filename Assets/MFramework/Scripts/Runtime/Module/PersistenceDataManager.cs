using System;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MFramework.Runtime
{
    public class PersistenceDataManager : IPersistenceDataManager
    {
        private const string GAME_DATA = "GameData";//TODO 后面读配置文件

        private string _persistenceFolder = Path.Combine(Application.persistentDataPath, GAME_DATA);

        public UniTask Init()
        {
            if (!Directory.Exists(_persistenceFolder))
                Directory.CreateDirectory(_persistenceFolder);
            return UniTask.CompletedTask;
        }

        public void SaveData<T>(string key, T data, bool isBytesData = true) where T : class
        {
            try
            {
                string filePath = GetFilePath(key, isBytesData);
                string json = JsonUtility.ToJson(data);
                if (isBytesData)
                {
                    File.WriteAllBytes(filePath, Encoding.UTF8.GetBytes(json));
                }
                else
                {
                    File.WriteAllText(filePath, json, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"保存数据失败: {key}, 错误: {ex.Message}");
            }
        }

        public async UniTask SaveDataAsync<T>(string key, T data, bool isBytesData = true) where T : class
        {
            try
            {
                string filePath = GetFilePath(key, isBytesData);
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
            try
            {
                string filePath = GetFilePath(key, isBytesData);
                if (!File.Exists(filePath)) return null;

                if (isBytesData)
                {
                    var bytes = File.ReadAllBytes(filePath);
                    return JsonUtility.FromJson<T>(Encoding.UTF8.GetString(bytes));
                }
                else
                {
                    var json = File.ReadAllText(filePath, Encoding.UTF8);
                    return JsonUtility.FromJson<T>(json);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"加载数据失败: {key}, 错误: {ex.Message}");
                return null;
            }
        }

        public async UniTask<T> ReadDataAsync<T>(string key, bool isBytesData = true) where T : class
        {
            try
            {
                string filePath = GetFilePath(key, isBytesData);
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

        public bool DeleteData(string key, bool isBytesData = true)
        {
            try
            {
                string filePath = GetFilePath(key, isBytesData);
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

        public bool HasData(string key, bool isBytesData = true)
        {
            string filePath = GetFilePath(key, isBytesData);
            return File.Exists(filePath);
        }

        public string GetPersistencePath()
        {
            return _persistenceFolder;
        }

        private string GetFilePath(string key, bool isBytesData = true)
        {
            return Path.Combine(_persistenceFolder, isBytesData ? $"{key}.bytes" : $"{key}.json");
        }


        public void Shutdown()
        {
            Debugger.Log("Shutdown PersistenceDataManager", LogType.FrameNormal);
        }
    }
}