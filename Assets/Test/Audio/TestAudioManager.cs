using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace MFramework.Runtime
{
    public class TestAudioManager : MonoBehaviour
    {
        private async UniTask ShowAPI()
        {
            string audioName = string.Empty;
            GameEntry.Audio.PlayBGM(audioName);
            int audioBGMID = await GameEntry.Audio.PlayBGM(audioName, 0.8f, false);

            GameEntry.Audio.PlaySound(audioName);
            GameEntry.Audio.PlaySound(audioName, () => { Debug.Log("音效播放完成回调"); });
            int audioSoundID = await GameEntry.Audio.PlaySound(audioName, () => { Debug.Log("音效播放完成回调"); }, 0.9f, false);

            GameEntry.Audio.StopAudio(audioBGMID);
            GameEntry.Audio.StopAllSoundEffects();
            GameEntry.Audio.StopBGM();

            GameEntry.Audio.SetPause(audioBGMID, true);
            GameEntry.Audio.SetPauseBGM(false);
            GameEntry.Audio.SetMute(audioSoundID, true);
            GameEntry.Audio.SetVolume(audioBGMID, 0.5f);

            GameEntry.Audio.GetAudioState(audioBGMID);
            GameEntry.Audio.GetAudioSourceData(audioBGMID);

            GameEntry.Audio.ClearCache();
        }

        int audioBGMID = 0;
        int audioSoundID = 0;

        private async UniTask OnGUI()
        {
            GUIStyle style = new GUIStyle("Button");
            style.fontSize = 36;

            int width = 300;
            int height = 100;

            int curWidth = 0;
            int curHeight = 0;

            if (GUI.Button(new Rect(curWidth, curHeight, width, height), "播放背景音乐1", style))
            {
                audioBGMID = await GameEntry.Audio.PlayBGM("resFileAudioClipBGM1", 1, true);
                Debugger.Log("播放背景音乐1");
            }
            if (audioBGMID > 0)
            {
                if (GameEntry.Audio.GetAudioState(audioBGMID) != AudioStateType.Pause)
                {
                    if (GUI.Button(new Rect(curWidth + width, curHeight, width, height), "暂停", style))
                    {
                        GameEntry.Audio.SetPause(audioBGMID, true);
                    }
                }
                else if ((GameEntry.Audio.GetAudioState(audioBGMID) == AudioStateType.Pause))
                {
                    if (GUI.Button(new Rect(curWidth + width, curHeight, width, height), "取消暂停", style))
                    {
                        GameEntry.Audio.SetPause(audioBGMID, false);
                    }
                }
                if (GameEntry.Audio.GetAudioSourceData(audioBGMID) != null)
                {
                    if (GUI.Button(new Rect(curWidth + width + width, curHeight, width, height), "停止", style))
                    {
                        GameEntry.Audio.StopAudio(audioBGMID);
                        audioBGMID = -1;
                    }
                }
            }
            curHeight += height;


            if (GUI.Button(new Rect(curWidth, curHeight, width, height), "播放背景音乐2", style))
            {
                GameEntry.Audio.PlayBGM("resFileAudioClipBGM2");
                Debugger.Log("播放背景音乐2");
                audioBGMID = -1;
            }


            curHeight += height;
            if (GUI.Button(new Rect(curWidth, curHeight, width, height), "播放音效1", style))
            {
                audioSoundID = await GameEntry.Audio.PlaySound("resFileAudioClipSound1", () =>
                {
                    Debugger.Log("播放音效1播放完成回调");
                    audioSoundID = -1;
                }, 1, false);
                Debugger.Log("播放音效1 " + audioSoundID);
            }
            if (audioSoundID > 0)
            {
                if (GameEntry.Audio.GetAudioState(audioSoundID) != AudioStateType.Pause)
                {
                    if (GUI.Button(new Rect(curWidth + width, curHeight, width, height), "暂停", style))
                    {
                        GameEntry.Audio.SetPause(audioSoundID, true);
                    }
                }
                else if ((GameEntry.Audio.GetAudioState(audioSoundID) == AudioStateType.Pause))
                {
                    if (GUI.Button(new Rect(curWidth + width, curHeight, width, height), "取消暂停", style))
                    {
                        GameEntry.Audio.SetPause(audioSoundID, false);
                    }
                }
                if (GameEntry.Audio.GetAudioSourceData(audioSoundID) != null)
                {
                    if (GUI.Button(new Rect(curWidth + width + width, curHeight, width, height), "停止", style))
                    {
                        GameEntry.Audio.StopAudio(audioSoundID);
                        audioSoundID = -1;
                    }
                }
            }
            curHeight += height;

            if (GUI.Button(new Rect(curWidth, curHeight, width, height), "播放音效2", style))
            {
                GameEntry.Audio.PlaySound("resFileAudioClipSound2.ogg");
                Debugger.Log("播放音效2");
            }
            curHeight += height;


        }
    }
}
