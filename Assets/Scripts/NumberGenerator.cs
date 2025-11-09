using UnityEngine;

public class NumberGenerator : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactionText = "Press E to generate number";
    
    public void Interact()
    {
        int randomNumber = Random.Range(0, 100);
        Debug.Log($"Сгенерировано число: {randomNumber}");
    }
    
    public string GetInteractionText()
    {
        return interactionText;
    }
}