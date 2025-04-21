using System.Collections.Generic;
using Blessing.HealthAndDamage;
using UnityEngine;

public interface IHealth
{
    public int CurrentHealth { get; }
    public int OriginalMaxHealth { get; }
    public bool IsAlive { get; }
    public void ReceiveDamage(int damageAmount);
    public void ReceiveHeal(int healAmount);
}
