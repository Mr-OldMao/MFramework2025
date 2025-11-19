namespace MFramework.Runtime
{
    public enum UIStateProgressType
    {
        Unstart = 0,

        LoadResStart,
        LoadResEnd,
        InitStart,
        InitEnd,
        ShowStart,
        ShowEnd,
        HideStart,
        HideEnd,
        DestoryStart,
        DestoryEnd,
    }


    public enum UILayerType
    {
        Background = 0,
        Normal = 500,
        Popup = 1000,
        Tips = 1500,
    }
}
