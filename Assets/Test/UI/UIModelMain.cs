using MFramework.Runtime;

namespace GameMain
{
    public class UIModelMain : UIModelBase
    {
        private string m_Title;
        private int m_Score;

        public string Title
        {
            get => m_Title;
            set
            {
                m_Title = value;
                DispatchEvent(GameEventType.TestUIEvent);
            }
        }

        public int Score
        {
            get => m_Score;
            set
            {
                m_Score = value;
                DispatchEvent(GameEventType.TestUIEvent);
            }
        }
    }
}
