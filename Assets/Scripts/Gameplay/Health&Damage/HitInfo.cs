
using Blessing.Gameplay.Characters;
using Blessing.Gameplay.Characters.Traits;

public enum HitType { Slash, Piercing, Impact, Explosion, Spell, Healing, Nothing }
public struct HitInfo
{   
    public int Damage;
    public int DamageClass;
    public float Impact;
    public HitType HitType;
    public Buff[] Buffs;

    public HitInfo(int damage, int damageClass, float impact, Buff[] buffs = null, HitType hitType = HitType.Nothing)
    {
        Damage = damage;
        DamageClass = damageClass;
        Impact = impact;
        Buffs = buffs;
        HitType = hitType;
    }
}
