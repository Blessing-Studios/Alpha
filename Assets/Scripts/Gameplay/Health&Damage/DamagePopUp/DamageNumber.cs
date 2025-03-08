using System.Threading;
using Blessing.Core.ObjectPooling;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;

namespace Blessing.Gameplay.HealthAndDamage
{
    public class DamageNumber : PooledObject
    {
        [SerializeField] private TextMeshPro textMesh;
        [SerializeField] private Color textColor;
        
        [SerializeField] private float disappearTime = 2f;
        [SerializeField] private float fadeOutSpeed = 1f;
        [SerializeField] private float moveUpSpeed = 1f;
        private float timer = 0f;
        public DamageNumber Initialize(Vector3 position, int damage)
        {
            Debug.Log(gameObject.name + ": DamageNumber Initialize");
            transform.position = position;
            textMesh.SetText(damage.ToString());
            textMesh.color = textColor;
            
            transform.LookAt(GameManager.Singleton.MainCamera.transform);
            transform.RotateAround(transform.position, transform.up, 180f);

            return this;
        }
        void Update()
        {
            transform.position += new Vector3(0f, moveUpSpeed * Time.deltaTime, 0f);

            timer += Time.deltaTime;

            if (timer >= disappearTime)
            {
                Color color = textMesh.color;
                color.a -= fadeOutSpeed * Time.deltaTime;
                textMesh.color = color;

                if (color.a <= 0f)
                {
                    Pool.Release(this);
                }
            }
        }

        public override void GetFromPool()
        {
            gameObject.SetActive(true);
        }
        public override void ReleaseToPool()
        {
            gameObject.SetActive(false);
            timer = 0.0f;
        }
        public override void DestroyPooledObject()
        {
            Destroy(gameObject);
        }
    }
}

