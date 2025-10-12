using UnityEngine;

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

    void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _playerCamera = GetComponentInChildren<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        _characterController.Move(_velocity * Time.deltaTime);
        _direction = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        if (_characterController.isGrounded) _velocity.y = Input.GetKeyDown(KeyCode.Space) ? _jumpForce : -0.1f;
        else _velocity.y += _gravity * Time.deltaTime;

            mouseDelta *= _rotateSpeed * Time.deltaTime;
        _rotation.y += mouseDelta.x;
        _rotation.x = Mathf.Clamp(_rotation.x - mouseDelta.y, -90, 90);
        _playerCamera.transform.localEulerAngles = _rotation;
    }

    void FixedUpdate()
    {
        _direction *= Input.GetKey(KeyCode.LeftShift) ? _runSpeed : _walkSpeed;
        Vector3 move = Quaternion.Euler(0, _playerCamera.transform.eulerAngles.y, 0) * new Vector3(_direction.x, 0, _direction.y);
        _velocity = new Vector3(move.x, _velocity.y, move.z);
    }

}


