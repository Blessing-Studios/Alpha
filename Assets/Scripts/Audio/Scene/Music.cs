using UnityEngine;

namespace Blessing.Audio.Scene
{
    [RequireComponent(typeof(AudioSource))]
    public class Music : MonoBehaviour
    {
        private AudioSource audioSource;
        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource.clip == null)
            {
                Debug.LogError(gameObject.name + ": audioSource.clip is missing");
            }
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            AudioManager.Singleton.PlayMusic(audioSource);
        }
    }
}

