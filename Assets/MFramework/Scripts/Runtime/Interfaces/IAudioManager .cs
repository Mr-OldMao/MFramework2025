using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFramework.Runtime
{
	public interface IAudioManager : IGameModule
	{
        void PlaySound(string soundName);
        void PlayMusic(string musicName);
        void SetVolume(float volume);
    }

}