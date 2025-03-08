using System;
using System.Collections;
using Blessing.Core.ObjectPooling;
using Unity.VisualScripting;
using UnityEngine;

namespace Blessing.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioEmitter : PooledObject
    {
        public AudioSource AudioSource;
        private Coroutine playingCoroutine;

        void Awake()
        {
            AudioSource = GetComponent<AudioSource>();
        }

        public void Play()
        {
            if (playingCoroutine != null)
            {
                StopCoroutine(playingCoroutine);
            }

            AudioSource.Play();
            playingCoroutine = StartCoroutine(WaitForSoundToEnd());
        }

        public void Stop()
        {
            if (playingCoroutine != null)
            {
                StopCoroutine(playingCoroutine);
                playingCoroutine = null;
            }

            AudioSource.Stop();
            Pool.Release(this);
        }

        IEnumerator WaitForSoundToEnd()
        {
            yield return new WaitWhile(() => AudioSource.isPlaying);

            Pool.Release(this);
        }

        public override void GetFromPool()
        {
            gameObject.SetActive(true);
            AudioManager.Singleton.ActiveAudioEmitters.Add(this);
        }
        public override void ReleaseToPool()
        {
            gameObject.SetActive(false);
            AudioManager.Singleton.ActiveAudioEmitters.Remove(this);

            if (AudioSource.clip != null && AudioManager.Singleton.AudioClipsCountDic.TryGetValue(AudioSource.clip, out int count))
            {
                count -= 1;
                AudioManager.Singleton.AudioClipsCountDic[AudioSource.clip] = count < 0 ? 0 : count;
            }
        }
        public override void DestroyPooledObject()
        {
            Destroy(gameObject);
        }
    }
}
