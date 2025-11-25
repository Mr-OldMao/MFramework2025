using MFramework.Runtime;
using MFramework.Runtime.UI;
using System.Threading.Tasks;
using UnityEngine;

namespace GameMain
{
    public class TestLocalDataManager : MonoBehaviour
    {
        private async void OnGUI()
        {
            GUIStyle style = new GUIStyle("Button");
            style.fontSize = 36;

            int width = 400;
            int height = 100;

            int curWidth = 0;
            int curHeight = 0;

            string keyNameA = "PlayerData_A";
            string keyNameB = "PlayerData_B";
            if (GUI.Button(new Rect(curWidth, curHeight, width, height), "存储.json数据A", style))
            {
                GameEntry.LocalData.SaveData(keyNameA, new PlayerData { id = 1001, name = "PlayerData_A", isMen = true }, false);
                Debugger.Log($"存储.json数据A");
            }
            curHeight += height;

            if (GUI.Button(new Rect(curWidth, curHeight, width, height), "存储.bytes数据B", style))
            {
                GameEntry.LocalData.SaveData(keyNameB, new PlayerData { id = 1002, name = "PlayerData_B", isMen = false }, true);
                Debugger.Log($"存储.bytes数据B");
            }
            curHeight += height;

            if (GUI.Button(new Rect(curWidth, curHeight, width, height), "读取.json数据A", style))
            {
                var data = GameEntry.LocalData.ReadData<PlayerData>(keyNameA, false);
                Debugger.Log($"读取数据A:{data.ToString()}");
            }
            curHeight += height;

            if (GUI.Button(new Rect(curWidth, curHeight, width, height), "读取.bytes数据B", style))
            {
                var data = GameEntry.LocalData.ReadData<PlayerData>(keyNameB, true);
                Debugger.Log($"读取数据B:{data.ToString()}");
            }
            curHeight += height;

            if (GUI.Button(new Rect(curWidth, curHeight, width, height), "删除数据A", style))
            {
                var data = GameEntry.LocalData.DeleteData(keyNameA, false);
                Debugger.Log($"删除数据A:{data}");
            }
            curHeight += height;
            if (GUI.Button(new Rect(curWidth, curHeight, width, height), "删除数据B", style))
            {
                var data = GameEntry.LocalData.DeleteData(keyNameB, true);
                Debugger.Log($"删除数据B:{data}");
            }
            curHeight += height;

            if (GUI.Button(new Rect(curWidth, curHeight, width, height), "获取数据A", style))
            {
                var data = GameEntry.LocalData.HasData(keyNameA, false);
                Debugger.Log($"获取数据A:{data}");
            }
            curHeight += height;
            if (GUI.Button(new Rect(curWidth, curHeight, width, height), "获取数据B", style))
            {
                var data = GameEntry.LocalData.HasData(keyNameB, true);
                Debugger.Log($"获取数据B:{data}");
            }
            curHeight += height;

            if (GUI.Button(new Rect(curWidth, curHeight, width, height), "获取本地持久化数据路径", style))
            {
                var data = GameEntry.LocalData.GetPersistencePath();
                Debugger.Log($"获取本地持久化数据路径:{data}");
            }
            curHeight += height;
        }

        private async Task ShowAPI()
        {
            string key = "PlayerData";
            GameEntry.LocalData.SaveData(key, new PlayerData { }, true);
            GameEntry.LocalData.SaveData(key, new PlayerData { }, false);
            await GameEntry.LocalData.SaveDataAsync(key, new PlayerData { }, true);
            await GameEntry.LocalData.SaveDataAsync(key, new PlayerData { }, false);


            GameEntry.LocalData.ReadData<PlayerData>(key, true);
            GameEntry.LocalData.ReadData<PlayerData>(key, false);
            await GameEntry.LocalData.ReadDataAsync<PlayerData>(key, true);
            await GameEntry.LocalData.ReadDataAsync<PlayerData>(key, false);

            GameEntry.LocalData.DeleteData(key);
            GameEntry.LocalData.HasData(key);
        }

        class PlayerData
        {
            public int id;
            public string name;
            public bool isMen;

            public new string ToString()
            {
                return $"id:{id},name:{name},isMen:{isMen}";
            }
        }
    }
}
