using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFramework.Runtime
{
	public interface IDataManager : IGameModule
	{
        //T GetConfig<T>(int id) where T : ConfigBase;
        void SaveData(string key, object data);
        T LoadData<T>(string key);
    }

}