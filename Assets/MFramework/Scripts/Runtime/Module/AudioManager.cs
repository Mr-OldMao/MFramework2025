using MFramework.Runtime.Extend;
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MFramework.Runtime
{
    public class AudioManager : IAudioManager
    {
        // 音频数据存储 k-audioID
        private Dictionary<int, AudioSourceData> m_DicAuidoSourceData = new Dictionary<int, AudioSourceData>();
        private AudioSourceData m_CurBGMAudioSourceData;

        // 对象池管理音效
        private Queue<AudioSource> m_PoolSoundEffects = new Queue<AudioSource>();
        private List<AudioSource> m_UsedSoundEffects = new List<AudioSource>();
        // 音频资源缓存
        private Dictionary<string, AudioClip> _audioClipCache = new Dictionary<string, AudioClip>();

        private Transform m_SoundEffectContainer;
        private int m_AudioID;
        private int NewAudioID => ++m_AudioID;

        private const int m_InitialPoolSize = 2;


        public UniTask Init()
        {
            Transform audioSoundsContainer = new GameObject("AudioSoundsContainer").transform;
            m_SoundEffectContainer = new GameObject("SoundEffects").transform;
            m_SoundEffectContainer.SetParent(audioSoundsContainer);

            Transform _bgmContainer = new GameObject("BGM").transform;
            _bgmContainer.SetParent(audioSoundsContainer);

            // 初始化对象池
            InitializePool();

            // 创建背景音乐专用的AudioSource
            GameObject bgmObj = new GameObject("BGM_Source");
            bgmObj.transform.SetParent(_bgmContainer);

            m_CurBGMAudioSourceData = new AudioSourceData();
            m_CurBGMAudioSourceData.audioSource = bgmObj.AddComponent<AudioSource>();
            m_CurBGMAudioSourceData.audioSource.loop = true;

            m_AudioID = 0;

            UnityEngine.Object.DontDestroyOnLoad(audioSoundsContainer);
            return UniTask.CompletedTask;
        }

        #region 对象池
        private void InitializePool()
        {
            for (int i = 0; i < m_InitialPoolSize; i++)
            {
                CreatePooledAudioSource();
            }
        }

        private AudioSource CreatePooledAudioSource()
        {
            GameObject soundObj = new GameObject($"SoundEffect_{m_PoolSoundEffects.Count}");
            soundObj.transform.SetParent(m_SoundEffectContainer);

            AudioSource audioSource = soundObj.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;

            m_PoolSoundEffects.Enqueue(audioSource);
            return audioSource;
        }

        private AudioSource GetPooledAudioSource()
        {
            if (m_PoolSoundEffects.Count == 0)
            {
                CreatePooledAudioSource();
            }

            AudioSource audioSource = m_PoolSoundEffects.Dequeue();
            m_UsedSoundEffects.Add(audioSource);
            return audioSource;
        }

        private void ReturnToPool(AudioSource audioSource, int audioId)
        {
            audioSource.Stop();
            audioSource.clip = null;

            m_UsedSoundEffects.Remove(audioSource);
            m_PoolSoundEffects.Enqueue(audioSource);

            m_DicAuidoSourceData.Remove(audioId);
        }
        #endregion


        private async UniTask<AudioClip> LoadAudioClip(string audioName, AudioSourceType audioSourceType)
        {
            switch (audioSourceType)
            {
                case AudioSourceType.BGM:
                    audioName = SystemConstantData.PATH_AUDIO_BGM + audioName;
                    break;
                case AudioSourceType.SoundEffect:
                    audioName = SystemConstantData.PATH_AUDIO_SOUND + audioName;
                    break;
            }

            // 检查缓存
            if (_audioClipCache.TryGetValue(audioName, out AudioClip cachedClip))
            {
                return cachedClip;
            }

            AudioClip clip = await GameEntry.Resource.LoadAssetAsync<AudioClip>(audioName);

            if (clip != null)
            {
                _audioClipCache[audioName] = clip;
            }
            else
            {
                Debugger.LogError($"Audio clip not found: {audioName}", LogType.FrameCore);
            }
            return clip;
        }
        #region Public

        public async void PlayBGM(string audioName)
        {
            await PlayBGM(audioName, 1f, true);
        }

        public async void PlaySound(string audioName)
        {
            await PlaySound(audioName, null, 1f, false);
        }

        public async void PlaySound(string audioName, Action playedCallback = null)
        {
            await PlaySound(audioName, playedCallback, 1f, false);
        }

        public async UniTask<int> PlaySound(string audioName, Action playedCallback = null, float volume = 1, bool isLoop = false)
        {
            AudioClip clip = await LoadAudioClip(audioName, AudioSourceType.SoundEffect);
            if (clip == null)
            {
                return 0;
            }
            AudioSource audioSource = GetPooledAudioSource();
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.loop = isLoop;

            // 创建或更新音频数据 
            int newAudioID = NewAudioID;
            var audioData = new AudioSourceData
            {
                audioID = newAudioID,
                audioName = audioName,
                audioStateType = AudioStateType.Playing,
                audioClip = clip,
                audioSource = audioSource,
                audioSourceType = AudioSourceType.SoundEffect
            };
            audioSource.volume = volume;
            audioSource.loop = isLoop;
            audioSource.Play();
            m_DicAuidoSourceData[newAudioID] = audioData;
            audioSource.gameObject.name = $"{audioData.audioSourceType}_{audioName}_{newAudioID}";

            if (!isLoop)
            {
                int timerID = 0;
                timerID = GameEntry.Timer.AddDelayTimer(0, 1, () =>
                {
                    if (!audioSource.isPlaying && audioData.audioStateType != AudioStateType.Pause)
                    {
                        ReturnToPool(audioSource, newAudioID);
                        playedCallback?.Invoke();
                        GameEntry.Timer.RemoveDelayTimer(timerID);
                    }
                });
            }
            else
            {
                Debugger.LogError($"循环音效不会自动结束，无法调用回调", LogType.FrameCore);
            }
            return newAudioID;
        }

        public async UniTask<int> PlayBGM(string audioName, float volume = 1, bool isLoop = true)
        {
            // 如果已经在播放相同的背景音乐，直接返回
            if (m_CurBGMAudioSourceData != null
                    && m_CurBGMAudioSourceData.audioName == audioName
                    && m_CurBGMAudioSourceData.audioSource.isPlaying)
            {
                return default;
            }

            // 停止当前背景音乐
            if (m_CurBGMAudioSourceData?.audioSource != null && m_CurBGMAudioSourceData.audioSource.isPlaying)
            {
                m_CurBGMAudioSourceData.audioSource.Stop();
            }

            AudioClip clip = await LoadAudioClip(audioName, AudioSourceType.BGM);
            if (clip == null)
            {
                return default;
            }
            int oldAudioId = m_CurBGMAudioSourceData != null ? m_CurBGMAudioSourceData.audioID : -1;
            int newAudioID = NewAudioID;
            m_CurBGMAudioSourceData.audioID = newAudioID;
            m_CurBGMAudioSourceData.audioName = audioName;
            m_CurBGMAudioSourceData.audioClip = clip;
            m_CurBGMAudioSourceData.audioStateType = AudioStateType.Playing;
            m_CurBGMAudioSourceData.audioSourceType = AudioSourceType.BGM;

            m_CurBGMAudioSourceData.audioSource.clip = clip;
            m_CurBGMAudioSourceData.audioSource.volume = volume;
            m_CurBGMAudioSourceData.audioSource.loop = isLoop;
            m_CurBGMAudioSourceData.audioSource.Play();
            m_CurBGMAudioSourceData.audioSource.gameObject.name = $"{m_CurBGMAudioSourceData.audioSourceType}_{audioName}_{newAudioID}";

            if (m_DicAuidoSourceData.ContainsKey(oldAudioId))
            {
                m_DicAuidoSourceData.Remove(oldAudioId);
            }
            m_DicAuidoSourceData[newAudioID] = m_CurBGMAudioSourceData;
            return newAudioID;
        }

        public void SetVolume(int audioId, float volume)
        {
            var audioSourceData = GetAudioSourceData(audioId);
            if (audioSourceData != null)
            {
                audioSourceData.audioSource.volume = volume;
            }
        }

        public void SetMute(int audioId, bool isMute)
        {
            var audioSourceData = GetAudioSourceData(audioId);
            if (audioSourceData != null)
            {
                audioSourceData.audioSource.mute = isMute;
            }
        }

        public void SetLoop(int audioId, bool isLoop)
        {
            var audioSourceData = GetAudioSourceData(audioId);
            if (audioSourceData != null)
            {
                audioSourceData.audioSource.loop = isLoop;
            }
        }

        public void SetPause(int audioId, bool isPause)
        {
            var audioSourceData = GetAudioSourceData(audioId);
            if (audioSourceData != null)
            {
                if (isPause)
                {
                    audioSourceData.audioSource.Pause();
                    audioSourceData.audioStateType = AudioStateType.Pause;
                }
                else
                {
                    audioSourceData.audioSource.UnPause();
                    audioSourceData.audioStateType = AudioStateType.Playing;
                }
            }
        }
        public void SetPauseBGM(bool isPause)
        {
            SetPause(m_CurBGMAudioSourceData.audioID, isPause);
        }

        public AudioSourceData GetAudioSourceData(int audioId)
        {
            if (m_DicAuidoSourceData.TryGetValue(audioId, out AudioSourceData data))
            {
                return data;
            }
            else
            {
                //Debugger.LogError("AudioSourceData not found: " + audioId, LogType.FrameNormal);
                return default;
            }
        }

        public AudioStateType GetAudioState(int audioId)
        {
            var audioSourceData = GetAudioSourceData(audioId);
            if (audioSourceData != null)
            {
                return audioSourceData.audioStateType;
            }
            return AudioStateType.NotPlay;
        }

        public void StopAudio(int audioId)
        {
            var audioSourceData = GetAudioSourceData(audioId);
            if (audioSourceData != null)
            {
                audioSourceData.audioSource.Stop();

                switch (audioSourceData.audioSourceType)
                {
                    case AudioSourceType.SoundEffect:
                        ReturnToPool(audioSourceData.audioSource, audioSourceData.audioID);
                        break;
                    case AudioSourceType.BGM:
                        m_CurBGMAudioSourceData.audioSource.clip = null;
                        m_DicAuidoSourceData.Remove(audioSourceData.audioID);
                        break;
                }
            }
        }
        public void StopBGM()
        {
            StopAudio(m_CurBGMAudioSourceData.audioID);
        }

        public void StopAllSoundEffects()
        {
            for (int i = 0; i < m_DicAuidoSourceData.Count();)
            {
                AudioSourceData audioSourceData = m_DicAuidoSourceData.ElementAt(i).Value;
                if (audioSourceData.audioSourceType == AudioSourceType.SoundEffect)
                {
                    int audioId = m_DicAuidoSourceData.ElementAt(i).Key;
                    ReturnToPool(audioSourceData.audioSource, audioId);
                }
                else
                {
                    i++;
                }
            }
        }

        public void ClearCache()
        {
            _audioClipCache.Clear();
            Resources.UnloadUnusedAssets();
        }

        #endregion
        public void Shutdown()
        {

        }
    }
}
