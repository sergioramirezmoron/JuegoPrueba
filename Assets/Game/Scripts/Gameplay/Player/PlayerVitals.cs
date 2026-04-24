using UnityEngine;

public class PlayerVitals : MonoBehaviour
{
    public float maxHealth = 100f;
    public float maxHunger = 100f;
    public float hungerDrainPerSecond = 0.35f;
    public float starvationDamagePerSecond = 4f;
    public float foodRestoreAmount = 25f;
    public float lowHungerThreshold = 25f;
    public float starvingMovementMultiplier = 0.65f;

    private PlayerInventory inventory;
    private float currentHealth;
    private float currentHunger;

    public float CurrentHealth => currentHealth;
    public float CurrentHunger => currentHunger;

    void Awake()
    {
        currentHealth = maxHealth;
        currentHunger = maxHunger;
    }

    void Update()
    {
        DrainHunger(hungerDrainPerSecond * Time.deltaTime);

        if (currentHunger <= 0f)
        {
            ApplyDamage(starvationDamagePerSecond * Time.deltaTime);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            TryEatFood();
        }
    }

    public void BindInventory(PlayerInventory playerInventory)
    {
        inventory = playerInventory;
    }

    public bool TryEatFood()
    {
        if (inventory == null || !inventory.TryConsumeResource(ResourceType.Food, 1))
        {
            return false;
        }

        RestoreHunger(foodRestoreAmount);
        return true;
    }

    public void DrainHunger(float amount)
    {
        currentHunger = Mathf.Max(0f, currentHunger - Mathf.Max(0f, amount));
    }

    public void RestoreHunger(float amount)
    {
        currentHunger = Mathf.Clamp(currentHunger + Mathf.Max(0f, amount), 0f, maxHunger);
    }

    public void ApplyDamage(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - Mathf.Max(0f, amount), 0f, maxHealth);
    }

    public float GetMovementMultiplier()
    {
        if (currentHunger >= lowHungerThreshold)
        {
            return 1f;
        }

        float normalizedHunger = lowHungerThreshold > 0f ? currentHunger / lowHungerThreshold : 0f;
        return Mathf.Lerp(starvingMovementMultiplier, 1f, normalizedHunger);
    }

    public string GetStatusDisplay()
    {
        return $"Vida: {Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}\n" +
               $"Hambre: {Mathf.CeilToInt(currentHunger)}/{Mathf.CeilToInt(maxHunger)}";
    }
}
