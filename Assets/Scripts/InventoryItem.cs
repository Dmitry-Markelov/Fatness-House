using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class InventoryItem : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    public bool consumable = true;
    public int itemPrice;


    public virtual void Use()
    {
        Debug.Log($"Используется предмет: {itemName}");
    }

    private void RestoreStamina(float amount)
    {
        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null)
        {
            player.RestoreStamina(amount);
            Debug.Log($"Восстановлено {amount}% стамины");
        }
    }

    private void IncreaseStrength(float duration)
    {
        Debug.Log($"Сила увеличена на {duration} секунд");
    }

    private void IncreaseStaminaRegen(float duration)
    {
        Debug.Log($"Восстановление стамины ускорено на {duration} секунд");
    }

    private void HealPlayer(float amount)
    {
        Debug.Log($"Восстановлено {amount}% здоровья");
    }
}