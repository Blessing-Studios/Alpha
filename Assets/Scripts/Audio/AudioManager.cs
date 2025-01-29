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
        [SerializeField] private AudioSource audioSourceFXPrefab;
        [SerializeField] private AudioSource currentMusicSource;

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

            if ( audioSourceFXPrefab == null )
            {
                Debug.LogError(gameObject.name + ": audioSourceFXPrefab is missing");
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
        }

        public GameObject PlayMusic(AudioSource musicSource)
        {
            if (currentMusicSource != null)
            {
                currentMusicSource.Stop();
            }

            currentMusicSource = musicSource;

            currentMusicSource.outputAudioMixerGroup = audioMixerMusic;
            currentMusicSource.Play();

            return currentMusicSource.gameObject;
        }

        public void PlaySoundLoop(string path)
        {
            // Criar lógica
        }

        public GameObject PlaySoundFx(AudioClip audioClip, Transform sourceTransform, float spatialBlend = 1.0f, float volume = 1.0f, int priority = 128)
        {
            AudioSource audioSource = Instantiate(audioSourceFXPrefab, sourceTransform.position, Quaternion.identity);

            audioSource.outputAudioMixerGroup = audioMixerSoundFX;
            audioSource.clip = audioClip;
            audioSource.name = audioClip.name;
            audioSource.spatialBlend = spatialBlend;
            audioSource.volume = volume;
            audioSource.priority = priority;
            audioSource.Play();

            Destroy(audioSource.gameObject, audioSource.clip.length);

            return audioSource.gameObject;
        }

        public GameObject PlaySoundFx(AudioClip[] audioClip, Transform sourceTransform, float spatialBlend = 1.0f, float volume = 1.0f, int priority = 128)
        {
            int rand = Random.Range(0, audioClip.Length);

            return PlaySoundFx(audioClip[rand], sourceTransform, spatialBlend, volume, priority);
        }

        public GameObject PlayUiSound(AudioClip audioClip, float volume = 1.0f)
        {
            return PlaySoundFx(audioClip, gameObject.transform, 1.0f, volume, 128);
        }

        public GameObject PlayUiSound(AudioClip[] audioClip , float volume = 1.0f)
        {
            int rand = Random.Range(0, audioClip.Length);
            return PlayUiSound(audioClip[rand], volume);
        }
    }
}