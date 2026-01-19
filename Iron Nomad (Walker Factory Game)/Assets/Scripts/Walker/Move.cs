using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Move : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private float _turnSpeed = 5f; // Langsame Drehung zum Testen

    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        // Kinematic ist hier perfekt: Wir steuern es per Code, Physik wirkt aber auf Child-Objekte
        _rb.isKinematic = true;
        _rb.interpolation = RigidbodyInterpolation.Interpolate; // WICHTIG gegen Ruckeln
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        // 1. Position berechnen
        Vector3 moveOffset = transform.forward * _moveSpeed * dt;
        _rb.MovePosition(_rb.position + moveOffset);

        // 2. Rotation berechnen (optional, um GetPointVelocity zu testen)
        Quaternion turnOffset = Quaternion.Euler(0, _turnSpeed * dt, 0);
        _rb.MoveRotation(_rb.rotation * turnOffset);
    }
}