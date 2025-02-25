
using Blessing.Gameplay.Characters;
using Blessing.Gameplay.Characters.Traits;

public struct HitInfo
{   
    public int DamageClass;
    public int Damage;
    public Buff[] Buffs;

    public HitInfo(int damage, int damageClass, Buff[] buffs = null)
    {
        Damage = damage;
        DamageClass = damageClass;
        Buffs = buffs;
    }
}
