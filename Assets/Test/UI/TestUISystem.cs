using GameMain;
using MFramework.Runtime;
using MFramework.Runtime.UI;
using UnityEngine;

public class TestUISystem : MonoBehaviour
{
    private int m_Count = 0;

    private void Start()
    {
        GameEntry.UI.ShowView<UIPanelMain>($"测试ShowView传参{++m_Count}", $"测试ShowViewBefore传参{++m_Count}");
    }

    public async void ShowAPI()
    {
        GameEntry.UI.GetView<UIPanelMain>();

        GameEntry.UI.GetController<UIControlMain>();
        GameEntry.UI.GetController(new UIControlMain());

        GameEntry.UI.GetModel<UIModelMain>();
        GameEntry.UI.GetModel(new UIModelMain());

        GameEntry.UI.ShowView<UIPanelMain>();
        GameEntry.UI.ShowView<UIPanelMain>(null);
        GameEntry.UI.ShowView<UIPanelMain>(null, null);

        GameEntry.UI.HideView<UIPanelMain>();
        GameEntry.UI.HideView<UIPanelMain>(null);
        GameEntry.UI.HideView<UIPanelMain>(null, null);

        await GameEntry.UI.ShowViewAsync<UIPanelMain>();
        await GameEntry.UI.ShowViewAsync<UIPanelMain>(null);
        await GameEntry.UI.ShowViewAsync<UIPanelMain>(null, null);

        await GameEntry.UI.HideViewAsync<UIPanelMain>();
        await GameEntry.UI.HideViewAsync<UIPanelMain>(null);
        await GameEntry.UI.HideViewAsync<UIPanelMain>(null, null);

        GameEntry.UI.Clear<UIPanelMain>();
        GameEntry.UI.Clear(new UIPanelMain());
        GameEntry.UI.ClearAll();
    }

    private async void OnGUI()
    {
        GUIStyle style = new GUIStyle("Button");
        style.fontSize = 36;

        int width = 200;
        int height = 100;

        int curWidth = 0;
        int curHeight = 0;


        if (GUI.Button(new Rect(curWidth, curHeight, width, height), "ShowView", style))
        {
            GameEntry.UI.ShowView<UIPanelMain>($"测试ShowView传参{++m_Count}", $"测试ShowViewBefore传参{++m_Count}");
        }
        curHeight += height;
        if (GUI.Button(new Rect(curWidth, curHeight, width, height), "HideView", style))
        {
            GameEntry.UI.HideView<UIPanelMain>($"测试HideView传参{++m_Count}", $"测试HideViewBefore传参{++m_Count}");
        }
        curHeight += height;
        if (GUI.Button(new Rect(curWidth, curHeight, width, height), "ShowViewAsync", style))
        {
            var model = GameEntry.UI.GetModel<UIModelMain>();
            model.Title = $"test param {++m_Count}";
            await GameEntry.UI.ShowViewAsync<UIPanelMain>(model, $"测试ShowViewAsync传参{++m_Count}");
        }
        curHeight += height;
        if (GUI.Button(new Rect(curWidth, curHeight, width, height), "HideViewAsync", style))
        {
            await GameEntry.UI.HideViewAsync<UIPanelMain>($"测试HideViewAsync传参{++m_Count}", $"测试HideViewAsync传参{++m_Count}");
        }
        curHeight += height;

        if (GUI.Button(new Rect(curWidth, curHeight, width, height), "Show", style))
        {
            GameEntry.UI.GetController<UIControlMain>().Show($"测试Show传参{++m_Count}", $"测试Show传参{++m_Count}");
        }
        curHeight += height;

        if (GUI.Button(new Rect(curWidth, curHeight, width, height), "更改数据", style))
        {
            int data = Random.Range(1000, 9999);
            GameEntry.UI.GetModel<UIModelMain>().Title = $"{data}";
        }
        curHeight += height;
    }
}