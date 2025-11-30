using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VendingItemUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private GameObject consumableBadge;
    
    public void Initialize(VendingItem item)
    {
        if (item == null) 
        {
            Debug.LogError("Item is null!");
            return;
        }
        
        iconImage.sprite = item.itemIcon;
        nameText.text = item.itemName;
        priceText.text = $"{item.price} руб.";
        consumableBadge.SetActive(item.consumable);
        
        iconImage.gameObject.SetActive(item.itemIcon != null);
    }
}