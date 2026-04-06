using System;
using UnityEngine;

namespace MiniGameSDK
{
    public static class SDKManager
    {
        private static IGameSDK m_Instance;

        public static IGameSDK Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    Init();
                }
                return m_Instance;
            }
        }

        private static void Init()
        {
#if SDK_DY
            m_Instance = new DouyinSDK();
#elif SDK_WX
            m_Instance = new WechatSDK();
#elif SDK_KS
            m_Instance = new KuaishouSDK();
#elif SDK_BL
            m_Instance = new BiliSDK();
#elif SDK_ZFB
            m_Instance = new AlipaySDK();
#else
            m_Instance = new EditorSDK();
#endif
            //m_Instance.Init();
        }

        /// <summary>
        /// 获取平台特有接口
        /// </summary>
        public static T GetSpecial<T>() where T : class
        {
            return m_Instance as T;
        }
    }
}