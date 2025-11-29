using UnityEngine;
using TMPro;

public class InteractionHint : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI hintText;
    public GameObject hintPanel;

    void Start()
    {
        HideHint();
    }

    public void ShowHint(string text)
    {
        if (hintText != null)
            hintText.text = text;
        
        if (hintPanel != null)
            hintPanel.SetActive(true);
    }

    public void HideHint()
    {
        if (hintPanel != null)
            hintPanel.SetActive(false);
    }
}