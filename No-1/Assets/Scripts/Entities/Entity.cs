using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    public enum EntityType { Player, Enemy, Boss }
    public enum EntityState { Idle, Moving, Attacking, Dead }

    [Header("Estadísticas Base")]
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float baseAttack = 10f;
    [SerializeField] protected float baseDefense = 5f;
    [SerializeField] protected EntityType type;
    
    protected float currentHealth;
    protected EntityState currentState;
    protected Inventory inventory;

    public EntityType Type => type;
    public EntityState CurrentState
    {
        get => currentState;
        set => currentState = value;
    }
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float BaseAttack => baseAttack;
    public float BaseDefense => baseDefense;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        currentState = EntityState.Idle;
        inventory = GetComponent<Inventory>();
    }

    public virtual void TakeDamage(float damage)
    {
        if (currentState == EntityState.Dead) return;

        float totalDefense = baseDefense;
        if (inventory != null)
        {
            if (inventory.leftHandItem != null) totalDefense += inventory.leftHandItem.defense;
            if (inventory.rightHandItem != null) totalDefense += inventory.rightHandItem.defense;
        }
        float reducedDamage = Mathf.Max(0, damage - totalDefense);

        currentHealth -= reducedDamage;
        Debug.Log($"{gameObject.name} recibió {reducedDamage} de daño (después de defensa: {totalDefense}). Vida restante: {currentHealth}");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            CurrentState = EntityState.Dead;
            Die();
        }
    }

    protected abstract void Die();
}