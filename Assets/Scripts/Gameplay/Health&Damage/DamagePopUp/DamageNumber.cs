using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

namespace Blessing.Gameplay.HealthAndDamage
{
    public class DamageNumber : MonoBehaviour
    {
        public IObjectPool<DamageNumber> Pool;

        [SerializeField] private TextMeshPro textMesh;
        [SerializeField] private Color textColor;
        
        [SerializeField] private float disappearTimer;
        [SerializeField] private float fadeOutSpeed;
        [SerializeField] private float moveUpSpeed ;

        public DamageNumber Initialize(Vector3 position, int damage, float disappearTimer, float fadeOutSpeed, float moveUpSpeed)
        {
            Debug.Log(gameObject.name + ": DamageNumber Initialize");
            transform.position = position;
            textMesh.SetText(damage.ToString());
            textMesh.color = textColor;
            this.disappearTimer = disappearTimer;
            this.fadeOutSpeed = fadeOutSpeed;
            this.moveUpSpeed = moveUpSpeed;
            
            transform.LookAt(GameManager.Singleton.MainCamera.transform);
            transform.RotateAround(transform.position, transform.up, 180f);

            return this;
        }

        void Update()
        {
            transform.position += new Vector3(0f, moveUpSpeed * Time.deltaTime, 0f);

            disappearTimer -= Time.deltaTime;

            if (disappearTimer < 0f)
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
    }
}

