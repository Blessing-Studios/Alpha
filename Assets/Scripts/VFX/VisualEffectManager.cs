using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.VFX;

namespace Blessing.VFX
{
    public class VisualEffectManager : MonoBehaviour
    {
        public static VisualEffectManager Singleton { get; private set; }

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
        }

        public void PlayVFX(GameObject visualEffectPrefab, Vector3 position)
        {
            GameObject newVisualEffect = Instantiate(visualEffectPrefab, position, Quaternion.identity);
            
            VisualEffect visualEffect = newVisualEffect.GetComponent<VisualEffect>();
            visualEffect.Play();

            Destroy(newVisualEffect, visualEffect.GetFloat("LifeTime"));
        }
    }
}