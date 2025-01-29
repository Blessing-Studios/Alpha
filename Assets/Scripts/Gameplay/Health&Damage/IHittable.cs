namespace Blessing.Gameplay.HealthAndDamage
{
    public interface IHittable
    {
        public void GotHit(IHitter hitter);
        public void GetOwnership();
    }
}

