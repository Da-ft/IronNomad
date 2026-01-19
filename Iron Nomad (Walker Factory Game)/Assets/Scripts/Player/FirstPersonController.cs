using UnityEngine;
using IronNomad.Inputs;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private Transform _cameraRoot; // Das Empty Object auf Augenhöhe

    [Header("Movement (Shooter Feel)")]
    [SerializeField] private float _walkSpeed = 8f;
    [SerializeField] private float _sprintSpeed = 12f;
    [SerializeField] private float _jumpForce = 7f;
    [SerializeField] private float _airControl = 0.5f; // Multiplikator für Luft-Bewegung

    [Header("Look Settings")]
    [SerializeField] private float _mouseSensitivity = 0.1f; // Achtung: Input System liefert oft hohe Werte
    [SerializeField] private float _maxLookAngle = 89f;

    [Header("Physics")]
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _groundDrag = 6f; // Hoher Drag = Schnelles Stoppen
    [SerializeField] private float _airDrag = 0f;    // Kein Drag in der Luft

    private Rigidbody _rb;
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private bool _jumpTriggered;

    // State
    private bool _isGrounded;
    private Rigidbody _groundRigidbody; // Der Walker
    private Quaternion _lastGroundRotation;
    private float _cameraPitch = 0f; // Vertikale Rotation

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        _rb.useGravity = true;

        // Maus fangen und verstecken
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        _inputReader.MoveEvent += OnMove;
        _inputReader.LookEvent += OnLook; // Neues Event für Maus
        _inputReader.JumpEvent += OnJump;
    }

    private void OnDisable()
    {
        _inputReader.MoveEvent -= OnMove;
        _inputReader.LookEvent -= OnLook;
        _inputReader.JumpEvent -= OnJump;
    }

    private void OnMove(Vector2 input) => _moveInput = input;
    private void OnLook(Vector2 input) => _lookInput = input;
    private void OnJump() => _jumpTriggered = true;

    private void Update()
    {
        // Kamera-Rotation machen wir im Update für maximale Smoothness (High Framerate)
        HandleLook();
    }

    private void FixedUpdate()
    {
        CheckGround();

        if (_isGrounded)
        {
            HandleGroundedMovement();
            HandlePlatformRotation();
            _rb.linearDamping = _groundDrag; // Unity 6: linearDamping (früher drag)
        }
        else
        {
            HandleAirMovement();
            _rb.linearDamping = _airDrag;
        }

        _jumpTriggered = false;
    }

    private void HandleLook()
    {
        // 1. Vertikal (Pitch) - Dreht nur die Kamera (CameraRoot)
        _cameraPitch -= _lookInput.y * _mouseSensitivity;
        _cameraPitch = Mathf.Clamp(_cameraPitch, -_maxLookAngle, _maxLookAngle);
        _cameraRoot.localRotation = Quaternion.Euler(_cameraPitch, 0, 0);

        // 2. Horizontal (Yaw) - Dreht den ganzen Körper (Rigidbody)
        // Wir machen das hier visuell, aber syncen es gleich im FixedUpdate für Physik
        float yawDelta = _lookInput.x * _mouseSensitivity;
        Quaternion bodyRotation = Quaternion.Euler(0, yawDelta, 0);

        // WICHTIG: Rigidbody Rotation nur im FixedUpdate?
        // Für FPS ist es besser, die Rotation sofort anzuwenden, sonst fühlt sich die Maus "laggy" an.
        // Rigidbody.MoveRotation ist okay, wenn wir interpolation nutzen.
        _rb.MoveRotation(_rb.rotation * bodyRotation);
    }

    private void CheckGround()
    {
        // Raycast etwas höher starten, um nicht im Boden zu stecken
        Vector3 start = transform.position + Vector3.up * 0.1f;
        // Distanz angepasst für Kapsel
        if (Physics.SphereCast(start, 0.3f, Vector3.down, out RaycastHit hit, 1.05f, _groundLayer))
        {
            if (!_isGrounded) _lastGroundRotation = hit.collider.attachedRigidbody.rotation;

            _isGrounded = true;
            _groundRigidbody = hit.collider.attachedRigidbody;
        }
        else
        {
            _isGrounded = false;
            _groundRigidbody = null;
        }
    }

    private void HandleGroundedMovement()
    {
        // Richtung ist jetzt immer relativ zum Spieler (FPS Standard)
        Vector3 moveDir = transform.right * _moveInput.x + transform.forward * _moveInput.y;
        moveDir.Normalize();

        Vector3 targetVelocity = moveDir * _walkSpeed;

        // Walker Velocity addieren
        if (_groundRigidbody != null)
        {
            targetVelocity += _groundRigidbody.GetPointVelocity(transform.position);
        }

        // Snappy Movement: Wir setzen die Velocity direkt (außer Y)
        if (_jumpTriggered)
        {
            // Momentum behalten + Sprung
            _rb.linearVelocity = new Vector3(targetVelocity.x, _jumpForce, targetVelocity.z);
        }
        else
        {
            // Am Boden kleben
            _rb.linearVelocity = new Vector3(targetVelocity.x, _rb.linearVelocity.y, targetVelocity.z);
        }
    }

    private void HandlePlatformRotation()
    {
        if (_groundRigidbody == null) return;

        Quaternion currentGroundRotation = _groundRigidbody.rotation;
        Quaternion deltaRotation = currentGroundRotation * Quaternion.Inverse(_lastGroundRotation);

        // Wir addieren die Plattform-Drehung zur aktuellen Drehung (die wir schon durch die Maus haben)
        _rb.MoveRotation(deltaRotation * _rb.rotation);

        _lastGroundRotation = currentGroundRotation;
    }

    private void HandleAirMovement()
    {
        // In der Luft geben wir nur kleine Impulse
        Vector3 moveDir = transform.right * _moveInput.x + transform.forward * _moveInput.y;
        moveDir.Normalize();

        if (moveDir.magnitude > 0.1f)
        {
            _rb.AddForce(moveDir * _walkSpeed * _airControl, ForceMode.Acceleration);
        }
    }
}