using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class FillBarGame : MonoBehaviour, IInteractable
{
    [Header("Настройки игры")]
    [SerializeField] private float fillAmountPerClick = 0.05f;
    [SerializeField] private float decreaseSpeed = 0.001f;
    [SerializeField] private float gameTime = 60f;
    [SerializeField] private float penaltyMultiplier = 2f;

    [Header("Настройки стамины")]
    [SerializeField] private bool requireFullStamina = true;
    [SerializeField] private bool drainStaminaToZero = true;
    [SerializeField] private float staminaDrainAmount = 100f;
    
    [Header("Награда за успех")]
    [SerializeField] private bool giveRewardOnWin = true;
    [SerializeField] private int currencyReward = 100;
    
    [Header("UI Elements to Hide")]
    [SerializeField] private GameObject staminaBarUI;
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private GameObject balanceUI;
    [SerializeField] private GameObject cursorUI;
    
    [Header("Цветовая схема #A1D9A0")]
    [SerializeField] private Color mainColor = new Color(0.631f, 0.851f, 0.627f, 1f);
    [SerializeField] private Color darkColor = new Color(0.431f, 0.651f, 0.427f, 1f);
    [SerializeField] private Color lightColor = new Color(0.831f, 0.951f, 0.827f, 1f);
    [SerializeField] private Color backgroundColor = new Color(0.631f, 0.851f, 0.627f, 0.3f);
    
    [Header("UI элементы")]
    [SerializeField] private Image progressBarFill;
    [SerializeField] private Image progressBarBackground;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Button wButton;
    [SerializeField] private Button eButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject panel;

    [Header("Интерактивные настройки")]
    [SerializeField] private string interactionText = "Нажмите [E] для тренировки";
    [SerializeField] private string notEnoughStaminaText = "Нужна полная стамина!";
    [SerializeField] private bool isInteractable = true;

    private bool isGameActive = false;
    private float currentTime;
    private string correctButton;
    private float currentFillAmount;
    private bool canActivate = true;
    private PlayerMovement playerMovement;
    private CurrencyManager currencyManager;

    void Start()
    {
        InitializeGame();
        SetupButtons();
        
        playerMovement = FindFirstObjectByType<PlayerMovement>();
        
        currencyManager = FindFirstObjectByType<CurrencyManager>();
        if (currencyManager == null)
        {
            Debug.LogWarning("CurrencyManager не найден в сцене!");
        }
        
        if (panel != null)
        {
            panel.SetActive(false);
        }
        
        ResetGameState();
    }

    void Update()
    {
        if (isGameActive)
        {
            HandleGameInput();
            DecreaseProgressBar();
            UpdateTimer();
            CheckGameConditions();
            UpdateProgressBarVisual();
        }
    }

    void SetOtherUIVisibility(bool visible)
    {
        if (staminaBarUI != null)
            staminaBarUI.SetActive(visible);
        
        if (inventoryUI != null)
            inventoryUI.SetActive(visible);
        
        if (balanceUI != null)
            balanceUI.SetActive(visible);

        if (cursorUI != null)
            cursorUI.SetActive(visible);  
    }

    public void Interact()
    {
        if (!isInteractable || !canActivate || isGameActive) return;
        
        if (!CheckStaminaRequirement())
        {
            Debug.Log("Недостаточно стамины для тренировки");
            return;
        }
        
        ActivateGame();
    }
    
    public string GetInteractionText()
    {
        if (!isInteractable) return "";
        
        if (requireFullStamina && !HasFullStamina())
        {
            return notEnoughStaminaText;
        }
        
        return interactionText;
    }

    bool CheckStaminaRequirement()
    {
        if (playerMovement == null)
        {
            Debug.LogWarning("PlayerMovement не найден!");
            return true;
        }
        
        if (requireFullStamina)
        {
            bool hasFullStamina = HasFullStamina();
            if (!hasFullStamina)
            {
                Debug.Log($"Требуется полная стамина. Текущая: {playerMovement.Stamina}/{playerMovement.MaxStamina}");
                return false;
            }
        }
        
        return true;
    }

    bool HasFullStamina()
    {
        if (playerMovement == null) return false;
        
        const float tolerance = 0.01f;
        float staminaPercentage = playerMovement.Stamina / playerMovement.MaxStamina;
        return staminaPercentage >= (1f - tolerance);
    }

    void DrainStamina()
    {
        if (playerMovement == null || !drainStaminaToZero) return;
        
        float drainAmount = (staminaDrainAmount / 100f) * playerMovement.MaxStamina;
        playerMovement.Stamina = Mathf.Max(0, playerMovement.Stamina - drainAmount);
        
        playerMovement.UpdateStaminaBar();
        
        Debug.Log($"Снято {staminaDrainAmount}% стамины. Осталось: {playerMovement.Stamina}/{playerMovement.MaxStamina}");
    }

    void GiveCurrencyReward()
    {
        if (!giveRewardOnWin || currencyManager == null) return;
        
        currencyManager.AddCurrency(currencyReward);
        Debug.Log($"Награда за успешную тренировку: +{currencyReward} валюты");
        
        ShowRewardEffect();
    }

    void ShowRewardEffect()
    {  
        if (timerText != null)
        {
            StartCoroutine(ShowRewardText());
        }
    }

    IEnumerator ShowRewardText()
    {
        string originalText = timerText.text;
        Color originalColor = timerText.color;

        timerText.text = $"+{currencyReward} валюты!";
        timerText.color = Color.yellow;
        
        yield return new WaitForSeconds(2f);

        timerText.text = originalText;
        timerText.color = originalColor;
    }

    void ActivateGame()
    {
        DrainStamina();
        
        ResetGameState();
        SetOtherUIVisibility(false);

        RayCast rayCast = FindFirstObjectByType<RayCast>();
        if (rayCast != null)
        {
            rayCast.SetInteractionEnabled(false);
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (panel != null)
        {
            panel.SetActive(true);
        }
        
        StartGame();
    }

    void DeactivateGame()
    {
        SetOtherUIVisibility(true);
        
        RayCast rayCast = FindFirstObjectByType<RayCast>();
        if (rayCast != null)
        {
            rayCast.SetInteractionEnabled(true);
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (panel != null)
        {
            panel.SetActive(false);
        }

        isGameActive = false;
        canActivate = true;
    }

    void ResetGameState()
    {
        currentTime = gameTime;
        currentFillAmount = 0.5f;
        isGameActive = false;
        canActivate = true;
        
        UpdateTimerDisplay();
        UpdateProgressBarVisual();
        
        if (timerText != null)
        {
            timerText.color = darkColor;
            timerText.fontStyle = FontStyles.Bold;
        }
        
        SelectNewCorrectButton();
        
        Debug.Log("Состояние игры сброшено. Начинаем с середины.");
    }

    void HandleGameInput()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            OnButtonClick("W");
        }
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            OnButtonClick("E");
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnQuitClick();
        }
    }

    void InitializeGame()
    {
        FindUIElements();
        SetupProgressBar();
        ApplyColorScheme();
    }

    void FindUIElements()
    {
        if (progressBarFill == null)
        {
            GameObject progressBar = GameObject.Find("ProgressBar");
            if (progressBar != null)
            {
                Image[] images = progressBar.GetComponentsInChildren<Image>();
                if (images.Length >= 2)
                {
                    progressBarBackground = images[0];
                    progressBarFill = images[1];
                }
                else if (images.Length == 1)
                {
                    progressBarFill = images[0];
                }
            }
        }
        
        if (timerText == null)
            timerText = GameObject.Find("Timer")?.GetComponent<TextMeshProUGUI>();
        
        if (wButton != null)
            wButton = GameObject.Find("W")?.GetComponent<Button>();
        
        if (eButton != null)
            eButton = GameObject.Find("E")?.GetComponent<Button>();
        
        if (quitButton != null)
            quitButton = GameObject.Find("Quit")?.GetComponent<Button>();
        
        if (panel != null)
            panel = GameObject.Find("Panel");
    }

    void ApplyColorScheme()
    {
        if (progressBarBackground != null)
        {
            progressBarBackground.color = backgroundColor;
        }
        
        if (quitButton != null)
        {
            Image quitImage = quitButton.GetComponent<Image>();
            TextMeshProUGUI quitText = quitButton.GetComponentInChildren<TextMeshProUGUI>();
            
            if (quitImage != null)
            {
                quitImage.color = mainColor;
            }
            if (quitText != null)
            {
                quitText.color = Color.white;
                quitText.fontStyle = FontStyles.Bold;
            }
        }
    }

    void SetupProgressBar()
    {
        if (progressBarFill != null)
        {
            progressBarFill.type = Image.Type.Filled;
            progressBarFill.fillMethod = Image.FillMethod.Vertical;
            progressBarFill.fillOrigin = 0;
        }
    }

    void SetupButtons()
    {
        if (wButton != null)
        {
            wButton.onClick.RemoveAllListeners();
            wButton.onClick.AddListener(() => OnButtonClick("W"));
        }

        if (eButton != null)
        {
            eButton.onClick.RemoveAllListeners();
            eButton.onClick.AddListener(() => OnButtonClick("E"));
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(OnQuitClick);
        }

        SelectNewCorrectButton();
    }

    void OnButtonClick(string buttonName)
    {
        if (!isGameActive) return;

        bool isCorrect = (buttonName == correctButton);
        
        if (isCorrect)
        {
            currentFillAmount = Mathf.Clamp01(currentFillAmount + fillAmountPerClick);
            FlashButton(buttonName, lightColor);
            SelectNewCorrectButton();
        }
        else
        {
            currentFillAmount = Mathf.Clamp01(currentFillAmount - (fillAmountPerClick * penaltyMultiplier));
            FlashButton(buttonName, new Color(0.9f, 0.4f, 0.4f, 1f));
        }
        
        UpdateProgressBarVisual();
    }

    void OnQuitClick()
    {
        DeactivateGame();
    }

    void SelectNewCorrectButton()
    {
        correctButton = (Random.Range(0, 2) == 0) ? "W" : "E";
        UpdateButtonsAppearance();
    }

    void UpdateButtonsAppearance()
    {
        if (wButton != null)
        {
            UpdateButtonAppearance(wButton, "W", correctButton == "W");
        }
        
        if (eButton != null)
        {
            UpdateButtonAppearance(eButton, "E", correctButton == "E");
        }
    }

    void UpdateButtonAppearance(Button button, string text, bool isCorrect)
    {
        Image buttonImage = button.GetComponent<Image>();
        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        
        if (buttonImage != null)
        {
            buttonImage.color = isCorrect ? mainColor : Color.white;
        }
        
        if (buttonText != null)
        {
            buttonText.text = text;
            buttonText.color = isCorrect ? darkColor : new Color(0.2f, 0.2f, 0.2f, 1f);
            buttonText.fontStyle = isCorrect ? FontStyles.Bold : FontStyles.Normal;
        }
    }

    void FlashButton(string buttonName, Color color)
    {
        Button button = (buttonName == "W") ? wButton : eButton;
        if (button == null) return;
        
        StartCoroutine(FlashButtonCoroutine(button, color, 0.1f));
    }

    IEnumerator FlashButtonCoroutine(Button button, Color flashColor, float duration)
    {
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage == null) yield break;
        
        Color originalColor = buttonImage.color;
        buttonImage.color = flashColor;
        
        yield return new WaitForSeconds(duration);
        
        bool isCorrect = (button == wButton && correctButton == "W") || 
                        (button == eButton && correctButton == "E");
        buttonImage.color = isCorrect ? mainColor : Color.white;
    }

    void DecreaseProgressBar()
    {
        currentFillAmount = Mathf.Clamp01(currentFillAmount - (decreaseSpeed * Time.deltaTime));
    }

    void UpdateTimer()
    {
        currentTime -= Time.deltaTime;
        UpdateTimerDisplay();
        
        if (currentTime <= 0)
        {
            EndGame(false);
        }
    }

    void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(currentTime);
            
            timerText.text = $"{seconds}";

            if (currentTime < 10)
            {
                float blink = Mathf.Sin(Time.time * 5f) * 0.3f + 0.7f;
                timerText.color = Color.Lerp(new Color(0.9f, 0.3f, 0.3f, 1f), darkColor, blink);
            }
            else if (currentTime < 20)
            {
                timerText.color = Color.Lerp(new Color(0.9f, 0.6f, 0.2f, 1f), darkColor, (currentTime - 10f) / 10f);
            }
            else
            {
                timerText.color = darkColor;
            }
        }
    }

    void UpdateProgressBarVisual()
    {
        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = currentFillAmount;

            if (currentFillAmount > 0.8f)
                progressBarFill.color = mainColor;
            else if (currentFillAmount > 0.6f)
                progressBarFill.color = Color.Lerp(darkColor, mainColor, (currentFillAmount - 0.6f) / 0.2f);
            else if (currentFillAmount > 0.4f)
                progressBarFill.color = Color.Lerp(new Color(1f, 0.8f, 0.4f, 1f), darkColor, (currentFillAmount - 0.4f) / 0.2f);
            else if (currentFillAmount > 0.2f)
                progressBarFill.color = Color.Lerp(new Color(0.9f, 0.5f, 0.5f, 1f), new Color(1f, 0.8f, 0.4f, 1f), (currentFillAmount - 0.2f) / 0.2f);
            else
                progressBarFill.color = new Color(0.9f, 0.4f, 0.4f, 1f);
        }
    }

    void CheckGameConditions()
    {
        if (currentFillAmount >= 0.99f)
        {
            EndGame(true);
            return;
        }
        
        if (currentFillAmount <= 0.01f)
        {
            EndGame(false);
        }
    }

    void StartGame()
    {
        isGameActive = true;
        canActivate = false;
        
        if (wButton != null) 
        {
            wButton.interactable = true;
            wButton.GetComponent<Image>().color = Color.white;
        }
        if (eButton != null) 
        {
            eButton.interactable = true;
            eButton.GetComponent<Image>().color = Color.white;
        }
        
        UpdateButtonsAppearance();
        
        Debug.Log("Игра началась. Заполнение: 50%");
    }

    void EndGame(bool isWin)
    {
        if (!isGameActive) return;
        
        isGameActive = false;
        
        if (timerText != null)
        {
            if (isWin)
            {
                timerText.text = "УСПЕХ";
                timerText.color = mainColor;
                
                if (giveRewardOnWin)
                {
                    GiveCurrencyReward();
                }
            }
            else
            {
                timerText.text = "ПРОВАЛ";
                timerText.color = new Color(0.8f, 0.3f, 0.3f, 1f);
            }
            timerText.fontStyle = FontStyles.Bold;
        }

        if (wButton != null) 
        {
            wButton.interactable = false;
            wButton.GetComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f, 0.5f);
        }
        
        if (eButton != null) 
        {
            eButton.interactable = false;
            eButton.GetComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f, 0.5f);
        }
        
        StartCoroutine(AutoCloseAfterDelay(3f));
        
        Debug.Log($"Игра завершена. Результат: {(isWin ? "Победа" : "Поражение")}. Заполнение: {currentFillAmount:P0}");
    }

    IEnumerator AutoCloseAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        DeactivateGame();
    }

    public void SetInteractable(bool value)
    {
        isInteractable = value;
    }
    
    public bool IsInteractable()
    {
        return isInteractable;
    }
    
    public void SetStaminaRequirements(bool requireFull, bool drainToZero, float drainAmount = 100f)
    {
        requireFullStamina = requireFull;
        drainStaminaToZero = drainToZero;
        staminaDrainAmount = drainAmount;
    }
    
    public void SetRewardSettings(bool giveReward, int rewardAmount = 100)
    {
        giveRewardOnWin = giveReward;
        currencyReward = rewardAmount;
    }
}