
using Blessing.Gameplay.Characters;
using Blessing.Gameplay.Characters.Traits;

public enum HitType { Slash, Piercing, Impact, Explosion, Spell, Healing, Nothing }
public struct HitInfo
{   
    public int DamageClass;
    public int Damage;
    public HitType HitType;
    public Buff[] Buffs;

    public HitInfo(int damage, int damageClass, Buff[] buffs = null, HitType hitType = HitType.Nothing)
    {
        Damage = damage;
        DamageClass = damageClass;
        Buffs = buffs;
        HitType = hitType;
    }
}
