using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class QTEGame2 : MonoBehaviour, IInteractable
{
    [Header("Настройки QTE")]
    [SerializeField] private float fillAmountPerPress = 0.02f;
    [SerializeField] private float decreaseSpeed = 0.1f;
    [SerializeField] private float gameTime = 30f;
    [SerializeField] private float cooldownTime = 0.2f;
    
    [Header("Настройки стамины")]
    [SerializeField] private bool requireFullStamina = true;
    [SerializeField] private bool drainStaminaToZero = true;
    [SerializeField] private float staminaDrainAmount = 100f;
    
    [Header("Награда за успех")]
    [SerializeField] private bool giveRewardOnWin = true;
    [SerializeField] private int currencyReward = 100;
    
    [Header("Цветовая схема #A1D9A0")]
    [SerializeField] private Color mainColor = new Color(0.631f, 0.851f, 0.627f, 1f);
    [SerializeField] private Color darkColor = new Color(0.431f, 0.651f, 0.427f, 1f);
    [SerializeField] private Color lightColor = new Color(0.831f, 0.951f, 0.827f, 1f);
    [SerializeField] private Color backgroundColor = new Color(0.631f, 0.851f, 0.627f, 0.3f);
    
    [Header("UI Elements to Hide")]
    [SerializeField] private GameObject staminaBarUI;
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private GameObject balanceUI;
    [SerializeField] private GameObject cursorUI;
    
    [Header("UI элементы")]
    [SerializeField] private Image progressBarFill;
    [SerializeField] private Image progressBarBackground;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject wKeyHighlight;

    [Header("Интерактивные настройки")]
    [SerializeField] private string interactionText = "Нажмите [E] для быстрой тренировки";
    [SerializeField] private string notEnoughStaminaText = "Нужна полная стамина!";
    [SerializeField] private bool isInteractable = true;

    private bool isGameActive = false;
    private float currentTime;
    private float currentFillAmount;
    private bool canActivate = true;
    private bool canPress = true;
    private int pressCount = 0;
    private PlayerMovement playerMovement;
    private CurrencyManager currencyManager;
    private Color originalWKeyColor = Color.white;
    private Coroutine endGameCoroutine;

    void Start()
    {
        InitializeGame();
        SetupButtons();
        
        playerMovement = FindFirstObjectByType<PlayerMovement>();
        currencyManager = FindFirstObjectByType<CurrencyManager>();
        
        if (panel != null)
        {
            panel.SetActive(false);
        }
        
        if (wKeyHighlight != null)
        {
            wKeyHighlight.SetActive(false);
            Image wImage = wKeyHighlight.GetComponent<Image>();
            if (wImage != null)
            {
                originalWKeyColor = wImage.color;
            }
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
            UpdateUI();
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
            Debug.Log("Недостаточно стамины для QTE");
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
        if (playerMovement == null) return true;
        
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
        
        Debug.Log($"Снято {staminaDrainAmount}% стамины для QTE. Осталось: {playerMovement.Stamina}/{playerMovement.MaxStamina}");
    }

    void HandleGameInput()
    {
        if (!canPress) return;
        
        if (Input.GetKeyDown(KeyCode.W))
        {
            OnSuccessfulPress();
        }
    }

    void OnSuccessfulPress()
    {
        pressCount++;
        
        currentFillAmount = Mathf.Clamp01(currentFillAmount + fillAmountPerPress);
        
        StartCoroutine(PressCooldown());
        
        FlashWKey();
        
        Debug.Log($"Нажатие W #{pressCount}. Заполнение: {currentFillAmount:P0}");
    }

    IEnumerator PressCooldown()
    {
        canPress = false;
        yield return new WaitForSeconds(cooldownTime);
        canPress = true;
    }

    void FlashWKey()
    {
        if (wKeyHighlight != null)
        {
            StartCoroutine(FlashWKeyCoroutine());
        }
    }

    IEnumerator FlashWKeyCoroutine()
    {
        if (wKeyHighlight == null) yield break;
        
        Image highlightImage = wKeyHighlight.GetComponent<Image>();
        if (highlightImage == null) yield break;
        
        highlightImage.color = mainColor;
        
        yield return new WaitForSeconds(0.1f);
        
        highlightImage.color = originalWKeyColor;
    }

    void GiveCurrencyReward()
    {
        if (!giveRewardOnWin) return;
        
        if (currencyManager == null)
        {
            currencyManager = FindFirstObjectByType<CurrencyManager>();
            if (currencyManager == null)
            {
                Debug.LogWarning("Не удалось найти CurrencyManager для выдачи награды!");
                return;
            }
        }
        
        currencyManager.AddCurrency(currencyReward);
        Debug.Log($"Награда за успешную QTE: +{currencyReward} валюты");
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
        
        if (wKeyHighlight != null)
        {
            wKeyHighlight.SetActive(true);
            Image wImage = wKeyHighlight.GetComponent<Image>();
            if (wImage != null)
            {
                wImage.color = originalWKeyColor;
            }
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
        
        if (wKeyHighlight != null)
        {
            wKeyHighlight.SetActive(false);
        }

        isGameActive = false;
        canActivate = true;
        
        // Останавливаем корутину, если она активна
        if (endGameCoroutine != null)
        {
            StopCoroutine(endGameCoroutine);
            endGameCoroutine = null;
        }
    }

    void ResetGameState()
    {
        currentTime = gameTime;
        currentFillAmount = 0.5f;
        pressCount = 0;
        isGameActive = false;
        canActivate = true;
        canPress = true;
        
        UpdateUI();
        
        if (timerText != null)
        {
            timerText.color = darkColor;
            timerText.fontStyle = FontStyles.Bold;
        }
        
        Debug.Log("QTE состояние сброшено. Начинаем с середины.");
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
        
        if (quitButton == null)
            quitButton = GameObject.Find("Quit")?.GetComponent<Button>();
        
        if (panel == null)
            panel = GameObject.Find("Panel");
            
        if (wKeyHighlight == null)
            wKeyHighlight = GameObject.Find("W");
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
            progressBarFill.fillMethod = Image.FillMethod.Horizontal;
            progressBarFill.fillOrigin = 0;
        }
    }

    void SetupButtons()
    {
        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(OnQuitClick);
        }
    }

    void OnQuitClick()
    {
        DeactivateGame();
    }

    void DecreaseProgressBar()
    {
        currentFillAmount = Mathf.Clamp01(currentFillAmount - (decreaseSpeed * Time.deltaTime));
    }

    void UpdateTimer()
    {
        currentTime -= Time.deltaTime;
        
        if (currentTime <= 0)
        {
            EndGame(false);
        }
    }

    void UpdateUI()
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
        canPress = true;
        
        Debug.Log("QTE началась. Быстро нажимайте W!");
    }

    void EndGame(bool isWin)
    {
        if (!isGameActive) return;
        
        isGameActive = false;
        canPress = false;
        
        // Немедленно показываем результат
        if (timerText != null)
        {
            if (isWin)
            {
                timerText.text = "УСПЕХ";
                timerText.color = mainColor;
                
                // Выдаем награду сразу
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
        
        // Запускаем корутину для завершения игры
        endGameCoroutine = StartCoroutine(EndGameSequence(isWin));
        
        Debug.Log($"QTE завершена. Результат: {(isWin ? "Победа" : "Поражение")}. Нажатий: {pressCount}");
    }

    IEnumerator EndGameSequence(bool isWin)
    {
        if (isWin && giveRewardOnWin && timerText != null)
        {
            yield return new WaitForSeconds(1f);

            timerText.text = $"+{currencyReward} валюты!";
            timerText.color = Color.yellow;
            timerText.fontStyle = FontStyles.Bold;
            
            yield return new WaitForSeconds(2f);
            
            timerText.text = "УСПЕХ";
            timerText.color = mainColor;
            
            yield return new WaitForSeconds(1f);
        }
        else
        {
            yield return new WaitForSeconds(3f);
        }

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