// 使用方式
using MFramework.Runtime;
using MFramework.Runtime.UI;
using System.Threading.Tasks;
using UnityEngine;

public class TestUISystem : MonoBehaviour
{
    private async void Start()
    {
        await Task.Delay(2000);

        TestOpenUI();
    }
    public async void TestOpenUI()
    {
        // 打开UI
        var ui = await GameEntry.UI.OpenView<UIPanelMain>("测试标题");

        await Task.Delay(2000);


        //// 关闭UI
        //GameEntry.UI.CloseView<UIPanelMain>();
    }
}