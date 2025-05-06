using UnityEngine;
using UnityEngine.Pool;

namespace Blessing.Core.ObjectPooling
{
    public abstract class  PooledObject : MonoBehaviour
    {
        public IObjectPool<PooledObject> Pool;
        public abstract void GetFromPool();
        public abstract void ReleaseToPool();
        public abstract void DestroyPooledObject();

        public virtual void Release()
        {
            Pool.Release(this);
        }
    }
}