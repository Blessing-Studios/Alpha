using System;
using Blessing.Core.ObjectPooling;
using UnityEngine;
using UnityEngine.Animations;

namespace Blessing.Gameplay.SkillsAndMagic
{
    public class HitEffect : PooledObject
    {
        [SerializeField] protected float lifeTime = 100;
        public float Timer;
        public ParentConstraint ParentConstraint;
        void Update()
        {
            if (Timer >= lifeTime)
            {
                Pool.Release(this);
            }

            Timer += Time.deltaTime;
        }

        public void SetParentConstrain(Transform parentTransform)
        {
            ConstraintSource source = new ConstraintSource(){sourceTransform = parentTransform, weight = 1.0f};
    
            ParentConstraint.AddSource(source);

            Matrix4x4 inverse = Matrix4x4.TRS(parentTransform.position, parentTransform.rotation, new Vector3(1,1,1)).inverse;
            ParentConstraint.SetTranslationOffset(0, inverse.MultiplyPoint3x4(transform.position));
            ParentConstraint.SetRotationOffset(0, (Quaternion.Inverse(parentTransform.rotation) * transform.rotation).eulerAngles);
                        
            ParentConstraint.constraintActive = true;
        }

        public override void GetFromPool()
        {
            gameObject.SetActive(true);
        }

        public override void ReleaseToPool()
        {
            gameObject.SetActive(false);
            Timer = 0.0f;

            ParentConstraint.constraintActive = false;
            int sourceCount = ParentConstraint.sourceCount;
            for(int i = sourceCount - 1; i >= 0; i--)
            {
                ParentConstraint.RemoveSource(i);
            }
        }
        public override void DestroyPooledObject()
        {
            Destroy(gameObject);
        }
    }
}
