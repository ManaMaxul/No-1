using UnityEngine;

//TPFinal - [Your Name]
public class HealthManager : MonoBehaviour
{
    [SerializeField] private Entity entity;

    void Start()
    {
        entity.OnHealthChanged += UpdateHealthUI;
    }

    void OnDestroy()
    {
        entity.OnHealthChanged -= UpdateHealthUI;
    }

    private void UpdateHealthUI(float currentHealth, float maxHealth)
    {
        Debug.Log($"Health: {currentHealth}/{maxHealth}");
        // Add UI update logic here
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Projectile"))
        {
            entity.TakeDamage(10f);
        }
    }
}