using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMove : MonoBehaviour
{
    public Transform gfxRoot;
    public SpriteRenderer spriteRenderer;
    [FormerlySerializedAs("animator")] public Animator bodyAnimator;
    public Animator headAnimator;

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

    public List<GameObject> heads;
    public bool eyeVisibility;
    public SpriteRenderer eyeRend;

    public GameObject eyeRenderGO;
    public string spitStateName = "Spit";
    public int headAnimatorLayer = 0;

    public float health = 100f;
    public float damagePerHit = 10f;
    public Transform healthBarFill;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!mainCamera) mainCamera = Camera.main;
        if (!gfxRoot) gfxRoot = transform;
        if (!spriteRenderer) spriteRenderer = gfxRoot.GetComponentInChildren<SpriteRenderer>();
        if (!bodyAnimator && spriteRenderer) bodyAnimator = spriteRenderer.GetComponent<Animator>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Update()
    {
        UpdateEyeRenderVisibility();
        UpdateHealthBar();

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

        if (bodyAnimator)
        {
            bool isWalking = input.sqrMagnitude > 0.01f;
            bodyAnimator.SetBool("isWalking", isWalking);
            bodyAnimator.speed = isWalking ? (isSprinting ? sprintAnimSpeed : 1f) : 1f;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("laser"))
            TakeDamage(damagePerHit);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("blib"))
            TakeDamage(damagePerHit);
    }

    void TakeDamage(float amount)
    {
        health = Mathf.Clamp(health - amount, 0f, 100f);
        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        if (!healthBarFill) return;

        Vector3 s = healthBarFill.localScale;
        s.y = Mathf.Clamp01(health / 100f);
        healthBarFill.localScale = s;
    }

    void UpdateEyeRenderVisibility()
    {
        bool spitPlaying = false;

        if (headAnimator)
        {
            var state = headAnimator.GetCurrentAnimatorStateInfo(headAnimatorLayer);
            var next = headAnimator.GetNextAnimatorStateInfo(headAnimatorLayer);

            spitPlaying =
                state.IsName(spitStateName) ||
                (headAnimator.IsInTransition(headAnimatorLayer) && next.IsName(spitStateName));
        }

        if (eyeRenderGO) eyeRenderGO.SetActive(!spitPlaying);
        if (eyeRend) eyeRend.enabled = !spitPlaying;
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
    public void TakeLaserDamage(float amount)
    {
        health = Mathf.Clamp(health - amount, 0f, 100f);
        UpdateHealthBar();
    }


    void Fire()
    {
        if (headAnimator) headAnimator.Play(spitStateName, headAnimatorLayer, 0f);

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
