namespace Blessing.Gameplay.HealthAndDamage
{
    public interface IHitter
    {
        public bool HasAuthority { get; }
        public HitInfo HitInfo { get; }

        /// <summary>
        /// Trigger Hit logic
        /// </summary>
        /// <param name="target"></param>
        /// <returns>false if already hit, true if it is a new target</returns>
        public bool Hit(IHittable target);
    }
}
