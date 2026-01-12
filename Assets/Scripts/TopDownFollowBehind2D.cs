using System.Collections.Generic;
using UnityEngine;

public class TopDownFollowBehind2D : MonoBehaviour
{
    [Header("Target / Leader")]
    public Transform leader;

    [Header("Follow Spacing")]
    public float followDistance = 1.2f;
    public float minSeparation = 0.6f;

    [Header("Smoothing / Movement")]
    public float followSmooth = 10f;
    public float maxSpeed = 0f;

    [Header("History Sampling")]
    public float sampleInterval = 0.02f;
    public int maxSamples = 300;

    [Header("Flip")]
    public SpriteRenderer sprite;
    public float flipDeadzone = 0.03f;
    public float flipHoldSeconds = 0.08f;

    private readonly List<Vector3> history = new List<Vector3>();
    private float sampleTimer;

    private Vector3 lastPos;
    private float moveDirX;
    private float lastMoveTime;

    void Reset()
    {
        followDistance = 1.2f;
        minSeparation = 0.6f;
        followSmooth = 10f;
        sampleInterval = 0.02f;
        maxSamples = 300;
        flipDeadzone = 0.03f;
        flipHoldSeconds = 0.08f;
    }

    void Start()
    {
        lastPos = transform.position;
    }

    void LateUpdate()
    {
        if (!leader) return;

        sampleTimer += Time.deltaTime;
        if (sampleTimer >= sampleInterval)
        {
            sampleTimer = 0f;
            history.Insert(0, leader.position);

            if (history.Count > maxSamples)
                history.RemoveAt(history.Count - 1);
        }

        if (history.Count < 2) return;

        Vector3 desired = GetPointAlongHistory(followDistance);

        Vector3 toDesired = desired - transform.position;
        float distToDesired = toDesired.magnitude;

        if (distToDesired < minSeparation && distToDesired > 0.0001f)
            desired = transform.position - toDesired.normalized * (minSeparation - distToDesired);

        Vector3 next = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-followSmooth * Time.deltaTime));

        if (maxSpeed > 0f)
        {
            Vector3 delta = next - transform.position;
            float maxDelta = maxSpeed * Time.deltaTime;
            if (delta.magnitude > maxDelta)
                next = transform.position + delta.normalized * maxDelta;
        }

        transform.position = next;

        UpdateFlip(next);
    }

    private void UpdateFlip(Vector3 newPos)
    {
        if (!sprite) return;

        float dx = newPos.x - lastPos.x;

        if (dx <= -flipDeadzone)
        {
            moveDirX = -1f;
            lastMoveTime = Time.time;
        }
        else if (dx >= flipDeadzone)
        {
            moveDirX = 1f;
            lastMoveTime = Time.time;
        }

        if (moveDirX != 0f && (Time.time - lastMoveTime) <= flipHoldSeconds)
            sprite.flipX = (moveDirX < 0f);
        else if (moveDirX != 0f)
            sprite.flipX = (moveDirX < 0f);

        lastPos = newPos;
    }

    private Vector3 GetPointAlongHistory(float distanceBack)
    {
        float traveled = 0f;

        for (int i = 0; i < history.Count - 1; i++)
        {
            Vector3 a = history[i];
            Vector3 b = history[i + 1];
            float seg = Vector3.Distance(a, b);

            if (traveled + seg >= distanceBack)
            {
                float t = (distanceBack - traveled) / Mathf.Max(seg, 0.0001f);
                return Vector3.Lerp(a, b, t);
            }

            traveled += seg;
        }

        return history[history.Count - 1];
    }
}
