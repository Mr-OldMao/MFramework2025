using MFramework.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
                NotifyDataChanged(nameof(Title));
            }
        }

        public int Score
        {
            get => m_Score;
            set
            {
                m_Score = value;
                NotifyDataChanged(nameof(Score));
            }
        }
    }
}
