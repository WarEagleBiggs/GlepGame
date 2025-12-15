using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class TopDownPlayerController : MonoBehaviour
{
    public Transform gfxRoot;
    public SpriteRenderer spriteRenderer;
    public Transform eye;
    public Camera mainCamera;

    public float moveSpeed = 6.0f;
    public float sprintMultiplier = 1.6f;
    public float acceleration = 35.0f;
    public float deceleration = 45.0f;

    public float mirrorDeadzone = 0.05f;

    public float eyeTurnSpeed = 720f;

    Rigidbody2D rb;
    Vector2 input;
    Vector2 targetVel;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!mainCamera) mainCamera = Camera.main;
        if (!gfxRoot) gfxRoot = transform;
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

        if (gfxRoot)
        {
            Vector3 s = gfxRoot.localScale;
            if (input.x > mirrorDeadzone) s.x = -Mathf.Abs(s.x);
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
        if (!eye || !mainCamera || !gfxRoot) return;

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3 mouseLocal = gfxRoot.InverseTransformPoint(new Vector3(mouseWorld.x, mouseWorld.y, gfxRoot.position.z));
        Vector3 eyeLocal = eye.localPosition;
        Vector2 dir = mouseLocal - eyeLocal;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion targetRot = Quaternion.Euler(0f, 0f, angle);

        eye.localRotation = Quaternion.RotateTowards(
            eye.localRotation,
            targetRot,
            eyeTurnSpeed * Time.deltaTime
        );
    }
}
