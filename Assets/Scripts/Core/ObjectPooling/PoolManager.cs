using System.Collections.Generic;
using Blessing.Core.ObjectPooling;
using UnityEngine;

namespace Blessing.Core.ObjectPooling
{
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Singleton { get; private set; }
        [SerializeField] private GenericObjectPooler poolerPrefab;
        public List<GenericObjectPooler> Poolers = new();
        public Dictionary<string, GenericObjectPooler> PoolerDic = new();
        protected virtual void Awake()
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
        public PooledObject Get(PooledObject pooledObject)
        {
            if (PoolerDic.ContainsKey(pooledObject.name))
            {
                return PoolerDic[pooledObject.name].Pool.Get();
            }

            GenericObjectPooler pooler = Instantiate(poolerPrefab, this.transform);

            pooler.Initialize(pooledObject);

            PoolerDic[pooledObject.name] = pooler;
            Poolers.Add(pooler);

            return pooler.Pool.Get();
        }

        public T Get<T>(PooledObject pooledObject) where T : PooledObject
        {
            if (PoolerDic.ContainsKey(pooledObject.name))
            {
                return PoolerDic[pooledObject.name].Pool.Get() as T;
            }

            GenericObjectPooler pooler = Instantiate(poolerPrefab, this.transform);

            pooler.Initialize(pooledObject);

            PoolerDic[pooledObject.name] = pooler;
            Poolers.Add(pooler);

            return pooler.Pool.Get() as T;
        }

        public void Release(PooledObject pooledObject)
        {
            // TODO: Tratar erro caso não encontre o Pooler no Dic
            PoolerDic[pooledObject.name].Pool.Release(pooledObject);
        }

        public void ClearAllPools()
        {
            // It will only clean the pools, not destroy the poolers
            foreach(GenericObjectPooler pooler in Poolers)
            {
                pooler.Pool.Clear();
            }
        }
    }
}
