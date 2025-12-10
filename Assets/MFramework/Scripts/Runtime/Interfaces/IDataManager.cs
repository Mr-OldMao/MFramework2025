//using Cysharp.Threading.Tasks;

//namespace MFramework.Runtime
//{
//    /// <summary>
//    /// 与ResourecesManager功能冗余，数据也是资源，二进制数据、JSON、配置文件等都可以作为 TextAsset 通过 Addressables 管理
//    /// </summary>
//    public interface IDataManager : IGameModule
//    {
//        //UniTask<byte[]> LoadBytesAsync(string dataPath, DataSourceType dataSourceType);
//        //UniTask<string> LoadTextAsync(string dataPath, DataSourceType dataSourceType);
//        //UniTask<T> LoadDataAsync<T>(string dataName, DataSourceType dataSourceType) where T : class;
//        //UniTask PreloadDataAsync(string dataName, DataSourceType dataSourceType);
//        //void UnloadData(string dataName);
//        //void UnloadAllData();

//        T GetData<T>(string dataName) where T : class;
//        bool TryGetData<T>(string dataName, out T data) where T : class;
//        bool IsLoadedData(string dataName);


//        DataLoadState GetDataLoadState(string dataName);

//        void SetDataByPersistence<T>(string dataName, T data) where T : class;
//        T GetDataByPersistence<T>(string dataName) where T : class;
//    }

//    /// <summary>
//    /// 数据加载状态
//    /// </summary>
//    public enum DataLoadState
//    {
//        NotLoaded,
//        Loading,
//        Loaded,
//        Failed
//    }
//    /// <summary>
//    /// 数据来源类型
//    /// </summary>
//    public enum DataSourceType
//    {
//        StreamingAssets = 0,
//        Resources = 1,
//        PersistentData = 2,
//    }
//}