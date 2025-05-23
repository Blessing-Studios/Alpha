using Blessing.Audio;
using Blessing.Core.ObjectPooling;
using UnityEngine;
using UnityEngine.VFX;

namespace Blessing.VFX
{
    public class PooledEffect : PooledObject
    {
        [SerializeField] private ParticleSystem particleEffect;
        [SerializeField] private VisualEffect visualEffect;
        [SerializeField] private AudioClip[] audioClips;
        public float Timer;
        private float lifeTime;

        void Awake()
        {
            float vEDuration = visualEffect != null ? visualEffect.GetFloat("LifeTime") : 0;
            float pEDuration = particleEffect != null ? particleEffect.main.duration : 0;

            lifeTime = vEDuration > pEDuration ? vEDuration : pEDuration;
        }
        public PooledEffect Initialized(Vector3 position)
        {
            transform.position = position;
            gameObject.SetActive(true);

            if (audioClips.Length > 0)
                    AudioManager.Singleton.PlaySoundFx(audioClips, transform);      

            return this;
        }

        void Update()
        {
            if (Timer >= lifeTime)
            {
                Release();
            }

            Timer += Time.deltaTime;
        }

        public override void GetFromPool()
        {
            
        }
        public override void ReleaseToPool()
        {
            gameObject.SetActive(false);
            Timer = 0.0f;
        }

        public override void DestroyPooledObject()
        {
            Destroy(gameObject);
        }
    }
}
