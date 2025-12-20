using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class StickyBullet2D : MonoBehaviour
{
    public float destroyDelay = 0.6f;

    Rigidbody2D rb;
    Collider2D col;
    bool hit;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hit) return;
        hit = true;

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        if (col) col.enabled = false;

        transform.SetParent(collision.transform, true);

        StartCoroutine(Die());
    }

    IEnumerator Die()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }
}