using UnityEngine;

public class Health : MonoBehaviour, IHealth
{
    [field: SerializeField] public int CurrentHealth { get; private set; }
    [field: SerializeField] public int OriginalMaxHealth { get; private set; }
    [field: SerializeField] public bool IsAlive { get; private set; }

    void Awake()
    {
        if (OriginalMaxHealth == 0)
            OriginalMaxHealth = 100;
    }

    void Start()
    {
        if (OriginalMaxHealth == 0)
            OriginalMaxHealth = 100;

        CurrentHealth = OriginalMaxHealth;
    }

    public void ReceiveDamage(int damageAmount)
    {
        //
    }

    public void ReceiveHeal(int healAmount)
    {
        //
    }
}
