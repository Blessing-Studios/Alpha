using System.Collections.Generic;
using Blessing.Core.ObjectPooling;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

namespace Blessing.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Singleton { get; private set; }
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private AudioMixerGroup audioMixerMusic;
        [SerializeField] private AudioMixerGroup audioMixerSoundFX;
        [SerializeField] private AudioEmitter audioEmitterPrefab;
        [SerializeField] private AudioSource currentMusicSource;
        public List<AudioEmitter> ActiveAudioEmitters = new();
        public Dictionary<AudioClip, int> AudioClipsCountDic = new();
        public int MaxAudioInstance = 30;

        void Awake()
        {
            if (Singleton != null && Singleton != this)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                Singleton = this;
            }
            if ( audioMixer == null )
            {
                Debug.LogError(gameObject.name + ": audioMixer is missing");
            }

            if ( audioMixerMusic == null )
            {
                Debug.LogError(gameObject.name + ": audioMixerMusic is missing");
            }

            if ( audioMixerSoundFX == null )
            {
                Debug.LogError(gameObject.name + ": audioMixerSoundFX is missing");
            }

            if ( audioEmitterPrefab == null )
            {
                Debug.LogError(gameObject.name + ": audioEmitterPrefab is missing");
            }
        }

        public GameObject PlayMusic(AudioSource musicSource)
        {
            if (currentMusicSource != null)
            {
                currentMusicSource.Stop();
            }

            currentMusicSource = musicSource;

            currentMusicSource.outputAudioMixerGroup = audioMixerMusic;
            currentMusicSource.loop = true;
            currentMusicSource.Play();

            return currentMusicSource.gameObject;
        }

        public void PlaySoundLoop(string path)
        {
            // Criar lógica
        }

        public GameObject PlayAudio(AudioClip audioClip, Transform sourceTransform, float spatialBlend = 1.0f, float volume = 1.0f, int priority = 128, AudioMixerGroup audioMixerGroup = null)
        {
            // AudioSource audioSource = Instantiate(audioSourceFXPrefab, sourceTransform.position, Quaternion.identity);

            // audioSource.outputAudioMixerGroup = audioMixerSoundFX;
            // audioSource.clip = audioClip;
            // audioSource.name = audioClip.name;
            // audioSource.spatialBlend = spatialBlend;
            // audioSource.volume = volume;
            // audioSource.priority = priority;
            // audioSource.Play();

            // Destroy(audioSource.gameObject, audioSource.clip.length);
            if (!CanPlayAudioClip(audioClip)) return null;

            AudioEmitter audioEmitter = PoolManager.Singleton.Get(audioEmitterPrefab) as AudioEmitter;

            audioEmitter.transform.position = sourceTransform.position;
            audioEmitter.AudioSource.clip = audioClip;
            audioEmitter.AudioSource.name = audioClip.name;
            audioEmitter.AudioSource.spatialBlend = spatialBlend;
            audioEmitter.AudioSource.volume = volume;
            audioEmitter.AudioSource.priority = priority;
            audioEmitter.AudioSource.outputAudioMixerGroup = audioMixerGroup != null ? audioMixerGroup : audioMixerSoundFX; 

            AudioClipsCountDic[audioClip] = AudioClipsCountDic.TryGetValue(audioClip, out int count) ? count + 1 : 1;

            audioEmitter.Play();

            return audioEmitter.gameObject;
        }

        private bool CanPlayAudioClip(AudioClip audioClip)
        {
            // Running AudioClips need to be less then MaxAudioInstance
            return !AudioClipsCountDic.TryGetValue(audioClip, out int count) || count < MaxAudioInstance;
        }

        public GameObject PlaySoundFx(AudioClip[] audioClip, Transform sourceTransform, float spatialBlend = 1.0f, float volume = 1.0f, int priority = 128)
        {
            int rand = Random.Range(0, audioClip.Length);
            return PlayAudio(audioClip[rand], sourceTransform, spatialBlend, volume, priority);
        }

        public GameObject PlayUiSound(AudioClip audioClip, float volume = 1.0f)
        {
            return PlayAudio(audioClip, gameObject.transform, 1.0f, volume, 128);
        }

        public GameObject PlayUiSound(AudioClip[] audioClip , float volume = 1.0f)
        {
            int rand = Random.Range(0, audioClip.Length);
            return PlayUiSound(audioClip[rand], volume);
        }
    }
}