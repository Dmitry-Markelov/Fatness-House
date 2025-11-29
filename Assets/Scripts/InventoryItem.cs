using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    public string itemName;
    public Sprite itemIcon;
    public bool consumable = true;
    public int itemValue = 1;
    
    public InventoryItem(string name, Sprite icon, bool isConsumable = true)
    {
        itemName = name;
        itemIcon = icon;
        consumable = isConsumable;
    }

    public virtual void Use()
    {
        // Debug.Log($"Using item: {itemName}");
        // Реализация использования в дочерних классах
    }
}