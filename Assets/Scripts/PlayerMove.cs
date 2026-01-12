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

    public float moveSpeed = 6f;
    public float sprintMultiplier = 1.6f;
    public float acceleration = 35f;

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

    Vector2 lastMoveDir = Vector2.right;

    public GameObject eyeRenderGO;
    public SpriteRenderer eyeRend;

    public string spitStateName = "Spit";
    public string ipadStateName = "iPad";
    public string danceStateName = "Dance";
    public string throwChildStateName = "ThrowChild";
    public int headAnimatorLayer = 0;

    [Header("Health")]
    public float health = 100f;
    public float damagePerHit = 10f;
    public Transform healthBarFill;

    [Header("Spit Meter")]
    public float spitMeter = 100f;
    public float spitCost = 10f;
    public float spitRegenPerSecond = 12f;
    public Transform spitBarFill;

    [Header("Toolbar")]
    public ToolBar toolBar;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!mainCamera) mainCamera = Camera.main;
        if (!gfxRoot) gfxRoot = transform;

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Update()
    {
        UpdateHealthBar();
        UpdateSpitBar();
        UpdateEyeRenderVisibility();

        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        input = Vector2.ClampMagnitude(input, 1f);

        if (input.sqrMagnitude > 0.001f)
            lastMoveDir = input.normalized;

        isSprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float speed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;

        if (gfxRoot)
        {
            Vector3 s = gfxRoot.localScale;
            if (lastMoveDir.x > mirrorDeadzone) s.x = -Mathf.Abs(s.x);
            else if (lastMoveDir.x < -mirrorDeadzone) s.x = Mathf.Abs(s.x);
            gfxRoot.localScale = s;
        }

        targetVel = input * speed;

        UpdateEyeRotation();

        spitMeter = Mathf.Clamp(spitMeter + spitRegenPerSecond * Time.deltaTime, 0f, 100f);
        fireTimer -= Time.deltaTime;

        if (toolBar && Input.GetMouseButton(0))
        {
            int action = toolBar.CurrSlot;

            if (action == 1)
            {
                if (fireTimer <= 0f && spitMeter >= spitCost)
                {
                    FireSpitMouseAimed();
                    spitMeter -= spitCost;
                    fireTimer = fireCooldown;
                }
            }
            else if (action == 2 && headAnimator)
                headAnimator.Play(ipadStateName, headAnimatorLayer, 0f);
            else if (action == 3 && headAnimator)
                headAnimator.Play(danceStateName, headAnimatorLayer, 0f);
            else if (action == 4 && headAnimator)
                headAnimator.Play(throwChildStateName, headAnimatorLayer, 0f);
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

    void FireSpitMouseAimed()
    {
        if (headAnimator)
            headAnimator.Play(spitStateName, headAnimatorLayer, 0f);

        if (!bulletPrefab || !mainCamera) return;

        Vector3 spawnPos = firePoint ? firePoint.position : transform.position;

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = spawnPos.z;

        Vector2 dir = (Vector2)(mouseWorld - spawnPos);
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;
        dir.Normalize();

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.Euler(0f, 0f, angle);

        GameObject go = Instantiate(bulletPrefab, spawnPos, rot);

        Rigidbody2D brb = go.GetComponent<Rigidbody2D>();
        if (brb)
        {
            brb.gravityScale = 0f;
            brb.velocity = dir * bulletSpeed;
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
        Vector2 eyeLocal = eye.localPosition;

        Vector2 dir = mouseLocal - eyeLocal;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        eye.localRotation = Quaternion.RotateTowards(
            eye.localRotation,
            Quaternion.Euler(0f, 0f, angle),
            eyeTurnSpeed * Time.deltaTime
        );
    }

    void UpdateEyeRenderVisibility()
    {
        if (!headAnimator) return;

        var state = headAnimator.GetCurrentAnimatorStateInfo(headAnimatorLayer);
        bool spitPlaying = state.IsName(spitStateName);

        if (eyeRenderGO) eyeRenderGO.SetActive(!spitPlaying);
        if (eyeRend) eyeRend.enabled = !spitPlaying;
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

    public void TakeDamage(float amount)
    {
        health = Mathf.Clamp(health - amount, 0f, 100f);
        UpdateHealthBar();
    }

    public void TakeLaserDamage(float amount)
    {
        TakeDamage(amount);
    }

    void UpdateHealthBar()
    {
        if (!healthBarFill) return;
        Vector3 s = healthBarFill.localScale;
        s.y = Mathf.Clamp01(health / 100f);
        healthBarFill.localScale = s;
    }

    void UpdateSpitBar()
    {
        if (!spitBarFill) return;
        Vector3 s = spitBarFill.localScale;
        s.y = Mathf.Clamp01(spitMeter / 100f);
        spitBarFill.localScale = s;
    }
}
