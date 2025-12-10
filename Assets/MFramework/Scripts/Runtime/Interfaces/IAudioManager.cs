using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MFramework.Runtime
{
    public interface IAudioManager : IGameBase
    {
        UniTask<int> PlaySound(string audioName, Action playedCallback, float volume = 1, bool isLoop = false);
        UniTask<int> PlayBGM(string audioName, float volume = 1, bool isLoop = true);

        void SetVolume(int audioId, float volume);
        void SetMute(int audioId, bool isMute);
        void SetLoop(int audioId, bool isLoop);
        void SetPause(int audioId, bool isPause);
        void SetPauseBGM(bool isPause);

        AudioSourceData GetAudioSourceData(int audioId);
        AudioStateType GetAudioState(int audioId);

        void StopAudio(int audioId);
        void StopBGM();
        void StopAllSoundEffects();

        void ClearCache();
    }

    public class AudioSourceData
    {
        public int audioID;
        public string audioName;
        public AudioSourceType audioSourceType;
        public AudioStateType audioStateType;
        public AudioSource audioSource;
        public AudioClip audioClip;
    }

    public enum AudioSourceType
    {
        /// <summary>
        /// 音效，同一时刻允许多个音效源同时播放
        /// </summary>
        SoundEffect,
        /// <summary>
        /// 背景音乐，同一时刻只允许一个背景音乐源播放
        /// </summary>
        BGM
    }

    public enum AudioStateType
    {
        NotPlay = 0,
        Playing,
        Pause
    }
}