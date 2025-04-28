using UnityEngine;

//TPFinal - Christian E. Casalnovo
public abstract class Entity : MonoBehaviour
{
    // Enum for entity types
    public enum EntityType
    {
        Player,
        Enemy,
        Boss
    }

    [SerializeField] protected EntityType entityType;

    // Stats based on EntityType
    private float maxHealth => GetStat(StatType.MaxHealth);
    public float CurrentHealth { get; protected set; }
    public float MoveSpeed => GetStat(StatType.MoveSpeed);

    // Enum for entity state
    public enum EntityState { Idle, Moving, Attacking, Dead, Damaged } // Added Damaged state
    public EntityState CurrentState { get; protected set; } = EntityState.Idle;

    // Delegate and Event for health changes
    public delegate void HealthChangedHandler(float newHealth, float maxHealth);
    public event HealthChangedHandler OnHealthChanged;

    // Stat types
    private enum StatType { MaxHealth, MoveSpeed }

    protected virtual void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public virtual void TakeDamage(float damage)
    {
        CurrentHealth -= damage;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        if (CurrentHealth <= 0)
        {
            CurrentState = EntityState.Dead;
            Die();
        }
        else
        {
            CurrentState = EntityState.Damaged; // Set to Damaged when taking damage
        }
    }

    protected abstract void Die();

    // Method to get stats based on EntityType
    private float GetStat(StatType stat)
    {
        switch (entityType)
        {
            case EntityType.Player:
                return stat == StatType.MaxHealth ? 50f : 5f; // Player: 50 HP, 5 speed
            case EntityType.Enemy:
                return stat == StatType.MaxHealth ? 15f : 3f; // Enemy: 15 HP, 3 speed
            case EntityType.Boss:
                return stat == StatType.MaxHealth ? 100f : 2f; // Boss: 100 HP, 2 speed
            default:
                return 0f;
        }
    }
}