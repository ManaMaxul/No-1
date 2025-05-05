using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HealthManager : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider armorBar;
    [SerializeField] private GameObject shieldObject;

    [Header("Configuración")]
    [SerializeField] private float maxArmor = 100f;
    [SerializeField] private float armorRegenRate = 5f;
    [SerializeField] private float armorRegenDelay = 3f;

    private Entity player;
    private float currentArmor;
    private float lastDamageTime;
    private bool isShieldActive;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Entity>();
        if (player != null)
        {
            currentArmor = maxArmor;
            UpdateUI();
        }
        else
        {
            Debug.LogError("No se encontró un jugador con componente Entity.");
        }

        if (shieldObject != null)
        {
            shieldObject.SetActive(false);
        }
    }

    void Update()
    {
        if (player == null) return;

        // Regeneración de armadura
        if (Time.time > lastDamageTime + armorRegenDelay && currentArmor < maxArmor)
        {
            currentArmor = Mathf.Min(maxArmor, currentArmor + armorRegenRate * Time.deltaTime);
            UpdateUI();
        }

        // Control del escudo
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleShield();
        }
    }

    public void TakeDamage(float damage)
    {
        if (player == null) return;

        lastDamageTime = Time.time;
        float remainingDamage = damage;

        // Primero absorbe el daño la armadura
        if (currentArmor > 0)
        {
            float armorDamage = Mathf.Min(currentArmor, damage);
            currentArmor -= armorDamage;
            remainingDamage -= armorDamage;
        }

        // El daño restante va a la vida
        if (remainingDamage > 0)
        {
            player.TakeDamage(remainingDamage);
        }

        UpdateUI();

        if (player.CurrentHealth <= 0)
        {
            Debug.Log("Jugador ha muerto, reiniciando nivel...");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void UpdateUI()
    {
        if (healthBar != null)
        {
            healthBar.value = player.CurrentHealth / player.MaxHealth;
        }
        if (armorBar != null)
        {
            armorBar.value = currentArmor / maxArmor;
        }
    }

    private void ToggleShield()
    {
        if (shieldObject != null)
        {
            isShieldActive = !isShieldActive;
            shieldObject.SetActive(isShieldActive);
        }
    }

    public bool IsShieldActive()
    {
        return isShieldActive;
    }
}