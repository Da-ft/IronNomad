using UnityEngine;
using IronNomad.Inputs;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private Transform _cameraRoot;

    [Header("Movement")]
    public float MoveSpeed = 4.0f;
    public float SprintSpeed = 6.0f;
    public float SpeedChangeRate = 10.0f;

    [Header("Camera")]
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

    private float _speed;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;
    private float _cameraPitch;
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private bool _isSprinting;
    private bool _jumpTriggered;

    private CharacterController _controller;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
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
        if (_inputReader == null) return;
        _inputReader.MoveEvent -= v => _moveInput = v;
        _inputReader.LookEvent -= v => _lookInput = v;
        _inputReader.SprintEvent -= v => _isSprinting = v;
        _inputReader.JumpEvent -= () => _jumpTriggered = true;
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
        if (_lookInput.sqrMagnitude < 0.0001f) return;

        _cameraPitch -= _lookInput.y * RotationSpeed;
        _cameraPitch = ClampAngle(_cameraPitch, BottomClamp, TopClamp);
        _cameraRoot.localRotation = Quaternion.Euler(_cameraPitch, 0.0f, 0.0f);
        transform.Rotate(Vector3.up * (_lookInput.x * RotationSpeed));
    }

    private void Move()
    {
        float targetSpeed = _isSprinting ? SprintSpeed : MoveSpeed;
        if (_moveInput == Vector2.zero) targetSpeed = 0.0f;

        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
        float inputMagnitude = _moveInput.magnitude;

        if (Mathf.Abs(currentHorizontalSpeed - targetSpeed) > 0.1f)
        {
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed * inputMagnitude;
        }

        Vector3 inputDirection = _moveInput != Vector2.zero
            ? transform.right * _moveInput.x + transform.forward * _moveInput.y
            : Vector3.zero;

        Vector3 finalMovement = inputDirection.normalized * (_speed * Time.deltaTime) +
                                new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime;

        if (Grounded && Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2f, GroundLayers))
        {
            if (hit.collider.attachedRigidbody != null)
                finalMovement += hit.collider.attachedRigidbody.GetPointVelocity(transform.position) * Time.deltaTime;
        }

        _controller.Move(finalMovement);
    }

    private void JumpAndGravity()
    {
        if (Grounded)
        {
            _fallTimeoutDelta = FallTimeout;

            if (_verticalVelocity < 0.0f)
                _verticalVelocity = -2f;

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
            _verticalVelocity += Gravity * Time.deltaTime;
    }

    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f) angle += 360f;
        if (angle > 360f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Grounded ? new Color(0f, 1f, 0f, 0.35f) : new Color(1f, 0f, 0f, 0.35f);
        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
    }
}