using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserTurret2D : MonoBehaviour
{
    public enum Mode { Spin, SweepBetweenAngles }

    [Header("Mode")]
    public Mode mode = Mode.Spin;

    [Header("Rotation")]
    public float rotateSpeed = 90f;

    [Header("Sweep (degrees)")]
    public float angleA = -45f;
    public float angleB = 45f;

    [Header("Laser")]
    public Transform firePoint;
    public float maxDistance = 30f;
    public LayerMask hitMask = ~0;

    [Header("Damage")]
    public float damagePerTick = 10f;
    public float damageInterval = 0.25f;

    LineRenderer lr;

    float currentAngle;
    float targetAngle;
    float damageTimer;

    int ignoreLayer;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;

        if (!firePoint) firePoint = transform;

        ignoreLayer = gameObject.layer;

        currentAngle = NormalizeAngle(transform.eulerAngles.z);
        targetAngle = NormalizeAngle(angleB);
    }

    void Update()
    {
        UpdateRotation();
        UpdateLaserAndDamage();
    }

    void UpdateRotation()
    {
        if (mode == Mode.Spin)
        {
            transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
            return;
        }

        float a = NormalizeAngle(angleA);
        float b = NormalizeAngle(angleB);

        currentAngle = MoveTowardsAngle(currentAngle, targetAngle, rotateSpeed * Time.deltaTime);
        transform.localRotation = Quaternion.Euler(0f, 0f, currentAngle);

        if (Mathf.Abs(Mathf.DeltaAngle(currentAngle, targetAngle)) < 0.1f)
            targetAngle = (Mathf.Abs(Mathf.DeltaAngle(targetAngle, a)) < 0.1f) ? b : a;
    }

    void UpdateLaserAndDamage()
    {
        Vector3 origin = firePoint.position;
        Vector2 dir = firePoint.right.normalized;

        int mask = hitMask & ~(1 << ignoreLayer);

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, maxDistance, mask);

        Vector3 end = origin + (Vector3)(dir * maxDistance);
        if (hit.collider != null)
            end = hit.point;

        lr.SetPosition(0, origin);
        lr.SetPosition(1, end);

        damageTimer -= Time.deltaTime;
        if (damageTimer > 0f) return;

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            PlayerMove pm = hit.collider.GetComponent<PlayerMove>();
            if (pm) pm.TakeLaserDamage(damagePerTick);
        }

        damageTimer = damageInterval;
    }

    static float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle < 0f) angle += 360f;
        return angle;
    }

    static float MoveTowardsAngle(float current, float target, float maxDelta)
    {
        float delta = Mathf.DeltaAngle(current, target);
        if (Mathf.Abs(delta) <= maxDelta) return target;
        return NormalizeAngle(current + Mathf.Sign(delta) * maxDelta);
    }
}
