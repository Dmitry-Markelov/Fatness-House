using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    [Header("Inventory Settings")]
    public int inventorySlots = 4;
    
    [Header("Visual Settings")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;
    public float bottomOffset = 50f;
    
    [Header("Slot References")]
    public GameObject[] slotObjects;
    
    [Header("Key Bindings")]
    public KeyCode[] slotKeys = {
        KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4
    };
    
    private Image[] slotBackgrounds;
    private Image[] slotIcons;
    private TextMeshProUGUI[] slotNumbers;
    private int selectedSlot = -1;
    private List<InventoryItem> items = new List<InventoryItem>();
    
    void Start()
    {
        InitializeSlots();
        PositionInventoryAtBottom();
        SelectSlot(0);
    }
    
    void InitializeSlots()
    {
        slotBackgrounds = new Image[inventorySlots];
        slotIcons = new Image[inventorySlots];
        slotNumbers = new TextMeshProUGUI[inventorySlots];
        
        for (int i = 0; i < inventorySlots; i++)
        {
            if (slotObjects[i] != null)
            {
                slotBackgrounds[i] = slotObjects[i].GetComponent<Image>();
                
                Transform slotBackground = slotObjects[i].transform.Find("SlotBackground");
                if (slotBackground != null)
                {
                    Transform itemTransform = slotBackground.Find("Item");
                    if (itemTransform != null)
                    {
                        slotIcons[i] = itemTransform.GetComponent<Image>();
                    }
                }
                
                Transform textTransform = slotObjects[i].transform.Find("Text (TMP)");
                if (textTransform != null)
                {
                    slotNumbers[i] = textTransform.GetComponent<TextMeshProUGUI>();
                    slotNumbers[i].text = (i + 1).ToString();
                }
                
                ClearSlot(i);
            }
        }
    }
    
    void PositionInventoryAtBottom()
    {
        RectTransform panelRect = GetComponent<RectTransform>();
        if (panelRect != null)
        {
            panelRect.anchorMin = new Vector2(0.5f, 0f);
            panelRect.anchorMax = new Vector2(0.5f, 0f);
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.anchoredPosition = new Vector2(0f, bottomOffset);
        }
    }
    
    void Update()
    {
        HandleSlotSelection();
        if (Input.GetMouseButtonDown(1))
        {
            UseSelectedItem();
        }
    }
    
    void HandleSlotSelection()
    {
        for (int i = 0; i < slotKeys.Length && i < inventorySlots; i++)
        {
            if (Input.GetKeyDown(slotKeys[i]))
            {
                SelectSlot(i);
                break;
            }
        }
        
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            if (scroll > 0)
            {
                SelectSlot((selectedSlot - 1 + inventorySlots) % inventorySlots);
            }
            else
            {
                SelectSlot((selectedSlot + 1) % inventorySlots);
            }
        }
    }
    
    public void SelectSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventorySlots) return;
        
        if (selectedSlot >= 0 && selectedSlot < inventorySlots && slotBackgrounds[selectedSlot] != null)
        {
            slotBackgrounds[selectedSlot].color = normalColor;
        }
        
        selectedSlot = slotIndex;
        if (slotBackgrounds[selectedSlot] != null)
        {
            slotBackgrounds[selectedSlot].color = selectedColor;
        }
        
        Debug.Log($"Selected slot: {selectedSlot + 1}");
    }
    
    void UseSelectedItem()
    {
        if (selectedSlot >= 0 && selectedSlot < items.Count && items[selectedSlot] != null)
        {
            InventoryItem item = items[selectedSlot];
            Debug.Log($"Using item: {item.itemName} from slot {selectedSlot + 1}");
            
            item.Use();
            
            if (item.consumable)
            {
                RemoveItem(selectedSlot);
            }
        }
        else
        {
            Debug.Log("No item in selected slot");
        }
    }
    
    public void AddItem(InventoryItem newItem)
    {
        for (int i = 0; i < inventorySlots; i++)
        {
            if (items.Count <= i || items[i] == null)
            {
                AddItemToSlot(newItem, i);
                Debug.Log($"Added {newItem.itemName} to slot {i + 1}");
                return;
            }
        }
        Debug.Log("Inventory is full!");
    }
    
    public void AddItemToSlot(InventoryItem newItem, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventorySlots) return;
        
        while (items.Count <= slotIndex)
        {
            items.Add(null);
        }
        
        items[slotIndex] = newItem;
        if (slotIcons[slotIndex] != null)
        {
            slotIcons[slotIndex].sprite = newItem.itemIcon;
            slotIcons[slotIndex].color = Color.white;
        }
    }
    
    public void RemoveItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= items.Count) return;
        
        items[slotIndex] = null;
        ClearSlot(slotIndex);
        Debug.Log($"Item removed from slot {slotIndex + 1}");
    }
    
    void ClearSlot(int slotIndex)
    {
        if (slotIcons[slotIndex] != null)
        {
            slotIcons[slotIndex].sprite = null;
            slotIcons[slotIndex].color = new Color(1, 1, 1, 0);
        }
    }
    
    public InventoryItem GetSelectedItem()
    {
        if (selectedSlot >= 0 && selectedSlot < items.Count)
        {
            return items[selectedSlot];
        }
        return null;
    }
}