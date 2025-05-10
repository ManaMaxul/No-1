using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class Entity : MonoBehaviour
{
    public enum EntityType { Player, Enemy, Boss }
    public enum EntityState { Idle, Moving, Attacking, Dead, Dashing }

    [Header("Estadísticas Base")]
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float baseAttack = 10f;
    [SerializeField] protected float baseDefense = 5f;
    [SerializeField] protected EntityType type;

    protected float currentHealth;
    protected EntityState currentState;
    protected Inventory inventory;

    public Inventory.TypeRarity typeRarity;
    public List<Inventory.DamageType> resistances = new List<Inventory.DamageType>();
    public List<Inventory.DamageType> weaknesses = new List<Inventory.DamageType>();

    // Eventos
    public event Action<float, float> OnHealthChanged; // currentHealth, maxHealth
    public event Action<EntityState> OnStateChanged;
    public event Action OnDeath;

    public EntityType Type => type;
    public EntityState CurrentState
    {
        get => currentState;
        set
        {
            if (currentState != value)
            {
                currentState = value;
                OnStateChanged?.Invoke(value);
            }
        }
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

        float totalDefense = CalculateTotalDefense();
        float reducedDamage = Mathf.Max(0, damage - totalDefense);
        currentHealth -= reducedDamage;
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"{gameObject.name} recibió {reducedDamage} de daño (después de defensa: {totalDefense}). Vida restante: {currentHealth}");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            CurrentState = EntityState.Dead;
            OnDeath?.Invoke();
            Die();
        }
    }

    protected virtual float CalculateTotalDefense()
    {
        float totalDefense = baseDefense;
        if (inventory != null)
        {
            Inventory.Item equippedShield = inventory.GetEquippedItem(Inventory.ItemCategory.Escudo);
            if (equippedShield != null) totalDefense += equippedShield.defense;

            Inventory.Item equippedArmor = inventory.GetEquippedItem(Inventory.ItemCategory.Armadura);
            if (equippedArmor != null) totalDefense += equippedArmor.defense;

            Inventory.Item equippedGloves = inventory.GetEquippedItem(Inventory.ItemCategory.Guantes);
            if (equippedGloves != null) totalDefense += equippedGloves.defense;

            Inventory.Item equippedChip = inventory.GetEquippedItem(Inventory.ItemCategory.Chip);
            if (equippedChip != null) totalDefense += equippedChip.defense;
        }
        return totalDefense;
    }

    protected abstract void Die();
}