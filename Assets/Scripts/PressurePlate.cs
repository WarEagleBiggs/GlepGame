using UnityEngine;
using System.Collections.Generic;

public class PressurePlate : MonoBehaviour
{
    public SpriteRenderer buttonVisual;
    public Color pressedColor = Color.green;

    public List<SpriteRenderer> poweredLines;
    public Color poweredColor = Color.red;

    public bool isDoor;
    public bool isPlatform;

    public Transform doorLeft;
    public Transform doorRight;

    public float doorLeftOpenLocalX;
    public float doorRightOpenLocalX;

    public Transform platform;
    public Collider2D platformTrigger;

    public float platformUpLocalY;
    public float moveSpeed = 6f;
    public float platformPauseTop = 0.4f;
    public float platformPauseBottom = 0.4f;

    public GameObject heightCollider;

    Color buttonStartColor;
    Color[] lineStartColors;

    float doorLeftClosedLocalX;
    float doorRightClosedLocalX;

    Vector3 platformStartLocalPos;
    Vector3 platformLastWorldPos;
    bool platformGoingUp = true;
    float platformPauseTimer;

    bool pressed;

    MaterialPropertyBlock mpb;
    string colorProp;

    PlatformRiderTracker riderTracker;

    public AudioSource ButtonSFX;
    public AudioSource ButtonSFX2;
    public GameObject ButtonHum;

    void Awake()
    {
        if (!buttonVisual) buttonVisual = GetComponentInChildren<SpriteRenderer>();
        buttonStartColor = buttonVisual ? buttonVisual.color : Color.white;

        lineStartColors = new Color[poweredLines.Count];
        for (int i = 0; i < poweredLines.Count; i++)
            if (poweredLines[i]) lineStartColors[i] = poweredLines[i].color;

        if (isDoor)
        {
            if (doorLeft) doorLeftClosedLocalX = doorLeft.localPosition.x;
            if (doorRight) doorRightClosedLocalX = doorRight.localPosition.x;
        }

        if (isPlatform && platform)
        {
            platformStartLocalPos = platform.localPosition;
            platformLastWorldPos = platform.position;
            platformPauseTimer = 0f;
            platformGoingUp = true;

            if (!platformTrigger) platformTrigger = platform.GetComponent<Collider2D>();

            if (platformTrigger)
            {
                riderTracker = platformTrigger.GetComponent<PlatformRiderTracker>();
                if (!riderTracker) riderTracker = platformTrigger.gameObject.AddComponent<PlatformRiderTracker>();
            }
        }

        mpb = new MaterialPropertyBlock();

        var m = buttonVisual ? buttonVisual.sharedMaterial : null;
        if (m != null && m.HasProperty("_RendererColor")) colorProp = "_RendererColor";
        else if (m != null && m.HasProperty("_BaseColor")) colorProp = "_BaseColor";
        else colorProp = "_Color";
    }

    void Update()
    {
        if (isDoor)
        {
            if (doorLeft)
            {
                float targetX = pressed ? doorLeftOpenLocalX : doorLeftClosedLocalX;
                Vector3 p = doorLeft.localPosition;
                p.x = Mathf.MoveTowards(p.x, targetX, moveSpeed * Time.deltaTime);
                doorLeft.localPosition = p;
            }

            if (doorRight)
            {
                float targetX = pressed ? doorRightOpenLocalX : doorRightClosedLocalX;
                Vector3 p = doorRight.localPosition;
                p.x = Mathf.MoveTowards(p.x, targetX, moveSpeed * Time.deltaTime);
                doorRight.localPosition = p;
            }
        }

        if (isPlatform && heightCollider)
            heightCollider.SetActive(!pressed);
    }

    void FixedUpdate()
    {
        if (!isPlatform || !platform) return;

        Vector3 targetLocal = platform.localPosition;

        if (!pressed)
        {
            platformGoingUp = true;
            platformPauseTimer = 0f;

            targetLocal.y = Mathf.MoveTowards(platform.localPosition.y, platformStartLocalPos.y, moveSpeed * Time.fixedDeltaTime);
        }
        else
        {
            if (platformPauseTimer > 0f)
            {
                platformPauseTimer -= Time.fixedDeltaTime;
                if (platformPauseTimer < 0f) platformPauseTimer = 0f;
            }
            else
            {
                float goalY = platformGoingUp ? platformUpLocalY : platformStartLocalPos.y;
                targetLocal.y = Mathf.MoveTowards(platform.localPosition.y, goalY, moveSpeed * Time.fixedDeltaTime);

                if (Mathf.Abs(targetLocal.y - goalY) <= 0.001f)
                {
                    if (platformGoingUp)
                    {
                        platformGoingUp = false;
                        platformPauseTimer = platformPauseTop;
                    }
                    else
                    {
                        platformGoingUp = true;
                        platformPauseTimer = platformPauseBottom;
                    }
                }
            }
        }

        platform.localPosition = targetLocal;

        Vector2 delta = (Vector2)(platform.position - platformLastWorldPos);
        if (delta.sqrMagnitude > 0f && riderTracker != null)
            riderTracker.MoveRiders(delta);

        platformLastWorldPos = platform.position;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        pressed = true;
        
        ButtonSFX.Play();
        ButtonHum.SetActive(true);

        Debug.Log("pressed do action");

        SetSpriteColor(buttonVisual, pressedColor);

        for (int i = 0; i < poweredLines.Count; i++)
            SetSpriteColor(poweredLines[i], poweredColor);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        ButtonSFX2.Play();
        pressed = false;
        ButtonHum.SetActive(false);

        SetSpriteColor(buttonVisual, buttonStartColor);

        for (int i = 0; i < poweredLines.Count; i++)
            SetSpriteColor(poweredLines[i], lineStartColors[i]);
    }

    void SetSpriteColor(SpriteRenderer sr, Color c)
    {
        if (!sr) return;

        sr.color = c;
        sr.GetPropertyBlock(mpb);
        mpb.SetColor(colorProp, c);
        sr.SetPropertyBlock(mpb);
    }

    class PlatformRiderTracker : MonoBehaviour
    {
        readonly HashSet<Rigidbody2D> riders = new HashSet<Rigidbody2D>();

        void OnTriggerEnter2D(Collider2D other)
        {
            var rb = other.attachedRigidbody;
            if (rb) riders.Add(rb);
        }

        void OnTriggerExit2D(Collider2D other)
        {
            var rb = other.attachedRigidbody;
            if (rb) riders.Remove(rb);
        }

        public void MoveRiders(Vector2 delta)
        {
            foreach (var rb in riders)
            {
                if (!rb) continue;
                rb.MovePosition(rb.position + delta);
            }
        }
    }
}
