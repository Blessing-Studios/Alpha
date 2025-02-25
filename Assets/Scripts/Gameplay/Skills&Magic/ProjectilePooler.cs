using System;
using System.Collections.Generic;
using Blessing.Core.ObjectPooling;
using UnityEngine;

namespace Blessing.Gameplay.SkillsAndMagic
{
    public class ProjectilePooler : AbstractObjectPooler<Projectile>
    {
        public GameObject ProjectilePrefab;

        protected override Projectile CreateObject()
        {
            Projectile projectile = Instantiate(ProjectilePrefab).GetComponent<Projectile>();
            // projectile.Pool = Pool;

            return projectile;
        }
        protected override void OnGetFromPool(Projectile pooledObject)
        {
            pooledObject.gameObject.SetActive(true);
        }

        protected override void OnReleaseToPool(Projectile pooledObject)
        {
            pooledObject.gameObject.SetActive(false);

            pooledObject.transform.SetParent(transform, true);

            pooledObject.TargetList.Clear();
            pooledObject.Timer = 0.0f;
        }
        protected override void OnDestroyPooledObject(Projectile pooledObject)
        {
            Destroy(pooledObject.gameObject);
        }
    }
}