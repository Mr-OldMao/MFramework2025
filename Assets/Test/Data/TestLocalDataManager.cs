using MFramework.Runtime;
using UnityEngine;

namespace GameMain
{
    public class TestLocalDataManager : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        private void ShowAPI()
        {
            GameEntry.LocalData.SaveData("test", "content....");


        }
    }
}
