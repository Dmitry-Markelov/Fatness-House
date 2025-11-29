using UnityEngine;
using TMPro;

public class InteractionUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI interactionText;
    [SerializeField] private GameObject interactionPanel;

    [Header("Settings")]
    [SerializeField] private string defaultText = "Нажмите [Е] для использования";

    public static InteractionUI Instance;

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
        
        HideInteractionText();
    }

    public void ShowInteractionText(string customText = null)
    {
        if (interactionPanel != null)
            interactionPanel.SetActive(true);

        if (interactionText != null)
        {
            interactionText.text = string.IsNullOrEmpty(customText) ? defaultText : customText;
        }
    }

    public void HideInteractionText()
    {
        if (interactionPanel != null)
            interactionPanel.SetActive(false);
    }

    public void UpdateInteractionText(string newText)
    {
        if (interactionText != null && interactionPanel.activeInHierarchy)
        {
            interactionText.text = newText;
        }
    }
}