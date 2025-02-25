using System;
using System.Collections.Generic;
using Blessing.Core.ObjectPooling;
using UnityEngine;

namespace Blessing.Core.ObjectPooling
{
    public class GenericObjectPooler : AbstractObjectPooler<PooledObject>
    {
        public PooledObject PooledObjectPrefab;
        public void Initialize(PooledObject prefab)
        {
            PooledObjectPrefab = prefab;
            this.name = prefab.name + "Pooler";
        }
        protected override PooledObject CreateObject()
        {
            PooledObject pooledObject = Instantiate(PooledObjectPrefab).GetComponent<PooledObject>();
            pooledObject.Pool = Pool;
            pooledObject.transform.SetParent(transform, true);

            return pooledObject;
        }
        protected override void OnGetFromPool(PooledObject pooledObject)
        {
            pooledObject.GetFromPool();
        }

        protected override void OnReleaseToPool(PooledObject pooledObject)
        {
            pooledObject.ReleaseToPool();
            pooledObject.transform.SetParent(transform, true);
        }
        protected override void OnDestroyPooledObject(PooledObject pooledObject)
        {
            pooledObject.DestroyPooledObject();
        }
    }
}