using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMove : MonoBehaviour
{
    public Transform gfxRoot;
    public Animator bodyAnimator;

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

    public GameObject mainGuy;
    public GameObject ipadGuy;
    public GameObject danceGuy;
    bool inAltMode;

    public float health = 100f;
    public float damagePerHit = 10f;
    public Transform healthBarFill;

    public float healthRegenPerSecond = 4f;

    public float spitMeter = 100f;
    public float spitCost = 10f;
    public float spitRegenPerSecond = 12f;
    public Transform spitBarFill;

    public ToolBar toolBar;

    public Animator DeathAnim;
    public GameObject DeathGuy;

    public Volume globalVolume;

    public GameObject winScreen;
    public GameObject deathScreen;

    bool isDead;
    bool hasWon;

    Vignette vignette;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!mainCamera) mainCamera = Camera.main;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        ExitAltMode();

        if (DeathGuy) DeathGuy.SetActive(false);
        if (winScreen) winScreen.SetActive(false);
        if (deathScreen) deathScreen.SetActive(false);

        if (globalVolume)
            globalVolume.profile.TryGet(out vignette);

        isDead = false;
        hasWon = false;
    }

    void Update()
    {
        UpdateHealthBar();
        UpdateSpitBar();

        if (isDead || hasWon)
        {
            input = Vector2.zero;
            targetVel = Vector2.zero;
            return;
        }

        health = Mathf.Clamp(health + healthRegenPerSecond * Time.deltaTime, 0f, 100f);
        spitMeter = Mathf.Clamp(spitMeter + spitRegenPerSecond * Time.deltaTime, 0f, 100f);

        if (inAltMode)
        {
            if (AnyInputPressed())
                ExitAltMode();
            return;
        }

        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        input = Vector2.ClampMagnitude(input, 1f);

        UpdateFacing();

        isSprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float speed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;
        targetVel = input * speed;

        UpdateEyeRotation();

        fireTimer -= Time.deltaTime;

        if (toolBar && Input.GetMouseButtonDown(0))
        {
            int slot = toolBar.CurrSlot;

            if (slot == 1)
            {
                if (fireTimer <= 0f && spitMeter >= spitCost)
                {
                    FireSpitMouseAimed();
                    spitMeter -= spitCost;
                    fireTimer = fireCooldown;
                }
            }
            else if (slot == 2)
            {
                EnterAltMode(ipadGuy);
            }
            else if (slot == 3)
            {
                EnterAltMode(danceGuy);
            }
        }
    }

    void FixedUpdate()
    {
        if (isDead || hasWon || inAltMode)
        {
            rb.velocity = Vector2.zero;
            return;
        }

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

    void UpdateFacing()
    {
        if (!gfxRoot) return;

        if (input.x > mirrorDeadzone)
        {
            Vector3 s = gfxRoot.localScale;
            s.x = -Mathf.Abs(s.x);
            gfxRoot.localScale = s;
        }
        else if (input.x < -mirrorDeadzone)
        {
            Vector3 s = gfxRoot.localScale;
            s.x = Mathf.Abs(s.x);
            gfxRoot.localScale = s;
        }
    }

    void EnterAltMode(GameObject modeGuy)
    {
        if (isDead || hasWon || !modeGuy) return;

        inAltMode = true;

        if (mainGuy) mainGuy.SetActive(false);
        if (ipadGuy) ipadGuy.SetActive(false);
        if (danceGuy) danceGuy.SetActive(false);

        modeGuy.SetActive(true);
    }

    void ExitAltMode()
    {
        inAltMode = false;

        if (mainGuy) mainGuy.SetActive(true);
        if (ipadGuy) ipadGuy.SetActive(false);
        if (danceGuy) danceGuy.SetActive(false);
    }

    bool AnyInputPressed()
    {
        return Input.anyKeyDown ||
               Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.01f ||
               Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.01f ||
               Input.GetMouseButtonDown(0) ||
               Input.GetMouseButtonDown(1);
    }

    void FireSpitMouseAimed()
    {
        if (isDead || hasWon) return;
        if (!bulletPrefab || !mainCamera) return;

        Vector3 spawnPos = firePoint ? firePoint.position : transform.position;
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = spawnPos.z;

        Vector2 dir = mouseWorld - spawnPos;
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;
        dir.Normalize();

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        GameObject go = Instantiate(bulletPrefab, spawnPos, Quaternion.Euler(0f, 0f, angle));

        Rigidbody2D brb = go.GetComponent<Rigidbody2D>();
        if (brb)
        {
            brb.gravityScale = 0f;
            brb.velocity = dir * bulletSpeed;
        }
    }

    void UpdateEyeRotation()
    {
        if (isDead || hasWon || !eye || !mainCamera || inAltMode) return;

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3 dir = mouseWorld - eye.position;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        eye.rotation = Quaternion.RotateTowards(
            eye.rotation,
            Quaternion.Euler(0f, 0f, angle),
            eyeTurnSpeed * Time.deltaTime
        );
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead || hasWon) return;

        if (other.CompareTag("win"))
        {
            Win();
            return;
        }

        if (other.CompareTag("laser"))
            TakeDamage(damagePerHit);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead || hasWon) return;

        if (collision.gameObject.CompareTag("blib"))
            TakeDamage(damagePerHit);
    }

    public void TakeDamage(float amount)
    {
        if (isDead || hasWon) return;

        health = Mathf.Clamp(health - amount, 0f, 100f);
        UpdateHealthBar();

        if (health <= 0f)
            Die();
    }

    void Win()
    {
        hasWon = true;
        ExitAltMode();

        input = Vector2.zero;
        targetVel = Vector2.zero;
        rb.velocity = Vector2.zero;

        if (winScreen) winScreen.SetActive(true);
    }

    void Die()
    {
        isDead = true;
        ExitAltMode();

        input = Vector2.zero;
        targetVel = Vector2.zero;
        rb.velocity = Vector2.zero;

        if (mainGuy) mainGuy.SetActive(false);
        if (ipadGuy) ipadGuy.SetActive(false);
        if (danceGuy) danceGuy.SetActive(false);

        if (DeathGuy) DeathGuy.SetActive(true);
        if (DeathAnim) DeathAnim.Play("Death");

        if (vignette != null)
            vignette.intensity.value = 0.8f;

        if (deathScreen) deathScreen.SetActive(true);
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
