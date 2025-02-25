using System;
using System.Collections.Generic;
using Blessing.Core.ObjectPooling;
using UnityEngine;

namespace Blessing.Gameplay.HealthAndDamage
{
    public class DamageNumberPooler : AbstractObjectPooler<DamageNumber>
    {
        public GameObject DamageNumberPrefab;

        protected override DamageNumber CreateObject()
        {
            DamageNumber damageNumber = Instantiate(DamageNumberPrefab).GetComponent<DamageNumber>();
            damageNumber.Pool = Pool;

            return damageNumber;
        }
        protected override void OnGetFromPool(DamageNumber pooledObject)
        {
            pooledObject.gameObject.SetActive(true);
        }

        protected override void OnReleaseToPool(DamageNumber pooledObject)
        {
            pooledObject.gameObject.SetActive(false);

            pooledObject.transform.SetParent(transform, false);
        }
        protected override void OnDestroyPooledObject(DamageNumber pooledObject)
        {
            Destroy(pooledObject.gameObject);
        }
    }
}

