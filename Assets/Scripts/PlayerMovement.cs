using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _rotateSpeed = 75;
    [SerializeField] private float _runSpeed = 7;
    [SerializeField] private float _walkSpeed = 5;
    [SerializeField] private float _jumpForce = 5;
    [SerializeField] private float _gravity = -9.81f;

    private CharacterController _characterController;
    private Camera _playerCamera;
    private Vector2 _rotation;
    private Vector2 _direction;
    private Vector3 _velocity;

    public Image StaminaBar;
    public float Stamina, MaxStamina;
    public float EventCost;
    public float RunCost;
    public float StaminaRegenRate = 10f;

    private bool _isRunning;
    private bool _canRun = true;

    void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _playerCamera = GetComponentInChildren<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
 
        Stamina = MaxStamina;
        UpdateStaminaBar();
    }

    void Update()
    {
        if (!enabled) return;
        
        _characterController.Move(_velocity * Time.deltaTime);
        _direction = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        if (_characterController.isGrounded) 
            _velocity.y = Input.GetKeyDown(KeyCode.Space) ? _jumpForce : -0.1f;
        else 
            _velocity.y += _gravity * Time.deltaTime;

        mouseDelta *= _rotateSpeed * Time.deltaTime;
        _rotation.y += mouseDelta.x;
        _rotation.x = Mathf.Clamp(_rotation.x - mouseDelta.y, -90, 90);
        _playerCamera.transform.localEulerAngles = _rotation;

        HandleRunning();
        RegenerateStamina();

        if (Input.GetKeyDown("f"))
        {
            Debug.Log("Действие выполнено");
            Stamina -= EventCost;
            if(Stamina < 0) Stamina = 0;
            UpdateStaminaBar();
        }
    }
    

    void FixedUpdate()
    {
        if (!enabled) return;

        float currentSpeed = _isRunning && _canRun ? _runSpeed : _walkSpeed;
        _direction *= currentSpeed;
        
        Vector3 move = Quaternion.Euler(0, _playerCamera.transform.eulerAngles.y, 0) * new Vector3(_direction.x, 0, _direction.y);
        _velocity = new Vector3(move.x, _velocity.y, move.z);
    }

    private void HandleRunning()
    {
        bool wantsToRun = Input.GetKey(KeyCode.LeftShift) && _direction.magnitude > 0.1f;
        
        if (wantsToRun && _canRun && Stamina > 0)
        {
            _isRunning = true;
            Stamina -= RunCost * Time.deltaTime;

            if (Stamina <= 0)
            {
                Stamina = 0;
                _canRun = false;
                _isRunning = false;
            }
            
            UpdateStaminaBar();
        }
        else
        {
            _isRunning = false;

            if (!_canRun && Stamina >= MaxStamina * 0.3f)
            {
                _canRun = true;
            }
        }
    }

    private void RegenerateStamina()
    {
        if (!_isRunning && Stamina < MaxStamina)
        {
            Stamina += StaminaRegenRate * Time.deltaTime;
            if (Stamina > MaxStamina)
                Stamina = MaxStamina;
            
            UpdateStaminaBar();
        }
    }

    private void UpdateStaminaBar()
    {
        if (StaminaBar != null)
            StaminaBar.fillAmount = Stamina / MaxStamina;
    }

    public void RestoreStamina(float amount)
    {
        Stamina = Mathf.Min(Stamina + (MaxStamina * amount / 100f), MaxStamina);
        UpdateStaminaBar();
        Debug.Log($"Восстановлено {amount}% стамины. Текущая: {Stamina}");
    }

    
}