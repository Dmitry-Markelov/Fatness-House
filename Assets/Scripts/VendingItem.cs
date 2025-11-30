using UnityEngine;

[CreateAssetMenu(fileName = "VendingItem", menuName = "Vending Machine/Item")]
public class VendingItem : ScriptableObject
{
    public string itemName;
    public int price;
    public Sprite itemIcon;
    public bool consumable;
    public GameObject itemPrefab;
}