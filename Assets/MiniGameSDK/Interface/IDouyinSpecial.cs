using System;

namespace MiniGameSDK
{
    public interface IDouyinSpecial
    {
        void OnInitCompletedCallback();

#if SDK_DY
        //侧边栏
        void ShowSideBar(Action<bool> callback);

        //排行榜
        void ShowRankList();
        void SetRankListData(int value);
        void GetRankData(Action<TTSDK.TTRank.RankData> callback);
        void GetRankData(Action<int> callback);


        void ShowRevisitGuide(Action<bool> callback = null);

        void CheckScene();
#endif

    }
}