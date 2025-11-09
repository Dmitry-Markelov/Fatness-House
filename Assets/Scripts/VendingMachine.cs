using UnityEngine;

public class VendingMachine : MonoBehaviour, IInteractable
{
    [Header("UI References")]
    [SerializeField] private GameObject vendingMachineUI;
    
    [Header("Player References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private RayCast rayCast;
    
    [Header("Settings")]
    [SerializeField] private string interactionText = "Press E to use Vending Machine";
    
    private bool isUIOpen = false;
    
    void Start()
    {
        if (playerMovement == null)
        {
            playerMovement = FindAnyObjectByType<PlayerMovement>();
        }
        
        if (rayCast == null)
        {
            rayCast = FindAnyObjectByType<RayCast>();
        }
        
        if (vendingMachineUI != null)
        {
            vendingMachineUI.SetActive(false);
        }
    }

    public void Interact()
    {
        if (!isUIOpen)
        {
            OpenVendingMachine();
        }
        else
        {
            CloseVendingMachine();
        }
    }
   
    public string GetInteractionText()
    {
        return interactionText;
    }
    
    private void OpenVendingMachine()
    {
        isUIOpen = true;
  
        if (vendingMachineUI != null)
        {
            vendingMachineUI.SetActive(true);
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }
   
        if (rayCast != null)
        {
            rayCast.SetInteractionEnabled(false);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Debug.Log("Vending Machine UI Opened");
    }
    
    private void CloseVendingMachine()
    {
        isUIOpen = false;
 
        if (vendingMachineUI != null)
        {
            vendingMachineUI.SetActive(false);
        }
  
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
 
        if (rayCast != null)
        {
            rayCast.SetInteractionEnabled(true);
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        Debug.Log("Vending Machine UI Closed");
    }
    
    public void CloseFromUI()
    {
        CloseVendingMachine();
    }
   
    void Update()
    {
        if (isUIOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseVendingMachine();
        }
    }
}