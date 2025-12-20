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
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        FindCurrencyTextIfNeeded();
        
        UpdateCurrencyUI();
        Debug.Log($"CurrencyManager запущен. Баланс: {currentCurrency}");
    }
    
    void FindCurrencyTextIfNeeded()
    {
        if (currencyText == null)
        {
            TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>(true);
            foreach (TextMeshProUGUI text in allTexts)
            {
                if (text.name.Contains("Currency", System.StringComparison.OrdinalIgnoreCase) ||
                    text.name.Contains("Balance", System.StringComparison.OrdinalIgnoreCase) ||
                    text.name.Contains("Money", System.StringComparison.OrdinalIgnoreCase) ||
                    text.name.Contains("Coins", System.StringComparison.OrdinalIgnoreCase))
                {
                    currencyText = text;
                    Debug.Log($"Автоматически найден currencyText: {text.name}");
                    break;
                }
            }
            
            if (currencyText == null && allTexts.Length > 0)
            {
                currencyText = allTexts[0];
                Debug.Log($"Автоматически назначен первый TextMeshPro: {currencyText.name}");
            }
        }
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
    
    public void UpdateCurrencyUI()
    {
        if (currencyText != null)
        {
            currencyText.text = $"{currentCurrency}";
        }
        else
        {
            Debug.LogWarning("Currency Text не назначен. Попытка найти...");
            FindCurrencyTextIfNeeded();
            
            if (currencyText != null)
            {
                currencyText.text = $"{currentCurrency}";
            }
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