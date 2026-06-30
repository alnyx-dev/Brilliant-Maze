using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Движение")]
    [SerializeField] private float _walkSpeed = 4f;
    [SerializeField] private float _runSpeed = 8f;

    [Header("Прыжок и гравитация")]
    [SerializeField] private float _jumpHeight = 1.2f;
    [SerializeField] private float _gravity = -9.81f;

    [Header("Камера / Обзор")]
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private float _mouseSensitivity = 0.1f;
    [SerializeField] private float _minPitch = -85f;
    [SerializeField] private float _maxPitch = 85f;

    private CharacterController _controller;
    private PlayerControls _controls;
    private Vector3 _velocity;
    private float _pitch;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _controls = new PlayerControls();

        if (_playerCamera == null)
            _playerCamera = Camera.main;
    }

    private void OnEnable() => _controls.Enable();
    private void OnDisable() => _controls.Disable();

    private void Start()
    {
        SetCursorLocked(true);
    }

    private void Update()
    {
        HandleLook();
        HandleMovement();
        HandleCursorToggle();
    }

    private void HandleLook()
    {
        Vector2 delta = _controls.Player.Look.ReadValue<Vector2>();

        float yaw = delta.x * _mouseSensitivity;
        float deltaPitch = delta.y * _mouseSensitivity;

        transform.Rotate(Vector3.up * yaw);

        _pitch = Mathf.Clamp(_pitch - deltaPitch, _minPitch, _maxPitch);
        _playerCamera.transform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    private void HandleMovement()
    {
        bool isGrounded = _controller.isGrounded;
        if (isGrounded && _velocity.y < 0f)
            _velocity.y = -2f;

        Vector2 input = _controls.Player.Move.ReadValue<Vector2>();
        Vector3 move = transform.right * input.x + transform.forward * input.y;
        move = Vector3.ClampMagnitude(move, 1f);

        float speed = _controls.Player.Sprint.IsPressed() ? _runSpeed : _walkSpeed;
        _controller.Move(move * speed * Time.deltaTime);

        if (isGrounded && _controls.Player.Jump.WasPressedThisFrame())
            _velocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);

        _velocity.y += _gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }

    private void HandleCursorToggle()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            SetCursorLocked(false);
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
            SetCursorLocked(true);
    }

    private void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}