using Cysharp.Threading.Tasks;

namespace MFramework.Runtime
{
    public interface IPersistenceDataManager : IGameBase
    {
        void SaveData<T>(string key, T data, bool isBytesData = true) where T : class;
        UniTask SaveDataAsync<T>(string key, T data, bool isBytesData = true) where T : class;

        T ReadData<T>(string key, bool isBytesData = true) where T : class;
        UniTask<T> ReadDataAsync<T>(string key, bool isBytesData = true) where T : class;

        bool DeleteData(string key, bool isBytesData = true);
        bool HasData(string key, bool isBytesData = true);
    }
}
