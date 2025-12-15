using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class TopDownPlayerController : MonoBehaviour
{
    [Header("refs")]
    public Transform gfxRoot;              // assign: a child transform that holds sprite + eye + visuals
    public SpriteRenderer spriteRenderer;  // optional (auto-find under gfxRoot)
    public Transform eye;                 // assign (child under gfxRoot)
    public Camera mainCamera;             // usually leave null -> auto Camera.main

    [Header("move")]
    public float moveSpeed = 6.0f;
    public float sprintMultiplier = 1.6f;
    public float acceleration = 35.0f;
    public float deceleration = 45.0f;

    [Header("mirror")]
    public float mirrorDeadzone = 0.05f;  // avoid flicker when barely moving

    [Header("eye")]
    public float eyeTurnSpeed = 720f;     // degrees/sec

    Rigidbody2D rb;
    Vector2 input;
    Vector2 targetVel;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!mainCamera) mainCamera = Camera.main;

        if (!gfxRoot) gfxRoot = transform; // fallback, but you should assign a dedicated child
        if (!spriteRenderer) spriteRenderer = gfxRoot.GetComponentInChildren<SpriteRenderer>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Update()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        input = Vector2.ClampMagnitude(input, 1f);

        bool sprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float speed = sprinting ? moveSpeed * sprintMultiplier : moveSpeed;

        // flip ALL visuals by scaling gfxRoot on X (recommended rig)
        if (gfxRoot)
        {
            Vector3 s = gfxRoot.localScale;

            if (input.x > mirrorDeadzone) s.x = -Mathf.Abs(s.x);       // kept your earlier flip direction
            else if (input.x < -mirrorDeadzone) s.x = Mathf.Abs(s.x);

            gfxRoot.localScale = s;
        }

        targetVel = input * speed;

        UpdateEyeRotation();
    }

    void FixedUpdate()
    {
        float rate = (input.sqrMagnitude > 0.0001f) ? acceleration : deceleration;
        rb.velocity = Vector2.MoveTowards(rb.velocity, targetVel, rate * Time.fixedDeltaTime);
    }

    void UpdateEyeRotation()
    {
        if (!eye || !mainCamera) return;

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (Vector2)(mouseWorld - eye.position);

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion targetRot = Quaternion.Euler(0f, 0f, angle);

        eye.rotation = Quaternion.RotateTowards(
            eye.rotation,
            targetRot,
            eyeTurnSpeed * Time.deltaTime
        );
    }
}
