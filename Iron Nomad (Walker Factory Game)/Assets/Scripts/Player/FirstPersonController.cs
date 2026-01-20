using UnityEngine;
using IronNomad.Inputs;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private Transform _cameraRoot;

    [Header("Player Movement")]
    public float MoveSpeed = 4.0f;
    public float SprintSpeed = 6.0f;
    public float SpeedChangeRate = 10.0f;

    [Header("Camera Control")]
    public float RotationSpeed = 1.0f;
    public float TopClamp = 90.0f;
    public float BottomClamp = -90.0f;

    [Header("Jump & Gravity")]
    public float JumpHeight = 1.2f;
    public float Gravity = -15.0f;
    public float JumpTimeout = 0.1f;
    public float FallTimeout = 0.15f;

    [Header("Ground Check")]
    public bool Grounded = true;
    public float GroundedOffset = -0.14f;
    public float GroundedRadius = 0.5f;
    public LayerMask GroundLayers;

    // Player State
    private float _speed;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;
    private float _cameraPitch;

    // Timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    // Cache Input
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private bool _isSprinting;
    private bool _jumpTriggered;

    private CharacterController _controller;
    private GameObject _mainCamera;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();

        // Maus locken
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        if (_inputReader == null) return;

        _inputReader.MoveEvent += v => _moveInput = v;
        _inputReader.LookEvent += v => _lookInput = v;
        _inputReader.SprintEvent += v => _isSprinting = v;
        _inputReader.JumpEvent += () => _jumpTriggered = true;
    }

    private void OnDisable()
    {
        // Inputs unsubscriben wäre hier sauberer
    }

    private void Update()
    {
        JumpAndGravity();
        GroundedCheck();
        Move();
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void GroundedCheck()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
    }

    private void CameraRotation()
    {
        if (_lookInput.sqrMagnitude >= 0.0001f)
        {
            _cameraPitch -= _lookInput.y * RotationSpeed;

            _cameraPitch = ClampAngle(_cameraPitch, BottomClamp, TopClamp);
            _cameraRoot.localRotation = Quaternion.Euler(_cameraPitch, 0.0f, 0.0f);

            float rotationVelocity = _lookInput.x * RotationSpeed;
            transform.Rotate(Vector3.up * rotationVelocity);
        }
    }

    private void Move()
    {
        float targetSpeed = _isSprinting ? SprintSpeed : MoveSpeed;
        if (_moveInput == Vector2.zero) targetSpeed = 0.0f;

        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
        float speedOffset = 0.1f;
        float inputMagnitude = _moveInput.magnitude;

        if (Mathf.Abs(currentHorizontalSpeed - targetSpeed) > speedOffset)
        {
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed * inputMagnitude;
        }

        Vector3 inputDirection = new Vector3(_moveInput.x, 0.0f, _moveInput.y).normalized;

        if (_moveInput != Vector2.zero)
        {
            inputDirection = transform.right * _moveInput.x + transform.forward * _moveInput.y;
        }

        Vector3 finalMovement = inputDirection.normalized * (_speed * Time.deltaTime) +
                                new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime;

        if (Grounded)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2f, GroundLayers))
            {
                if (hit.collider.attachedRigidbody != null)
                {
                    Vector3 platformVel = hit.collider.attachedRigidbody.GetPointVelocity(transform.position);

                    finalMovement += platformVel * Time.deltaTime;
                }
            }
        }

        _controller.Move(finalMovement);
    }

    private void JumpAndGravity()
    {
        if (Grounded)
        {
            _fallTimeoutDelta = FallTimeout;

            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            if (_jumpTriggered && _jumpTimeoutDelta <= 0.0f)
            {
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                _jumpTriggered = false;
            }

            if (_jumpTimeoutDelta >= 0.0f) _jumpTimeoutDelta -= Time.deltaTime;
        }
        else
        {
            _jumpTimeoutDelta = JumpTimeout;
            _jumpTriggered = false;

            if (_fallTimeoutDelta >= 0.0f) _fallTimeoutDelta -= Time.deltaTime;
        }

        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (Grounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
    }
}