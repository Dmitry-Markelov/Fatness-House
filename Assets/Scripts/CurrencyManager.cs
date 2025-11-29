using UnityEngine;
using TMPro;
using System.Collections;

public class CurrencyManager : MonoBehaviour
{
    [Header("Настройки валюты")]
    public string currencyName = "Гантель";
    public int currentCurrency = 0;
    public int maxCurrency = 9999;

    [Header("UI Элементы")]
    public TextMeshProUGUI currencyText;
    public GameObject currencyIcon;

    [Header("Визуальные эффекты")]
    public ParticleSystem collectEffect;
    public AudioClip collectSound;

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
        UpdateCurrencyUI();
    }

    public void AddCurrency(int amount)
    {
        if (amount <= 0) return;

        currentCurrency = Mathf.Min(currentCurrency + amount, maxCurrency);

        if (collectEffect != null)
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        
        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, Camera.main.transform.position);

        UpdateCurrencyUI();
        Debug.Log($"Получено {amount} {currencyName}. Всего: {currentCurrency}");
    }

    public bool SpendCurrency(int amount)
    {
        if (amount <= 0) return false;

        if (currentCurrency >= amount)
        {
            currentCurrency -= amount;
            UpdateCurrencyUI();
            Debug.Log($"Потрачено {amount} {currencyName}. Осталось: {currentCurrency}");
            return true;
        }
        else
        {
            Debug.Log($"Недостаточно {currencyName}. Нужно: {amount}, есть: {currentCurrency}");
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

    public void ResetCurrency()
    {
        currentCurrency = 0;
        UpdateCurrencyUI();
    }
}