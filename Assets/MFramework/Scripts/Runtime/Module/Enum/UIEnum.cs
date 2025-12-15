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

    public enum UIHideType
    {
        /// <summary>
        /// 长期隐藏最佳
        /// </summary>
        SetActive = 0,
        /// <summary>
        /// 频繁切换时最佳
        /// </summary>
        CanvasGroup,
    }
}
