using MFramework.Runtime;
using Cysharp.Threading.Tasks;
using GameMain.Generate.FlatBuffers;

namespace GameMain
{
    public  class UIControlSidebar : UIControllerBase
    {
        public int rewardType;
        public FB_reward_sidebar fB_Reward_Sidebar;
        public override async UniTask Init(IUIView view, IUIModel model)
        {
            rewardType = GameMainLogic.Instance.GetUserDataSidebar().rewardType;
            fB_Reward_Sidebar = DataTools.GetRewardSidebar(rewardType);

            await base.Init(view, model);
        }


        public string GetRewardDes()
        {
            string des = string.Empty;
            switch (rewardType)
            {
                case 1:
                case 2:
                    des = string.Format(fB_Reward_Sidebar.Des, GameMainLogic.Instance.GetUserDataSidebar().rewardValue);
                    break;
                case 3:
                    des = fB_Reward_Sidebar.Des;
                    break;
            }
            return des;
        }
    }
}
