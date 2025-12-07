using UnityEngine;
using TMPro;

public class CurrencyManager : MonoBehaviour
{
    [Header("Настройки валюты")]
    public int currentCurrency = 200;
    public int maxCurrency = 9999;
    
    [Header("UI Элементы")]
    public TextMeshProUGUI currencyText;
    
    public static CurrencyManager Instance;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Не уничтожать при загрузке новой сцены
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        UpdateCurrencyUI();
        Debug.Log($"CurrencyManager запущен. Баланс: {currentCurrency}");
    }
    
    public void AddCurrency(int amount)
    {
        if (amount <= 0) return;
        
        currentCurrency = Mathf.Min(currentCurrency + amount, maxCurrency);
        UpdateCurrencyUI();
        Debug.Log($"Добавлено {amount} гантелей. Новый баланс: {currentCurrency}");
    }
    
    public bool SpendCurrency(int amount)
    {
        if (amount <= 0) return false;
        
        if (currentCurrency >= amount)
        {
            currentCurrency -= amount;
            UpdateCurrencyUI();
            Debug.Log($"Потрачено {amount} гантелей. Остаток: {currentCurrency}");
            return true;
        }
        else
        {
            Debug.Log($"Недостаточно гантелей. Нужно: {amount}, есть: {currentCurrency}");
            return false;
        }
    }
    
    public bool HasEnoughCurrency(int amount)
    {
        bool enough = currentCurrency >= amount;
        if (!enough)
        {
            Debug.Log($"Проверка: {amount} гантелей. Баланс: {currentCurrency}. Достаточно: {enough}");
        }
        return enough;
    }
    
    void UpdateCurrencyUI()
    {
        if (currencyText != null)
        {
            currencyText.text = $"{currentCurrency}";
        }
        else
        {
            Debug.LogWarning("Currency Text не назначен");
        }
    }
    
    [ContextMenu("Добавить 100 гантелей")]
    public void AddTestCurrency()
    {
        AddCurrency(100);
    }
    
    [ContextMenu("Сбросить баланс")]
    public void ResetCurrency()
    {
        currentCurrency = 200;
        UpdateCurrencyUI();
        Debug.Log("Баланс сброшен до 200");
    }
}