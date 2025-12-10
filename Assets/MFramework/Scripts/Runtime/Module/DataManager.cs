//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Text;
//using Cysharp.Threading.Tasks;
//using UnityEngine;
//using UnityEngine.Networking;

//namespace MFramework.Runtime
//{
//    public class DataManager : IDataManager, IDisposable
//    {
//        #region 内部类和结构

//        [Serializable]
//        private class DataItem
//        {
//            public string DataName;
//            public object Data;
//            public DataLoadState State;
//            public string LoadPath;
//            public Type DataType;
//            public DataSourceType SourceType;
//        }

//        [Serializable]
//        private class PersistenceDataWrapper
//        {
//            public string DataJson;
//            public string DataTypeName;
//        }

//        #endregion

//        #region 私有字段

//        private readonly Dictionary<string, DataItem> _dataCache = new Dictionary<string, DataItem>();
//        private readonly object _lockObject = new object();
//        private  string _persistenceDataFolder;

//        // 各数据源的基路径配置
//        private readonly Dictionary<DataSourceType, string> _sourceBasePaths = new Dictionary<DataSourceType, string>();

//        #endregion

//        #region 初始化
//        public UniTask Init()
//        {
//            // 初始化持久化数据目录
//            _persistenceDataFolder = Path.Combine(Application.persistentDataPath, "GameData");
//            if (!Directory.Exists(_persistenceDataFolder))
//            {
//                Directory.CreateDirectory(_persistenceDataFolder);
//            }

//            InitializeSourceBasePaths();
//            return UniTask.CompletedTask;
//        }

//        private void InitializeSourceBasePaths()
//        {
//            // 配置各数据源的基路径
//            _sourceBasePaths[DataSourceType.StreamingAssets] = Application.streamingAssetsPath;
//            _sourceBasePaths[DataSourceType.Resources] = ""; // Resources不需要基路径
//            _sourceBasePaths[DataSourceType.PersistentData] = Application.persistentDataPath;
//        }

//        public void Shutdown()
//        {
//            Debugger.Log("TODO DataManager Shutdown", LogType.FrameCore);
//        }

//        #endregion

//        #region 核心数据加载接口

//        public async UniTask<byte[]> LoadBytesAsync(string dataPath, DataSourceType dataSourceType)
//        {
//            if (string.IsNullOrEmpty(dataPath))
//                throw new ArgumentException("数据路径不能为空");

//            try
//            {
//                byte[] data = null;

//                switch (dataSourceType)
//                {
//                    case DataSourceType.StreamingAssets:
//                        data = await LoadBytesFromStreamingAssetsAsync(dataPath);
//                        break;
//                    case DataSourceType.Resources:
//                        data = await LoadBytesFromResourcesAsync(dataPath);
//                        break;
//                    case DataSourceType.PersistentData:
//                        data = await LoadBytesFromPersistentDataAsync(dataPath);
//                        break;
//                    default:
//                        throw new ArgumentException($"不支持的数据源类型: {dataSourceType}");
//                }

//                return data;
//            }
//            catch (Exception ex)
//            {
//                Debugger.LogError($"加载字节数据失败: {dataPath}, 数据源: {dataSourceType}, 错误: {ex.Message}");
//                throw;
//            }
//        }

//        public async UniTask<string> LoadTextAsync(string dataPath, DataSourceType dataSourceType)
//        {
//            var bytes = await LoadBytesAsync(dataPath, dataSourceType);
//            return Encoding.UTF8.GetString(bytes);
//        }

//        public async UniTask<T> LoadDataAsync<T>(string dataName, DataSourceType dataSourceType) where T : class
//        {
//            if (string.IsNullOrEmpty(dataName))
//                throw new ArgumentException("数据名称不能为空");

//            string cacheKey = GetCacheKey(dataName, dataSourceType);

//            lock (_lockObject)
//            {
//                // 检查缓存
//                if (_dataCache.TryGetValue(cacheKey, out var cachedItem))
//                {
//                    if (cachedItem.State == DataLoadState.Loaded)
//                    {
//                        return cachedItem.Data as T;
//                    }
//                    else if (cachedItem.State == DataLoadState.Loading)
//                    {
//                        throw new InvalidOperationException($"数据正在加载中: {dataName}");
//                    }
//                }

//                // 创建新的加载项
//                var newItem = new DataItem
//                {
//                    DataName = dataName,
//                    State = DataLoadState.Loading,
//                    DataType = typeof(T),
//                    SourceType = dataSourceType
//                };
//                _dataCache[cacheKey] = newItem;
//            }

//            try
//            {
//                T data = null;

//                // 根据数据类型选择加载策略
//                if (typeof(T) == typeof(byte[]))
//                {
//                    var bytes = await LoadBytesAsync(dataName, dataSourceType);
//                    data = bytes as T;
//                }
//                else if (typeof(T) == typeof(string))
//                {
//                    var text = await LoadTextAsync(dataName, dataSourceType);
//                    data = text as T;
//                }
//                else
//                {
//                    // 对于复杂对象，先加载字节数据，然后反序列化
//                    var bytes = await LoadBytesAsync(dataName, dataSourceType);
//                    data = DeserializeData<T>(bytes, dataName);
//                }

//                lock (_lockObject)
//                {
//                    if (_dataCache.TryGetValue(cacheKey, out var item))
//                    {
//                        item.Data = data;
//                        item.State = DataLoadState.Loaded;
//                    }
//                }

//                return data;
//            }
//            catch (Exception ex)
//            {
//                lock (_lockObject)
//                {
//                    if (_dataCache.TryGetValue(cacheKey, out var item))
//                    {
//                        item.State = DataLoadState.Failed;
//                    }
//                }

//                Debug.LogError($"加载数据失败: {dataName}, 数据源: {dataSourceType}, 错误: {ex.Message}");
//                throw;
//            }
//        }

//        public async UniTask PreloadDataAsync(string dataName, DataSourceType dataSourceType)
//        {
//            string cacheKey = GetCacheKey(dataName, dataSourceType);

//            if (IsLoadedData(dataName))
//                return;

//            try
//            {
//                // 预加载只是确保数据在缓存中，不关心具体类型
//                var bytes = await LoadBytesAsync(dataName, dataSourceType);

//                lock (_lockObject)
//                {
//                    if (!_dataCache.ContainsKey(cacheKey))
//                    {
//                        _dataCache[cacheKey] = new DataItem
//                        {
//                            DataName = dataName,
//                            Data = bytes,
//                            State = DataLoadState.Loaded,
//                            DataType = typeof(byte[]),
//                            SourceType = dataSourceType
//                        };
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                Debug.LogWarning($"预加载数据失败: {dataName}, 数据源: {dataSourceType}, 错误: {ex.Message}");
//            }
//        }

//        #endregion

//        #region 数据访问接口

//        public T GetData<T>(string dataName) where T : class
//        {
//            if (string.IsNullOrEmpty(dataName))
//                throw new ArgumentException("数据名称不能为空");

//            // 在所有数据源中查找
//            foreach (var sourceType in Enum.GetValues(typeof(DataSourceType)))
//            {
//                string cacheKey = GetCacheKey(dataName, (DataSourceType)sourceType);

//                lock (_lockObject)
//                {
//                    if (_dataCache.TryGetValue(cacheKey, out var item) &&
//                        item.State == DataLoadState.Loaded)
//                    {
//                        if (item.Data is T typedData)
//                        {
//                            return typedData;
//                        }
//                        else
//                        {
//                            throw new InvalidCastException($"数据类型不匹配: 期望 {typeof(T).Name}, 实际 {item.Data.GetType().Name}");
//                        }
//                    }
//                }
//            }

//            throw new InvalidOperationException($"数据未加载或加载失败: {dataName}");
//        }

//        public bool TryGetData<T>(string dataName, out T data) where T : class
//        {
//            data = null;

//            if (string.IsNullOrEmpty(dataName))
//                return false;

//            // 在所有数据源中查找
//            foreach (var sourceType in Enum.GetValues(typeof(DataSourceType)))
//            {
//                string cacheKey = GetCacheKey(dataName, (DataSourceType)sourceType);

//                lock (_lockObject)
//                {
//                    if (_dataCache.TryGetValue(cacheKey, out var item) &&
//                        item.State == DataLoadState.Loaded &&
//                        item.Data is T typedData)
//                    {
//                        data = typedData;
//                        return true;
//                    }
//                }
//            }

//            return false;
//        }

//        public bool IsLoadedData(string dataName)
//        {
//            // 检查是否在任何数据源中已加载
//            foreach (var sourceType in Enum.GetValues(typeof(DataSourceType)))
//            {
//                string cacheKey = GetCacheKey(dataName, (DataSourceType)sourceType);

//                lock (_lockObject)
//                {
//                    if (_dataCache.TryGetValue(cacheKey, out var item) &&
//                        item.State == DataLoadState.Loaded)
//                    {
//                        return true;
//                    }
//                }
//            }

//            return false;
//        }

//        #endregion

//        #region 数据管理接口

//        public void UnloadData(string dataName)
//        {
//            if (string.IsNullOrEmpty(dataName))
//                return;

//            // 从所有数据源中卸载
//            foreach (var sourceType in Enum.GetValues(typeof(DataSourceType)))
//            {
//                string cacheKey = GetCacheKey(dataName, (DataSourceType)sourceType);

//                lock (_lockObject)
//                {
//                    if (_dataCache.ContainsKey(cacheKey))
//                    {
//                        var item = _dataCache[cacheKey];

//                        // 如果数据是IDisposable，则释放资源
//                        if (item.Data is IDisposable disposable)
//                        {
//                            disposable.Dispose();
//                        }

//                        _dataCache.Remove(cacheKey);
//                    }
//                }
//            }
//        }

//        public void UnloadAllData()
//        {
//            lock (_lockObject)
//            {
//                foreach (var item in _dataCache.Values)
//                {
//                    if (item.Data is IDisposable disposable)
//                    {
//                        disposable.Dispose();
//                    }
//                }
//                _dataCache.Clear();
//            }
//        }

//        public DataLoadState GetDataLoadState(string dataName)
//        {
//            // 返回优先级最高的数据源状态
//            foreach (var sourceType in Enum.GetValues(typeof(DataSourceType)))
//            {
//                string cacheKey = GetCacheKey(dataName, (DataSourceType)sourceType);

//                lock (_lockObject)
//                {
//                    if (_dataCache.TryGetValue(cacheKey, out var item))
//                    {
//                        return item.State;
//                    }
//                }
//            }
//            return DataLoadState.NotLoaded;
//        }

//        #endregion

//        #region 持久化数据接口

//        public void SetDataByPersistence<T>(string dataName, T data) where T : class
//        {
//            if (string.IsNullOrEmpty(dataName) || data == null)
//                return;

//            try
//            {
//                string filePath = Path.Combine(_persistenceDataFolder, $"{dataName}.json");

//                var wrapper = new PersistenceDataWrapper
//                {
//                    DataJson = JsonUtility.ToJson(data),
//                    DataTypeName = typeof(T).AssemblyQualifiedName
//                };

//                string json = JsonUtility.ToJson(wrapper);
//                File.WriteAllText(filePath, json, Encoding.UTF8);

//                Debug.Log($"持久化数据保存成功: {dataName}");
//            }
//            catch (Exception ex)
//            {
//                Debug.LogError($"保存持久化数据失败: {dataName}, 错误: {ex.Message}");
//            }
//        }

//        public T GetDataByPersistence<T>(string dataName) where T : class
//        {
//            if (string.IsNullOrEmpty(dataName))
//                return null;

//            try
//            {
//                string filePath = Path.Combine(_persistenceDataFolder, $"{dataName}.json");

//                if (!File.Exists(filePath))
//                    return null;

//                string json = File.ReadAllText(filePath, Encoding.UTF8);
//                var wrapper = JsonUtility.FromJson<PersistenceDataWrapper>(json);

//                // 验证类型
//                var expectedType = typeof(T);
//                var actualType = Type.GetType(wrapper.DataTypeName);

//                if (actualType != expectedType)
//                {
//                    Debug.LogWarning($"持久化数据类型不匹配: 期望 {expectedType.Name}, 实际 {actualType?.Name ?? "未知"}");
//                    return null;
//                }

//                T data = JsonUtility.FromJson<T>(wrapper.DataJson);
//                return data;
//            }
//            catch (Exception ex)
//            {
//                Debug.LogError($"读取持久化数据失败: {dataName}, 错误: {ex.Message}");
//                return null;
//            }
//        }

//        #endregion

//        #region 私有方法 - 数据源加载

//        private async UniTask<byte[]> LoadBytesFromStreamingAssetsAsync(string dataPath)
//        {
//            string fullPath = Path.Combine(Application.streamingAssetsPath, dataPath);

//#if UNITY_ANDROID && !UNITY_EDITOR
//            // Android平台需要使用UnityWebRequest
//            using (var request = UnityWebRequest.Get(fullPath))
//            {
//                var operation = request.SendWebRequest();
                
//                while (!operation.isDone)
//                {
//                    await UniTask.Yield();
//                }
                
//                if (request.result != UnityWebRequest.Result.Success)
//                {
//                    throw new Exception($"StreamingAssets加载失败: {request.error}");
//                }
                
//                return request.downloadHandler.data;
//            }
//#else
//            if (!File.Exists(fullPath))
//                throw new FileNotFoundException($"文件不存在: {fullPath}");

//            return await File.ReadAllBytesAsync(fullPath);
//#endif
//        }

//        private async UniTask<byte[]> LoadBytesFromResourcesAsync(string dataPath)
//        {
//            // 移除文件扩展名（Resources.Load不需要扩展名）
//            string resourcePath = Path.GetFileNameWithoutExtension(dataPath);

//            var resourceRequest = Resources.LoadAsync<TextAsset>(resourcePath);

//            while (!resourceRequest.isDone)
//            {
//                await UniTask.Yield();
//            }

//            var textAsset = resourceRequest.asset as TextAsset;
//            if (textAsset == null)
//                throw new Exception($"Resources中未找到资源: {resourcePath}");

//            return textAsset.bytes;
//        }

//        private async UniTask<byte[]> LoadBytesFromPersistentDataAsync(string dataPath)
//        {
//            string fullPath = Path.Combine(Application.persistentDataPath, dataPath);

//            if (!File.Exists(fullPath))
//                throw new FileNotFoundException($"文件不存在: {fullPath}");

//            return await File.ReadAllBytesAsync(fullPath);
//        }

//        #endregion

//        #region 私有方法 - 工具方法

//        private string GetCacheKey(string dataName, DataSourceType sourceType)
//        {
//            return $"{sourceType}:{dataName}";
//        }

//        private T DeserializeData<T>(byte[] bytes, string dataName) where T : class
//        {
//            // 这里实现数据的反序列化逻辑
//            // 可以根据数据名称或类型选择不同的反序列化方式

//            if (typeof(T) == typeof(string))
//            {
//                return Encoding.UTF8.GetString(bytes) as T;
//            }

//            // 如果是FlatBuffers数据
//            if (dataName.EndsWith(".fbs") || dataName.Contains("flatbuffer"))
//            {
//                // 调用FlatBuffers反序列化逻辑
//                // return FlatBufferConverter.Deserialize<T>(bytes);
//                throw new NotImplementedException("FlatBuffers反序列化需要具体实现");
//            }

//            // 默认使用JSON反序列化
//            string json = Encoding.UTF8.GetString(bytes);
//            try
//            {
//                return JsonUtility.FromJson<T>(json);
//            }
//            catch (Exception ex)
//            {
//                Debug.LogError($"JSON反序列化失败: {dataName}, 错误: {ex.Message}");
//                throw;
//            }
//        }

//        #endregion

//        #region IDisposable 实现

//        public void Dispose()
//        {
//            UnloadAllData();
//        }

//        #endregion

//        #region 扩展方法 - 便捷访问

//        /// <summary>
//        /// 从StreamingAssets加载数据（便捷方法）
//        /// </summary>
//        public UniTask<T> LoadFromStreamingAssetsAsync<T>(string dataPath) where T : class
//        {
//            return LoadDataAsync<T>(dataPath, DataSourceType.StreamingAssets);
//        }

//        /// <summary>
//        /// 从Resources加载数据（便捷方法）
//        /// </summary>
//        public UniTask<T> LoadFromResourcesAsync<T>(string dataPath) where T : class
//        {
//            return LoadDataAsync<T>(dataPath, DataSourceType.Resources);
//        }

//        /// <summary>
//        /// 从PersistentData加载数据（便捷方法）
//        /// </summary>
//        public UniTask<T> LoadFromPersistentDataAsync<T>(string dataPath) where T : class
//        {
//            return LoadDataAsync<T>(dataPath, DataSourceType.PersistentData);
//        }
//        #endregion
//    }
//}