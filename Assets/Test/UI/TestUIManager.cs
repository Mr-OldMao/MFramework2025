using GameMain;
using MFramework.Runtime;
using MFramework.Runtime.UI;
using UnityEngine;

public class TestUIManager : MonoBehaviour
{
    public async void ShowAPI()
    {
        GameEntry.UI.GetView<UIPanelMain>();

        GameEntry.UI.GetController<UIControlMain>();
        GameEntry.UI.GetController(new UIControlMain());

        GameEntry.UI.GetModel<UIModelMain>();
        GameEntry.UI.GetModel(new UIModelMain(null));

        GameEntry.UI.ShowView<UIPanelMain>();

        GameEntry.UI.HideView<UIPanelMain>();

        await GameEntry.UI.ShowViewAsync<UIPanelMain>();

        await GameEntry.UI.HideViewAsync<UIPanelMain>();

        GameEntry.UI.RemoveView<UIPanelMain>();
        GameEntry.UI.RemoveView(new UIPanelMain());
        GameEntry.UI.RemoveAllView();
    }

    private async void OnGUI()
    {
        GUIStyle style = new GUIStyle("Button");
        style.fontSize = 36;

        int width = 300;
        int height = 100;

        int curWidth = 0;
        int curHeight = 0;


        if (GUI.Button(new Rect(curWidth, curHeight, width, height), "ShowView", style))
        {
            GameEntry.UI.ShowView<UIPanelMain>();
        }
        curHeight += height;
        if (GUI.Button(new Rect(curWidth, curHeight, width, height), "HideView", style))
        {
            GameEntry.UI.HideView<UIPanelMain>();
        }
        curHeight += height;
        if (GUI.Button(new Rect(curWidth, curHeight, width, height), "ShowViewAsync", style))
        {
            await GameEntry.UI.ShowViewAsync<UIPanelMain>();
        }
        curHeight += height;
        if (GUI.Button(new Rect(curWidth, curHeight, width, height), "HideViewAsync", style))
        {
            await GameEntry.UI.HideViewAsync<UIPanelMain>();
        }
        curHeight += height;

        if (GUI.Button(new Rect(curWidth, curHeight, width, height), "更改数据", style))
        {
            int data = Random.Range(1000, 9999);
            GameEntry.UI.GetModel<UIModelMain>().UpdateTitle($"{data}");
        }
        curHeight += height;

        if (GUI.Button(new Rect(curWidth, curHeight, width, height), "更改图片", style))
        {
            GameEntry.UI.GetView<UIPanelMain>().ChangeSprite();
        }
        curHeight += height;

        if (GUI.Button(new Rect(curWidth, curHeight, width, height), "销毁窗体", style))
        {
            GameEntry.UI.RemoveView<UIPanelMain>();
        }
        curHeight += height;
    }
}