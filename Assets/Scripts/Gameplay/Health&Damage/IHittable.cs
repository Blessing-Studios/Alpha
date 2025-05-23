using UnityEngine;

namespace Blessing.HealthAndDamage
{
    public interface IHittable
    {
        public Transform transform { get; }
        public bool HasAuthority { get; }
        public void GotHit(IHitter hitter, HurtBox hurtBox);
        public void GetOwnership();
    }
}