using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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

    [Header("Сбор алмазов")]
    [SerializeField] private DiamondCounter _diamondCounter;
    [SerializeField] private AudioClip _diamondPickupClip;
    [SerializeField] [Range(0f, 1f)] private float _diamondPickupVolume = 0.7f;

    [Header("UI")]
    [SerializeField] private GameUI _gameUI;

    [Header("Звуки шагов")]
    [SerializeField] private AudioClip[] _footstepClips;
    [SerializeField] private float _walkStepInterval = 0.5f;
    [SerializeField] private float _runStepInterval = 0.35f;
    [SerializeField] [Range(0f, 1f)] private float _footstepVolume = 0.5f;

    private CharacterController _controller;
    private PlayerControls _controls;
    private Vector3 _velocity;
    private float _pitch;
    private AudioSource _audioSource;
    private float _stepTimer;
    private int _lastClipIndex = -1;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _controls = new PlayerControls();
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;

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
        if (Time.timeScale == 0f) return;

        HandleLook();
        HandleMovement();
        HandleFootsteps();
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

    private void HandleFootsteps()
    {
        if (_footstepClips == null || _footstepClips.Length == 0) return;

        Vector2 input = _controls.Player.Move.ReadValue<Vector2>();
        bool isMoving = input.sqrMagnitude > 0.01f;
        bool isGrounded = _controller.isGrounded;

        if (!isMoving || !isGrounded)
        {
            _stepTimer = 0f;
            return;
        }

        bool isRunning = _controls.Player.Sprint.IsPressed();
        float interval = isRunning ? _runStepInterval : _walkStepInterval;

        _stepTimer += Time.deltaTime;
        if (_stepTimer >= interval)
        {
            _stepTimer = 0f;
            PlayFootstep();
        }
    }

    private void PlayFootstep()
    {
        int index;
        do
        {
            index = Random.Range(0, _footstepClips.Length);
        } while (index == _lastClipIndex && _footstepClips.Length > 1);

        _lastClipIndex = index;
        _audioSource.PlayOneShot(_footstepClips[index], _footstepVolume);
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Diamond"))
        {
            _diamondCounter.Increment();

            if (_diamondPickupClip != null)
                _audioSource.PlayOneShot(_diamondPickupClip, _diamondPickupVolume);

            other.gameObject.SetActive(false);
        }
    }

    public void Die()
    {
        if (_gameUI != null)
            _gameUI.ShowDeath();
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}