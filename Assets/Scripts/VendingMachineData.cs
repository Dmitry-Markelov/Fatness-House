using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VendingMachineData", menuName = "Vending Machine/Data")]
public class VendingMachineData : ScriptableObject
{
    [System.Serializable]
    public class ItemData
    {
        public string itemName;
        public int price;
        public Sprite itemIcon;
        public bool consumable = true;
    }
    
    public List<ItemData> itemsForSale = new List<ItemData>();
}