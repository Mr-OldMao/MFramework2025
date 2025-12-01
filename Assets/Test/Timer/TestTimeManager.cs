using UnityEngine;

namespace MFramework.Runtime
{
    public class TestTimeManager : MonoBehaviour
    {
        private void ShowAPI()
        {
            GameEntry.Timer.IsStartingUp = true;
            GameEntry.Timer.AddDelayTimer(1000, () => { Debugger.Log("callback"); });
            GameEntry.Timer.AddDelayTimer(1.5f, () => { Debugger.Log("callback"); });
            GameEntry.Timer.AddDelayTimer(1.5f, 0.5f, () => { Debugger.Log("callback"); });
            GameEntry.Timer.AddDelayTimer(1.5f, 0.5f, () => { Debugger.Log("callback"); }, () => { Debugger.Log("end callback"); }, 10);
            GameEntry.Timer.GetTimerInfo(GameEntry.Timer.AddDelayTimer(1.5f, 0.5f,
                () => { Debugger.Log("callback"); },
                () => { Debugger.Log("end callback"); }, 10));
            GameEntry.Timer.ToString(GameEntry.Timer.AddDelayTimer(1.5f, 0.5f,
                () => { Debugger.Log("callback"); },
                () => { Debugger.Log("end callback"); }, -1));
        }

        int timerNumA = 0;
        int timerNumB = 0;
        int timerNumC = 0;
        int timerNumD = 0;

        private void OnGUI()
        {
            GUIStyle style = new GUIStyle("Button");
            style.fontSize = 36;

            int width = 600;
            int height = 100;

            int curWidth = 0;
            int curHeight = 0;

            if (!GameEntry.Timer.IsStartingUp)
            {
                if (GUI.Button(new Rect(curWidth, curHeight, width, height), "启动计时器", style))
                {
                    GameEntry.Timer.IsStartingUp = true;
                    Debugger.Log("启动计时器");
                }
                curHeight += height;
            }
            else
            {
                if (GUI.Button(new Rect(curWidth, curHeight, width, height), "暂停计时器", style))
                {
                    GameEntry.Timer.IsStartingUp = false;
                    Debugger.Log("暂停计时器");
                }
                curHeight += height;
            }


            if (GUI.Button(new Rect(curWidth, curHeight, width, height), "注册延时1秒执行", style))
            {
                timerNumA = GameEntry.Timer.AddDelayTimer(1000, () =>
                {
                    Debugger.Log($"callback，timeNum：{timerNumA}");
                    timerNumA = 0;
                });
                Debugger.Log("延时1秒执行,timeNum:" + timerNumA);
            }
            if (timerNumA > 0)
            {
                if (GUI.Button(new Rect(curWidth + width, curHeight, 200, height), "注册信息", style))
                {
                    var tiemrInfo = GameEntry.Timer.GetTimerInfo(timerNumA);
                    Debugger.Log($"获取计时器注册信息,id:{tiemrInfo.timerId},delaySeconds:{tiemrInfo.targetDelaySeconds},curDelaySeconds:{tiemrInfo.curDelaySeconds}");
                }
                if (GUI.Button(new Rect(curWidth + width + 200, curHeight, 300, height), "移除延时事件", style))
                {
                    GameEntry.Timer.RemoveDelayTimer(timerNumA);
                    timerNumA = 0;
                    Debugger.Log("移除延时事件,timeNum:" + timerNumA);
                }
            }
            curHeight += height;


            if (GUI.Button(new Rect(curWidth, curHeight, width, height), "注册延时2.5秒执行", style))
            {
                timerNumB = GameEntry.Timer.AddDelayTimer(3.5f, () =>
                {
                    Debugger.Log($"callback，timeNum：{timerNumB}");
                    timerNumB = 0;
                });
                Debugger.Log("延时2.5秒执行,timeNum:" + timerNumB);
            }
            if (timerNumB > 0)
            {
                if (GUI.Button(new Rect(curWidth + width, curHeight, 200, height), "详细信息", style))
                {
                    Debugger.Log(GameEntry.Timer.ToString(timerNumB));
                }
                if (GUI.Button(new Rect(curWidth + width + 200, curHeight, 300, height), "移除延时事件", style))
                {
                    GameEntry.Timer.RemoveDelayTimer(timerNumB);
                    timerNumB = 0;
                    Debugger.Log("移除延时事件,timeNum:" + timerNumB);
                }
            }
            curHeight += height;


            if (GUI.Button(new Rect(curWidth, curHeight, width, height), "注册延时3.5秒执行10次间隔0.5秒", style))
            {
                timerNumC = GameEntry.Timer.AddDelayTimer(3.5f, 0.5f, () =>
                {
                    Debugger.Log($"callback，timeNum：{timerNumC}");
                }, () =>
                {
                    Debugger.Log($"end callback，timeNum：{timerNumC}");
                    timerNumC = 0;
                }, 10);
                Debugger.Log("注册延时3.5秒执行10次间隔0.5秒,timeNum:" + timerNumC);
            }
            if (timerNumC > 0)
            {
                if (GUI.Button(new Rect(curWidth + width, curHeight, 200, height), "详细信息", style))
                {
                    Debugger.Log(GameEntry.Timer.ToString(timerNumC));
                }
                if (GUI.Button(new Rect(curWidth + width + 200, curHeight, 300, height), "移除延时事件", style))
                {
                    GameEntry.Timer.RemoveDelayTimer(timerNumC);
                    timerNumC = 0;
                    Debugger.Log("移除延时事件,timeNum:" + timerNumC);
                }
            }
            curHeight += height;


            if (GUI.Button(new Rect(curWidth, curHeight, width, height), "注册延时1.5秒执行无限次间隔1秒", style))
            {
                timerNumD = GameEntry.Timer.AddDelayTimer(1.5f, 1f, () =>
                {
                    Debugger.Log($"callback，timeNum：{timerNumD}");
                }, () =>
                {
                    Debugger.Log($"end callback，timeNum：{timerNumD}");
                    timerNumD = 0;
                }, -1);
                Debugger.Log("注册延时1.5秒执行无限次间隔1秒,timeNum:" + timerNumD);
            }
            if (timerNumD > 0)
            {
                if (GUI.Button(new Rect(curWidth + width, curHeight, 200, height), "详细信息", style))
                {
                    Debugger.Log(GameEntry.Timer.ToString(timerNumD));
                }
                if (GUI.Button(new Rect(curWidth + width + 200, curHeight, 300, height), "移除延时事件", style))
                {
                    GameEntry.Timer.RemoveDelayTimer(timerNumD);
                    timerNumD = 0;
                    Debugger.Log("移除延时事件,timeNum:" + timerNumD);
                }
            }
            curHeight += height;
        }
    }
}
