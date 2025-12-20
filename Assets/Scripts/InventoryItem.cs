using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class InventoryItem : ScriptableObject
{
    [Header("Основные настройки")]
    public string itemName = "New Item";
    public Sprite itemIcon;
    public bool consumable = true;
    public int itemPrice = 10;

    [Header("Эффект предмета")]
    public ItemEffect effect = ItemEffect.RestoreStamina;
    public float effectAmount = 25f;
    public float effectDuration = 0f;

    public enum ItemEffect
    {
        RestoreStamina,
        RestoreHealth,
        IncreaseStrength,
        SpeedBoost
    }

    public virtual void Use()
    {
        Debug.Log($"Используется предмет: {itemName}");
        
        switch (effect)
        {
            case ItemEffect.RestoreStamina:
                RestoreStamina(effectAmount);
                break;
            case ItemEffect.RestoreHealth:
                RestoreHealth(effectAmount);
                break;
            case ItemEffect.IncreaseStrength:
                IncreaseStrength(effectDuration);
                break;
            case ItemEffect.SpeedBoost:
                SpeedBoost(effectDuration);
                break;
        }
    }

    private void RestoreStamina(float amount)
    {
        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
        if (player != null)
        {
            player.RestoreStamina(amount);
            Debug.Log($"✅ Восстановлено {amount}% стамины");
        }
        else
        {
            Debug.LogWarning("PlayerMovement не найден");
        }
    }

    private void RestoreHealth(float amount)
    {
        Debug.Log($"Восстановлено {amount}% здоровья");
    }

    private void IncreaseStrength(float duration)
    {
        Debug.Log($"Сила увеличена на {duration} секунд");
    }

    private void SpeedBoost(float duration)
    {
        Debug.Log($"Скорость увеличена на {duration} секунд");
    }
}