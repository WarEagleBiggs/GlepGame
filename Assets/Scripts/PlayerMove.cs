using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMove : MonoBehaviour
{
    public Transform gfxRoot;
    public SpriteRenderer spriteRenderer;
    public Animator animator;

    public Transform eye;
    public Camera mainCamera;

    public float moveSpeed = 6.0f;
    public float sprintMultiplier = 1.6f;
    public float acceleration = 35.0f;

    public float mirrorDeadzone = 0.05f;
    public float eyeTurnSpeed = 720f;

    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 14f;
    public float fireCooldown = 0.12f;

    public float sprintAnimSpeed = 1.5f;

    Rigidbody2D rb;
    Vector2 input;
    Vector2 targetVel;
    float fireTimer;
    bool isSprinting;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!mainCamera) mainCamera = Camera.main;
        if (!gfxRoot) gfxRoot = transform;
        if (!spriteRenderer) spriteRenderer = gfxRoot.GetComponentInChildren<SpriteRenderer>();
        if (!animator && spriteRenderer) animator = spriteRenderer.GetComponent<Animator>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Update()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        input = Vector2.ClampMagnitude(input, 1f);

        isSprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float speed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;

        if (gfxRoot)
        {
            Vector3 s = gfxRoot.localScale;
            if (input.x > mirrorDeadzone) s.x = -Mathf.Abs(s.x);
            else if (input.x < -mirrorDeadzone) s.x = Mathf.Abs(s.x);
            gfxRoot.localScale = s;
        }

        targetVel = input * speed;

        UpdateEyeRotation();

        fireTimer -= Time.deltaTime;
        if (Input.GetMouseButton(0) && fireTimer <= 0f)
        {
            Fire();
            fireTimer = fireCooldown;
        }
    }

    void FixedUpdate()
    {
        if (input.sqrMagnitude < 0.0001f)
            rb.velocity = Vector2.zero;
        else
            rb.velocity = Vector2.MoveTowards(rb.velocity, targetVel, acceleration * Time.fixedDeltaTime);

        if (animator)
        {
            bool isWalking = input.sqrMagnitude > 0.01f;
            animator.SetBool("isWalking", isWalking);
            animator.speed = isWalking ? (isSprinting ? sprintAnimSpeed : 1f) : 1f;
        }
    }

    void UpdateEyeRotation()
    {
        if (!eye || !mainCamera || !gfxRoot) return;

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3 mouseLocal3 = gfxRoot.InverseTransformPoint(
            new Vector3(mouseWorld.x, mouseWorld.y, gfxRoot.position.z)
        );

        Vector2 mouseLocal = new Vector2(mouseLocal3.x, mouseLocal3.y);
        Vector2 eyeLocal = new Vector2(eye.localPosition.x, eye.localPosition.y);

        Vector2 dir = mouseLocal - eyeLocal;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        eye.localRotation = Quaternion.RotateTowards(
            eye.localRotation,
            Quaternion.Euler(0f, 0f, angle),
            eyeTurnSpeed * Time.deltaTime
        );
    }

    void Fire()
    {
        if (!bulletPrefab || !mainCamera) return;

        Vector3 spawnPos = firePoint ? firePoint.position : transform.position;

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = ((Vector2)mouseWorld - (Vector2)spawnPos).normalized;
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.Euler(0f, 0f, angle);

        GameObject go = Instantiate(bulletPrefab, spawnPos, rot);

        Rigidbody2D brb = go.GetComponent<Rigidbody2D>();
        if (brb)
        {
            brb.gravityScale = 0f;
            brb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            brb.interpolation = RigidbodyInterpolation2D.Interpolate;
            brb.velocity = dir * bulletSpeed;
        }
    }
}
