using UnityEngine;
using System.Collections.Generic;

public class PressurePlate : MonoBehaviour
{
    public SpriteRenderer buttonVisual;
    public Color pressedColor = Color.green;

    public List<SpriteRenderer> poweredLines;
    public Color poweredColor = Color.red;

    public bool isDoor;

    public Transform doorLeft;
    public Transform doorRight;

    public float doorLeftOpenLocalX;
    public float doorRightOpenLocalX;

    public float doorMoveSpeed = 6f;

    Color buttonStartColor;
    Color[] lineStartColors;

    float doorLeftClosedLocalX;
    float doorRightClosedLocalX;

    bool pressed;

    MaterialPropertyBlock mpb;
    string colorProp;

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

        mpb = new MaterialPropertyBlock();

        var m = buttonVisual ? buttonVisual.sharedMaterial : null;
        if (m != null && m.HasProperty("_RendererColor")) colorProp = "_RendererColor";
        else if (m != null && m.HasProperty("_BaseColor")) colorProp = "_BaseColor";
        else colorProp = "_Color";
    }

    void Update()
    {
        if (!isDoor) return;

        if (doorLeft)
        {
            float targetX = pressed ? doorLeftOpenLocalX : doorLeftClosedLocalX;
            Vector3 p = doorLeft.localPosition;
            p.x = Mathf.MoveTowards(p.x, targetX, doorMoveSpeed * Time.deltaTime);
            doorLeft.localPosition = p;
        }

        if (doorRight)
        {
            float targetX = pressed ? doorRightOpenLocalX : doorRightClosedLocalX;
            Vector3 p = doorRight.localPosition;
            p.x = Mathf.MoveTowards(p.x, targetX, doorMoveSpeed * Time.deltaTime);
            doorRight.localPosition = p;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        pressed = true;

        Debug.Log("pressed do action");

        SetSpriteColor(buttonVisual, pressedColor);

        for (int i = 0; i < poweredLines.Count; i++)
            SetSpriteColor(poweredLines[i], poweredColor);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        pressed = false;

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
}
