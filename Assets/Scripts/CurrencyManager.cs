using UnityEngine;
using TMPro;

public class CurrencyManager : MonoBehaviour
{
    [Header("Настройки валюты")]
    public int currentCurrency = 50;
    public int maxCurrency = 9999;

    [Header("UI Элементы")]
    public TextMeshProUGUI currencyText;

    public static CurrencyManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateCurrencyUI();
    }

    public void AddCurrency(int amount)
    {
        if (amount <= 0) return;

        currentCurrency = Mathf.Min(currentCurrency + amount, maxCurrency);
        UpdateCurrencyUI();
    }

    public bool SpendCurrency(int amount)
    {
        if (amount <= 0) return false;

        if (currentCurrency >= amount)
        {
            currentCurrency -= amount;
            UpdateCurrencyUI();
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool HasEnoughCurrency(int amount)
    {
        return currentCurrency >= amount;
    }

    void UpdateCurrencyUI()
    {
        if (currencyText != null)
        {
            currencyText.text = $"{currentCurrency}";
        }
    }
}