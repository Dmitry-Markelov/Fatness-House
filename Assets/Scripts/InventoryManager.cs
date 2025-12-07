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
    public Color emptyIconColor = new Color(1, 1, 1, 0.3f); // НОВОЕ: цвет пустой иконки
    
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
        SelectSlot(0);
        Debug.Log($"✅ InventoryManager инициализирован. Слотов: {inventorySlots}");
    }
    
    void InitializeSlots()
    {
        slotBackgrounds = new Image[inventorySlots];
        slotIcons = new Image[inventorySlots];
        slotNumbers = new TextMeshProUGUI[inventorySlots];
        
        Debug.Log("Инициализация слотов инвентаря...");
        
        for (int i = 0; i < inventorySlots; i++)
        {
            if (slotObjects[i] != null)
            {
                Debug.Log($"Слот {i}: GameObject = {slotObjects[i].name}");
                
                // Получаем фон слота
                slotBackgrounds[i] = slotObjects[i].GetComponent<Image>();
                if (slotBackgrounds[i] == null)
                {
                    Debug.LogWarning($"Слот {i}: нет Image компонента на GameObject");
                }
                
                // Ищем иконку (Item) ВНУТРИ SlotBackground
                Transform slotBackground = slotObjects[i].transform.Find("SlotBackground");
                if (slotBackground != null)
                {
                    Transform itemTransform = slotBackground.Find("Item");
                    if (itemTransform != null)
                    {
                        slotIcons[i] = itemTransform.GetComponent<Image>();
                        Debug.Log($"Слот {i}: иконка найдена - {itemTransform.name}");
                    }
                    else
                    {
                        Debug.LogWarning($"Слот {i}: не найден дочерний объект 'Item' внутри SlotBackground");
                        // Попробуем найти иконку другим способом
                        slotIcons[i] = FindIconInChildren(slotBackground);
                    }
                }
                else
                {
                    Debug.LogWarning($"Слот {i}: не найден 'SlotBackground'");
                    // Пытаемся найти иконку прямо в slotObjects[i]
                    slotIcons[i] = slotObjects[i].GetComponentInChildren<Image>();
                }
                
                // Ищем текст с номером
                Transform textTransform = slotObjects[i].transform.Find("Text (TMP)");
                if (textTransform != null)
                {
                    slotNumbers[i] = textTransform.GetComponent<TextMeshProUGUI>();
                    slotNumbers[i].text = (i + 1).ToString();
                }
                else
                {
                    // Ищем любой TextMeshPro в потомках
                    slotNumbers[i] = slotObjects[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (slotNumbers[i] != null)
                    {
                        slotNumbers[i].text = (i + 1).ToString();
                    }
                }
                
                // Очищаем слот
                ClearSlot(i);
            }
            else
            {
                Debug.LogError($"Слот {i}: GameObject не назначен в Inspector!");
            }
        }
        
        // Проверяем что все иконки найдены
        CheckIconsInitialized();
    }
    
    Image FindIconInChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            Image img = child.GetComponent<Image>();
            if (img != null && child.name.Contains("Icon") || child.name.Contains("Item"))
            {
                return img;
            }
        }
        return null;
    }
    
    void CheckIconsInitialized()
    {
        Debug.Log("=== ПРОВЕРКА ИНИЦИАЛИЗАЦИИ ИКОНОК ===");
        for (int i = 0; i < slotIcons.Length; i++)
        {
            if (slotIcons[i] == null)
            {
                Debug.LogError($"❌ Слот {i}: ИКОНКА НЕ НАЙДЕНА!");
                Debug.Log($"Попробуйте в Inspector назначить Image компонент вручную");
            }
            else
            {
                Debug.Log($"✅ Слот {i}: иконка инициализирована - {slotIcons[i].name}");
            }
        }
        Debug.Log("=================================");
    }
    
    void Update()
    {
        HandleSlotSelection();
        
        if (Input.GetMouseButtonDown(1))
        {
            UseSelectedItem();
        }
        
        // Отладка: показать инвентарь по клавише I
        if (Input.GetKeyDown(KeyCode.I))
        {
            DebugInventory();
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
        
        Debug.Log($"Выбран слот {selectedSlot + 1}");
    }
    
    void UseSelectedItem()
    {
        if (selectedSlot >= 0 && selectedSlot < items.Count && items[selectedSlot] != null)
        {
            InventoryItem item = items[selectedSlot];
            Debug.Log($"Используется предмет: {item.itemName} из слота {selectedSlot + 1}");
            
            item.Use();
            
            if (item.consumable)
            {
                RemoveItem(selectedSlot);
            }
        }
        else
        {
            Debug.Log("В выбранном слоте нет предмета");
        }
    }
    
    // ГЛАВНЫЙ МЕТОД: ДОБАВЛЕНИЕ ПРЕДМЕТА
    public void AddItem(InventoryItem newItem)
    {
        if (newItem == null)
        {
            Debug.LogError("Попытка добавить null предмет!");
            return;
        }

        Debug.Log($"=== ПОПЫТКА ДОБАВИТЬ ПРЕДМЕТ ===");
        Debug.Log($"Предмет: {newItem.itemName}");
        Debug.Log($"Иконка: {(newItem.itemIcon != null ? newItem.itemIcon.name : "НЕТ")}");

        // Ищем пустой сlot
        for (int i = 0; i < inventorySlots; i++)
        {
            // Расширяем список если нужно
            while (items.Count <= i)
            {
                items.Add(null);
                Debug.Log($"Добавлен null элемент в items. Теперь длина: {items.Count}");
            }
            
            // Если слот пустой
            if (items[i] == null)
            {
                // Добавляем предмет
                items[i] = newItem;
                
                // ВАЖНО: ОБНОВЛЯЕМ ИКОНКУ
                UpdateSlotIcon(i, newItem);
                
                Debug.Log($"✅ Предмет добавлен в слот {i + 1}: {newItem.itemName}");
                DebugInventory();
                return;
            }
        }
        
        Debug.Log("❌ Инвентарь полон! Не могу добавить: " + newItem.itemName);
        DebugInventory();
    }
    
    void UpdateSlotIcon(int slotIndex, InventoryItem item)
    {
        if (slotIcons == null || slotIndex >= slotIcons.Length)
        {
            Debug.LogError($"Не могу обновить иконку: slotIcons не инициализирован или индекс {slotIndex} вне диапазона");
            return;
        }
        
        if (slotIcons[slotIndex] == null)
        {
            Debug.LogError($"Слот {slotIndex}: slotIcons[{slotIndex}] = NULL!");
            return;
        }
        
        Image iconImage = slotIcons[slotIndex];
        
        if (item.itemIcon != null)
        {
            iconImage.sprite = item.itemIcon;
            iconImage.color = Color.white;
            iconImage.gameObject.SetActive(true);
            Debug.Log($"✅ Иконка установлена в слоте {slotIndex + 1}: {item.itemIcon.name}");
        }
        else
        {
            // Если у предмета нет иконки, показываем цветной квадрат
            iconImage.sprite = null;
            iconImage.color = GetColorByItemName(item.itemName);
            iconImage.gameObject.SetActive(true);
            Debug.Log($"⚠️ У предмета нет иконки, показываю цветной квадрат");
        }
    }
    
    Color GetColorByItemName(string name)
    {
        // Генерируем уникальный цвет на основе имени
        int hash = name.GetHashCode();
        return new Color(
            (hash & 0xFF) / 255f,
            ((hash >> 8) & 0xFF) / 255f,
            ((hash >> 16) & 0xFF) / 255f,
            1f
        );
    }
    
    public void RemoveItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= items.Count) return;
        
        Debug.Log($"Удаляю предмет из слота {slotIndex + 1}");
        
        items[slotIndex] = null;
        ClearSlot(slotIndex);
    }
    
    void ClearSlot(int slotIndex)
    {
        if (slotIcons[slotIndex] != null)
        {
            slotIcons[slotIndex].sprite = null;
            slotIcons[slotIndex].color = emptyIconColor;
            slotIcons[slotIndex].gameObject.SetActive(true);
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

    public void DebugInventory()
    {
        Debug.Log("=== СОДЕРЖИМОЕ ИНВЕНТАРЯ ===");
        for (int i = 0; i < inventorySlots; i++)
        {
            string itemName = i < items.Count && items[i] != null ? items[i].itemName : "ПУСТО";
            Debug.Log($"Слот {i + 1}: {itemName}");
        }
        Debug.Log("=========================");
    }
    
    [ContextMenu("Проверить инициализацию слотов")]
    public void CheckSlots()
    {
        Debug.Log("=== ПРОВЕРКА СЛОТОВ ИНВЕНТАРЯ ===");
        for (int i = 0; i < inventorySlots; i++)
        {
            Debug.Log($"Слот {i}:");
            Debug.Log($"  GameObject: {(slotObjects[i] != null ? slotObjects[i].name : "NULL")}");
            Debug.Log($"  SlotIcon: {(slotIcons != null && i < slotIcons.Length && slotIcons[i] != null ? slotIcons[i].name : "NULL")}");
            Debug.Log($"  Item: {(i < items.Count && items[i] != null ? items[i].itemName : "NULL")}");
        }
        Debug.Log("================================");
    }
    
    [ContextMenu("Тест: добавить тестовый предмет")]
    public void TestAddItem()
    {
        // Создаем тестовый предмет
        InventoryItem testItem = ScriptableObject.CreateInstance<InventoryItem>();
        testItem.itemName = "ТЕСТОВЫЙ ПРЕДМЕТ";
        testItem.consumable = true;
        
        // Добавляем
        AddItem(testItem);
    }
}