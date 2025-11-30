using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VendingMachineData", menuName = "Vending Machine/Data")]
public class VendingMachineData : ScriptableObject
{
    public List<VendingItem> itemsForSale = new List<VendingItem>();
}