using UnityEngine;
using UnityEngine.Pool;

namespace Blessing.Core.ObjectPooling
{
    public abstract class AbstractObjectPooler<T> : MonoBehaviour where T : MonoBehaviour
    {
        public IObjectPool<T> Pool;
        [SerializeField] private bool collectionCheck = true;
        [SerializeField] private int defaultCapacity = 100;
        [SerializeField] private int maxSize = 1000;

        void Awake()
        {
            Pool = new ObjectPool<T>(CreateObject, OnGetFromPool, OnReleaseToPool, OnDestroyPooledObject, collectionCheck, defaultCapacity, maxSize);
        }

        protected abstract T CreateObject();
        protected abstract void OnGetFromPool(T pooledObject);
        protected abstract void OnReleaseToPool(T pooledObject);
        protected abstract void OnDestroyPooledObject(T pooledObject);
    }
}

