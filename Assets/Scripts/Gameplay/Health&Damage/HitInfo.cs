
using UnityEngine;
using Blessing.Gameplay.Characters.Traits;

public enum HitType { Slash, Piercing, Impact, Block, Explosion, Spell, Healing, Nothing }
public struct HitInfo
{
    public int Damage;
    public int DamageClass;
    public float Impact;
    public Vector3 HitPosition;
    public HitType HitType;
    public Buff[] Buffs;

    public HitInfo(int damage, int damageClass, float impact, Vector3 hitPosition, Buff[] buffs = null, HitType hitType = HitType.Nothing)
    {
        Damage = damage;
        DamageClass = damageClass;
        Impact = impact;
        HitPosition = hitPosition;
        Buffs = buffs;
        HitType = hitType;
    }

    public bool CanTriggerTakeHit()
    {
        return HitType switch
        {
            HitType.Healing => false,
            _ => true
        };
    }
}
