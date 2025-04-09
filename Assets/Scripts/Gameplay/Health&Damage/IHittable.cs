using UnityEngine;

namespace Blessing.HealthAndDamage
{
    public interface IHittable
    {
        public bool HasAuthority { get; }
        public void GotHit(IHitter hitter);
        public void GetOwnership();
    }
}

