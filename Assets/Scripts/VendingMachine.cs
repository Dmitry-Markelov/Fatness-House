using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class VendingMachine : MonoBehaviour, IInteractable
{
    [Header("Основные настройки")]
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
    
    [Header("Товары")]
    [SerializeField] private InventoryItem[] manualItems = new InventoryItem[6];
    
    [Header("Эффекты")]
    [SerializeField] private AudioClip buySound;
    [SerializeField] private ParticleSystem buyParticles;
    
    private bool isUIOpen = false;
    private InventoryManager playerInventory; // ИЗМЕНИЛОСЬ: теперь InventoryManager

    void Start()
    {
        Debug.Log("=== VENDING MACHINE START ===");
        
        // Создаем CurrencyManager если его нет
        if (CurrencyManager.Instance == null)
        {
            CreateCurrencyManager();
        }
        
        // Находим InventoryManager на игроке
        FindPlayerInventory();
        
        if (vendingMachineUI != null)
            vendingMachineUI.SetActive(false);
        
        InitializeUI();
        CheckItems();
        
        Debug.Log($"✅ Вендинг инициализирован. Предметов: {CountItems()}, Баланс: {CurrencyManager.Instance.currentCurrency}");
    }
    
    void FindPlayerInventory()
    {
        Debug.Log("Поиск InventoryManager...");
        
        // Ищем InventoryManager в сцене
        InventoryManager inventory = FindObjectOfType<InventoryManager>();
        
        if (inventory != null)
        {
            playerInventory = inventory;
            Debug.Log($"✅ InventoryManager найден: {inventory.gameObject.name}");
            Debug.Log($"Слотов инвентаря: {inventory.inventorySlots}");
            
            // Проверяем инициализацию слотов
            playerInventory.CheckSlots();
            return;
        }
        
        // Если не нашли, ищем на игроке
        Debug.Log("InventoryManager не найден в сцене, ищу на игроке...");
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            player = GameObject.Find("Player");
            if (player == null)
            {
                player = GameObject.Find("PlayerController");
            }
        }
        
        if (player != null)
        {
            Debug.Log($"Игрок найден: {player.name}");
            playerInventory = player.GetComponent<InventoryManager>();
            
            if (playerInventory == null)
            {
                playerInventory = player.GetComponentInChildren<InventoryManager>();
            }
            
            if (playerInventory != null)
            {
                Debug.Log($"✅ InventoryManager найден на игроке");
                playerInventory.CheckSlots();
            }
            else
            {
                Debug.LogError("❌ InventoryManager не найден на игроке!");
                Debug.Log("Добавьте компонент InventoryManager на игрока или создайте его в сцене");
            }
        }
        else
        {
            Debug.LogError("❌ Игрок не найден в сцене!");
        }
    }
    
    void CreateCurrencyManager()
    {
        GameObject cmGO = new GameObject("CurrencyManager");
        CurrencyManager cm = cmGO.AddComponent<CurrencyManager>();
        cm.currentCurrency = 200;
        cm.maxCurrency = 9999;
        Debug.Log($"✅ CurrencyManager создан. Баланс: {cm.currentCurrency}");
    }

    void InitializeUI()
    {
        Debug.Log("Инициализация UI вендинга...");

        // Инициализируем кнопки покупки
        for (int i = 0; i < buyButtons.Length; i++)
        {
            int index = i;
            
            if (buyButtons[i] != null)
            {
                buyButtons[i].onClick.RemoveAllListeners();
                buyButtons[i].onClick.AddListener(() => BuyItem(index));
                Debug.Log($"Кнопка {i} инициализирована");
            }
            else
            {
                Debug.LogError($"❌ Кнопка {i} не назначена в Inspector!");
            }
        }

        // Кнопка закрытия
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseVendingMachine);
            Debug.Log("Кнопка закрытия инициализирована");
        }
        else
        {
            Debug.LogError("❌ Кнопка закрытия не назначена!");
        }

        // Обновляем отображение
        UpdateItemsDisplay();
    }
    
    void CheckItems()
    {
        Debug.Log("Проверка товаров...");
        for (int i = 0; i < manualItems.Length; i++)
        {
            if (manualItems[i] != null)
            {
                Debug.Log($"Слот {i}: {manualItems[i].itemName} - {manualItems[i].itemPrice} ГАНТ.");
            }
            else
            {
                Debug.LogWarning($"Слот {i}: ПУСТО");
            }
        }
    }
    
    int CountItems()
    {
        int count = 0;
        foreach (var item in manualItems)
        {
            if (item != null) count++;
        }
        return count;
    }
    
    void UpdateItemsDisplay()
    {
        Debug.Log("Обновление отображения товаров...");
        
        for (int i = 0; i < manualItems.Length; i++)
        {
            InventoryItem item = manualItems[i];
            
            // Имя предмета
            if (itemNameTexts != null && i < itemNameTexts.Length && itemNameTexts[i] != null)
            {
                itemNameTexts[i].text = item != null ? item.itemName : "EMPTY";
            }
            
            // Цена
            if (priceTexts != null && i < priceTexts.Length && priceTexts[i] != null)
            {
                priceTexts[i].text = item != null ? item.itemPrice.ToString() + " ГАНТ." : "---";
            }
            
            // Иконка
            if (itemIcons != null && i < itemIcons.Length && itemIcons[i] != null)
            {
                if (item != null && item.itemIcon != null)
                {
                    itemIcons[i].sprite = item.itemIcon;
                    itemIcons[i].color = Color.white;
                    Debug.Log($"Иконка товара {i}: {item.itemIcon.name}");
                }
                else if (item != null)
                {
                    itemIcons[i].sprite = null;
                    itemIcons[i].color = GetColorByIndex(i);
                }
                else
                {
                    itemIcons[i].sprite = null;
                    itemIcons[i].color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
                }
            }
        }
    }
    
    Color GetColorByIndex(int index)
    {
        Color[] colors = {
            Color.red, Color.green, Color.blue, 
            Color.yellow, Color.cyan, Color.magenta
        };
        return index < colors.Length ? colors[index] : Color.white;
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
        Debug.Log("=== ОТКРЫТИЕ ВЕНДИНГА ===");
        
        // Проверяем инвентарь перед открытием
        if (playerInventory == null)
        {
            Debug.LogError("❌ InventoryManager не найден! Не могу открыть вендинг.");
            ShowFeedback("Ошибка: инвентарь не найден", Color.red);
            return;
        }
        
        // Блокируем взаимодействие
        if (rayCast != null) 
            rayCast.SetInteractionEnabled(false);
        
        isUIOpen = true;
        
        // Показываем UI вендинга
        if (vendingMachineUI != null) 
            vendingMachineUI.SetActive(true);
        
        // Скрываем другие UI
        SetOtherUIVisibility(false);
        
        // Блокируем движение игрока
        if (playerMovement != null) 
            playerMovement.enabled = false;
        
        // Разблокируем курсор
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Обновляем UI
        UpdateUI();
    }
    
    void CloseVendingMachine()
    {
        Debug.Log("Закрытие вендингового аппарата");
        
        if (!isUIOpen) return;
        
        isUIOpen = false;
        
        // Скрываем UI вендинга
        if (vendingMachineUI != null) 
            vendingMachineUI.SetActive(false);
        
        // Показываем другие UI
        SetOtherUIVisibility(true);
        
        // Восстанавливаем движение игрока
        if (playerMovement != null) 
            playerMovement.enabled = true;
        
        // Восстанавливаем курсор
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Восстанавливаем RayCast
        StartCoroutine(EnableRayCastNextFrame());
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
        if (CurrencyManager.Instance != null && balanceText != null)
        {
            balanceText.text = $"ГАНТЕЛЕЙ: {CurrencyManager.Instance.currentCurrency}";
            Debug.Log($"Баланс в UI: {CurrencyManager.Instance.currentCurrency}");
        }
        else
        {
            Debug.LogError("CurrencyManager или balanceText не найдены!");
        }
    }
    
    void UpdateButtonsState()
    {
        if (CurrencyManager.Instance == null) 
        {
            Debug.LogError("CurrencyManager.Instance равен null!");
            return;
        }
        
        Debug.Log("Обновление состояния кнопок...");
        
        for (int i = 0; i < buyButtons.Length && i < manualItems.Length; i++)
        {
            if (buyButtons[i] != null)
            {
                if (manualItems[i] == null)
                {
                    buyButtons[i].interactable = false;
                    Debug.Log($"Кнопка {i}: неактивна (нет товара)");
                    continue;
                }

                if (manualItems[i].itemPrice <= 0)
                {
                    buyButtons[i].interactable = false;
                    Debug.Log($"Кнопка {i}: неактивна (цена 0)");
                    continue;
                }

                bool canAfford = CurrencyManager.Instance.HasEnoughCurrency(manualItems[i].itemPrice);
                buyButtons[i].interactable = canAfford;
                
                Debug.Log($"Кнопка {i}: {(canAfford ? "АКТИВНА" : "НЕАКТИВНА")} | " +
                         $"Цена: {manualItems[i].itemPrice} | " +
                         $"Баланс: {CurrencyManager.Instance.currentCurrency}");
            }
        }
    }
    
    public void BuyItem(int itemIndex)
    {
        Debug.Log($"=== ПОПЫТКА ПОКУПКИ ===");
        Debug.Log($"Слот: {itemIndex + 1}");
        
        // Проверка индекса
        if (itemIndex < 0 || itemIndex >= manualItems.Length || manualItems[itemIndex] == null)
        {
            ShowFeedback("Товар недоступен", Color.red);
            Debug.Log($"Ошибка: товар в слоте {itemIndex} не найден");
            return;
        }
        
        InventoryItem item = manualItems[itemIndex];
        
        // Проверка цены
        if (item.itemPrice <= 0)
        {
            ShowFeedback("Этот товар нельзя купить", Color.yellow);
            return;
        }
        
        // Проверка CurrencyManager
        if (CurrencyManager.Instance == null)
        {
            ShowFeedback("Ошибка системы валюты", Color.red);
            Debug.LogError("CurrencyManager.Instance is null!");
            return;
        }
        
        // Проверка инвентаря
        if (playerInventory == null)
        {
            ShowFeedback("Ошибка инвентаря", Color.red);
            Debug.LogError("❌ PlayerInventory не найден!");
            return;
        }
        
        // Проверяем баланс
        Debug.Log($"Баланс: {CurrencyManager.Instance.currentCurrency}, Цена: {item.itemPrice}");
        
        if (CurrencyManager.Instance.SpendCurrency(item.itemPrice))
        {
            Debug.Log($"✅ Деньги списаны. Новый баланс: {CurrencyManager.Instance.currentCurrency}");
            
            // Эффекты покупки
            PlayBuyEffects();
            
            // Добавляем в инвентарь
            AddItemToInventory(item);
            
            // Обновляем UI
            UpdateUI();
        }
        else
        {
            ShowFeedback($"Недостаточно гантелей! Нужно: {item.itemPrice}", Color.red);
            Debug.Log($"❌ Недостаточно средств. Баланс: {CurrencyManager.Instance.currentCurrency}, нужно: {item.itemPrice}");
        }
    }
    
    void PlayBuyEffects()
    {
        // Звук
        if (buySound != null)
        {
            AudioSource.PlayClipAtPoint(buySound, Camera.main.transform.position, 0.5f);
        }
        
        // Частицы
        if (buyParticles != null && vendingMachineUI != null)
        {
            ParticleSystem particles = Instantiate(buyParticles, vendingMachineUI.transform);
            Destroy(particles.gameObject, 2f);
        }
    }
    
    // ГЛАВНЫЙ МЕТОД: ДОБАВЛЕНИЕ ПРЕДМЕТА В ИНВЕНТАРЬ
    void AddItemToInventory(InventoryItem item)
    {
        if (item == null)
        {
            ShowFeedback("Ошибка: предмет не настроен", Color.red);
            Debug.LogError("Попытка добавить null предмет!");
            return;
        }
        
        if (playerInventory == null)
        {
            ShowFeedback("Ошибка: инвентарь не найден", Color.red);
            Debug.LogError("PlayerInventory равен null!");
            return;
        }
        
        Debug.Log($"=== ДОБАВЛЕНИЕ ПРЕДМЕТА В ИНВЕНТАРЬ ===");
        Debug.Log($"Предмет: {item.itemName}");
        Debug.Log($"Иконка: {(item.itemIcon != null ? item.itemIcon.name : "НЕТ")}");
        
        // Показываем текущее состояние инвентаря
        playerInventory.DebugInventory();
        
        // Вызываем метод AddItem из InventoryManager
        playerInventory.AddItem(item);
        
        ShowFeedback($"✅ Куплено: {item.itemName}", Color.green);
        
        // Показываем обновленный инвентарь
        playerInventory.DebugInventory();
        
        // Показываем инвентарь на 3 секунды
        StartCoroutine(ShowInventoryBriefly());
    }
    
    IEnumerator ShowInventoryBriefly()
    {
        Debug.Log("Показываю инвентарь...");
        
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(true);
            Debug.Log("Панель инвентаря активирована");
            
            yield return new WaitForSeconds(3f);
            
            // Если вендинг все еще открыт, скрываем инвентарь
            if (isUIOpen && inventoryUI != null)
            {
                inventoryUI.SetActive(false);
                Debug.Log("Панель инвентаря скрыта");
            }
        }
        else
        {
            Debug.LogWarning("Панель инвентаря (inventoryUI) не назначена!");
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
            Debug.Log($"Feedback: {message}");
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
        // Закрытие по Escape
        if (isUIOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseVendingMachine();
        }
        
        // Аварийное восстановление
        if (isUIOpen && Input.GetKeyDown(KeyCode.F12))
        {
            EmergencyClose();
        }
        
        // Тест: добавить тестовый предмет в инвентарь
        if (Input.GetKeyDown(KeyCode.T) && playerInventory != null)
        {
            Debug.Log("Тест: добавляю тестовый предмет...");
            playerInventory.TestAddItem();
        }
    }
    
    void EmergencyClose()
    {
        Debug.LogWarning("!!! АВАРИЙНОЕ ЗАКРЫТИЕ ВЕНДИНГА !!!");
        
        isUIOpen = false;
        
        // Принудительно закрываем все
        if (vendingMachineUI != null) 
            vendingMachineUI.SetActive(false);
        
        // Восстанавливаем все UI
        SetOtherUIVisibility(true);
        
        if (playerMovement != null) 
            playerMovement.enabled = true;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Ищем и восстанавливаем RayCast
        RayCast rc = FindObjectOfType<RayCast>();
        if (rc != null) 
        {
            rc.SetInteractionEnabled(true);
            Debug.Log("RayCast восстановлен");
        }
        
        Debug.Log("Аварийное закрытие завершено");
    }
    
    [ContextMenu("Проверить вендинг")]
    public void CheckVendingMachine()
    {
        Debug.Log("=== ПРОВЕРКА ВЕНДИНГА ===");
        Debug.Log($"Предметов: {CountItems()}/6");
        Debug.Log($"CurrencyManager: {(CurrencyManager.Instance != null ? "✅ Есть" : "❌ Нет")}");
        Debug.Log($"InventoryManager: {(playerInventory != null ? "✅ Есть" : "❌ Нет")}");
        
        if (CurrencyManager.Instance != null)
        {
            Debug.Log($"Баланс: {CurrencyManager.Instance.currentCurrency}");
        }
        
        if (playerInventory != null)
        {
            playerInventory.DebugInventory();
        }
        
        Debug.Log("========================");
    }
}