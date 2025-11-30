using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class VendingMachine : MonoBehaviour, IInteractable
{
    [Header("Настройки")]
    [SerializeField] private string interactionText = "Press E to use Vending Machine";
    
    [Header("UI References")]
    [SerializeField] private GameObject vendingMachineUI;
    [SerializeField] private Button[] buyButtons;
    [SerializeField] private Image[] itemIcons;
    [SerializeField] private TextMeshProUGUI[] itemNameTexts;
    [SerializeField] private TextMeshProUGUI[] priceTexts;
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI balanceText;
    
    [Header("Player References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private RayCast rayCast;
    
    [Header("UI Elements to Hide")]
    [SerializeField] private GameObject staminaBarUI;
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private GameObject balanceUI;
    
    [Header("Товары через ScriptableObject")]
    public VendingItem[] itemsForSale = new VendingItem[6];

    private bool isUIOpen = false;

    [System.Serializable]
    public class VendingItem
    {
        public string itemName;
        public int price;
        public Sprite itemIcon;
        public bool consumable = true;
        public InventoryItem itemPrefab;
    }

    void Start()
    {
        if (vendingMachineUI != null)
            vendingMachineUI.SetActive(false);
        
        SetupVendingMachine();
    }

    void SetupVendingMachine()
    {
        for (int i = 0; i < itemsForSale.Length; i++)
        {
            itemsForSale[i] = new VendingItem();
        }

        InventoryItem[] allItems = Resources.LoadAll<InventoryItem>("ScriptableObject/Data/Items");
        
        SetupDefaultItems(allItems);
        InitializeUI();
    }

    void SetupDefaultItems(InventoryItem[] allItems)
    {
        Dictionary<string, InventoryItem> itemDict = new Dictionary<string, InventoryItem>();
        foreach (var item in allItems)
        {
            if (item != null)
            {
                itemDict[item.itemName.ToUpper()] = item;
            }
        }

        // itemsForSale[0] = CreateVendingItem("ENERGYDRINK", 15, GetItemFromDict(itemDict, "ENERGYDRINK", "CREATINE"));
        // itemsForSale[1] = CreateVendingItem("PROTEINE", 25, GetItemFromDict(itemDict, "PROTEINE", "BCAA"));
        // itemsForSale[2] = CreateVendingItem("SANDWICH", 10, GetItemFromDict(itemDict, "PROTEINEBAR", "ENERGYDRINK"));
        // itemsForSale[3] = CreateVendingItem("VITAMINS", 30, GetItemFromDict(itemDict, "PROTEINE", "TEST"));
        // itemsForSale[4] = CreateVendingItem("BANDAGE", 20, GetItemFromDict(itemDict, "PROTEINEBAR", "BCAA"));
        // itemsForSale[5] = CreateVendingItem("WATER", 5, GetItemFromDict(itemDict, "TEST", "ENERGYDRINK"));
    }

    VendingItem CreateVendingItem(string name, int price, InventoryItem itemPrefab)
    {
        return new VendingItem
        {
            itemName = name,
            price = price,
            itemPrefab = itemPrefab,
            itemIcon = itemPrefab != null ? itemPrefab.itemIcon : null,
            consumable = true
        };
    }

    InventoryItem GetItemFromDict(Dictionary<string, InventoryItem> dict, string primary, string fallback)
    {
        if (dict.ContainsKey(primary))
            return dict[primary];
        if (dict.ContainsKey(fallback))
            return dict[fallback];
        return null;
    }

    void InitializeUI()
    {
        Debug.Log("Initializing Vending Machine UI...");

        for (int i = 0; i < buyButtons.Length && i < itemsForSale.Length; i++)
        {
            int index = i;
            
            if (buyButtons[i] != null)
            {
                buyButtons[i].onClick.RemoveAllListeners();
                buyButtons[i].onClick.AddListener(() => BuyItem(index));
                Debug.Log($"Button {i} initialized");
            }
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseVendingMachine);
        }

        UpdateItemsDisplay();
    }

    void UpdateItemsDisplay()
    {
        Debug.Log("Updating items display...");

        for (int i = 0; i < itemsForSale.Length; i++)
        {
            if (itemsForSale[i] != null)
            {
                if (itemNameTexts != null && i < itemNameTexts.Length && itemNameTexts[i] != null)
                {
                    itemNameTexts[i].text = itemsForSale[i].itemName;
                    Debug.Log($"Set item name {i}: {itemsForSale[i].itemName}");
                }

                if (priceTexts != null && i < priceTexts.Length && priceTexts[i] != null)
                {
                    priceTexts[i].text = itemsForSale[i].price.ToString();
                    Debug.Log($"Set price {i}: {itemsForSale[i].price}");
                }

                if (itemIcons != null && i < itemIcons.Length && itemIcons[i] != null)
                {
                    if (itemsForSale[i].itemPrefab != null && itemsForSale[i].itemPrefab.itemIcon != null)
                    {
                        itemIcons[i].sprite = itemsForSale[i].itemPrefab.itemIcon;
                        itemIcons[i].gameObject.SetActive(true);
                        Debug.Log($"Set icon {i} from ScriptableObject");
                    }
                    else if (itemsForSale[i].itemIcon != null)
                    {
                        itemIcons[i].sprite = itemsForSale[i].itemIcon;
                        itemIcons[i].gameObject.SetActive(true);
                        Debug.Log($"Set icon {i} from direct reference");
                    }
                    else
                    {
                        itemIcons[i].gameObject.SetActive(false);
                        Debug.Log($"No icon for item {i}");
                    }
                }
            }
        }
    }

    public void Interact()
    {
        if (!isUIOpen)
        {
            OpenVendingMachine();
        }
    }

    public string GetInteractionText()
    {
        return interactionText;
    }

    void OpenVendingMachine()
    {
        if (rayCast != null) 
            rayCast.SetInteractionEnabled(false);

        isUIOpen = true;

        if (vendingMachineUI != null) 
            vendingMachineUI.SetActive(true);

        SetOtherUIVisibility(false);

        if (playerMovement != null) 
            playerMovement.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        UpdateUI();
    }

    void CloseVendingMachine()
    {
        isUIOpen = false;

        if (vendingMachineUI != null) 
            vendingMachineUI.SetActive(false);

        SetOtherUIVisibility(true);

        if (playerMovement != null) 
            playerMovement.enabled = true;

        StartCoroutine(EnableRayCastNextFrame());

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    IEnumerator EnableRayCastNextFrame()
    {
        yield return new WaitForEndOfFrame();
        if (rayCast != null)
            rayCast.SetInteractionEnabled(true);
    }

    void SetOtherUIVisibility(bool visible)
    {
        if (staminaBarUI != null)
            staminaBarUI.SetActive(visible);

        if (inventoryUI != null)
            inventoryUI.SetActive(visible);

        if (balanceUI != null)
            balanceUI.SetActive(visible);
    }

    void UpdateUI()
    {
        UpdateBalanceDisplay();
        UpdateButtonsState();
        UpdateItemsDisplay();
    }

    void UpdateBalanceDisplay()
    {
        if (balanceText != null && CurrencyManager.Instance != null)
        {
            balanceText.text = $"ГАНТЕЛЕЙ: {CurrencyManager.Instance.currentCurrency}";
        }
    }

    void UpdateButtonsState()
    {
        for (int i = 0; i < buyButtons.Length && i < itemsForSale.Length; i++)
        {
            if (buyButtons[i] != null && itemsForSale[i] != null && CurrencyManager.Instance != null)
            {
                bool canAfford = CurrencyManager.Instance.HasEnoughCurrency(itemsForSale[i].price);
                buyButtons[i].interactable = canAfford;
            }
        }
    }

    public void BuyItem(int itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= itemsForSale.Length || itemsForSale[itemIndex] == null)
        {
            ShowFeedback("Товар недоступен", Color.red);
            return;
        }

        VendingItem item = itemsForSale[itemIndex];

        if (CurrencyManager.Instance == null)
        {
            ShowFeedback("Ошибка системы валюты", Color.red);
            return;
        }

        if (CurrencyManager.Instance.SpendCurrency(item.price))
        {
            AddItemToInventory(item);
            ShowFeedback($"Куплено: {item.itemName}!", Color.green);
            UpdateUI();
        }
        else
        {
            ShowFeedback($"Недостаточно гантелей! Нужно: {item.price}", Color.red);
        }
    }

    void AddItemToInventory(VendingItem item)
    {
        if (item.itemPrefab == null)
        {
            ShowFeedback("Ошибка: предмет не настроен", Color.red);
            return;
        }

        InventoryManager inventory = FindAnyObjectByType<InventoryManager>();
        if (inventory != null)
        {
            inventory.AddItem(item.itemPrefab);
            Debug.Log($"Добавлен предмет в инвентарь: {item.itemName}");
        }
        else
        {
            Debug.LogWarning("InventoryManager не найден");
            ShowFeedback("Ошибка инвентаря", Color.red);
        }
    }

    void ShowFeedback(string message, Color color)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = color;
            CancelInvoke(nameof(ClearFeedback));
            Invoke(nameof(ClearFeedback), 2f);
        }
    }

    void ClearFeedback()
    {
        if (feedbackText != null)
        {
            feedbackText.text = "";
        }
    }

    void Update()
    {
        if (isUIOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseVendingMachine();
        }
    }

    [ContextMenu("Refresh Vending Machine")]
    public void RefreshVendingMachine()
    {
        SetupVendingMachine();
        Debug.Log("Vending Machine refreshed!");
    }
}