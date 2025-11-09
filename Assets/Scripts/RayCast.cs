using UnityEngine;

interface IInteractable
{
    public void Interact();
    public string GetInteractionText();
}

public class RayCast : MonoBehaviour
{
    [Header("Raycast Settings")]
    public Transform InteractorSource;
    public float InteractRange = 3f;
    
    [Header("UI Settings (Optional)")]
    public GameObject interactionHintUI;
    
    private IInteractable currentInteractable;
    private bool canInteract = true;

    void Update()
    {
        if (!canInteract)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SetInteractionEnabled(true);
            }
            return;
        }

        Ray ray = new Ray(InteractorSource.position, InteractorSource.forward);
        RaycastHit hitInfo;
        bool hitInteractable = false;
        IInteractable interactable = null;

        if (Physics.Raycast(ray, out hitInfo, InteractRange))
        {
            hitInteractable = hitInfo.collider.gameObject.TryGetComponent(out interactable);
        }

        if (hitInteractable)
        {
            currentInteractable = interactable;
            UpdateInteractionHint(true);
        }
        else
        {
            currentInteractable = null;
            UpdateInteractionHint(false);
        }

        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    private void UpdateInteractionHint(bool show)
    {
        if (interactionHintUI != null)
        {
            interactionHintUI.SetActive(show);
            
            // if (show && currentInteractable != null)
            // {
            //     Text hintText = interactionHintUI.GetComponentInChildren<Text>();
            //     if (hintText != null)
            //     {
            //         hintText.text = currentInteractable.GetInteractionText();
            //     }
            // }
        }
    }

    public void SetInteractionEnabled(bool enabled)
    {
        canInteract = enabled;
        if (!enabled)
        {
            UpdateInteractionHint(false);
            currentInteractable = null;
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (InteractorSource != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(InteractorSource.position, InteractorSource.forward * InteractRange);
        }
    }
}