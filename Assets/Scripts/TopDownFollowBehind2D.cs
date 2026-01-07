using System.Collections.Generic;
using UnityEngine;

public class TopDownFollowBehind2D : MonoBehaviour
{
    [Header("Target / Leader")]
    public Transform leader;                 // Player, or the previous follower in the chain

    [Header("Follow Spacing")]
    [Tooltip("How far behind the leader this follower should try to stay (world units).")]
    public float followDistance = 1.2f;

    [Tooltip("Extra spacing so followers don't overlap each other.")]
    public float minSeparation = 0.6f;

    [Header("Smoothing / Movement")]
    [Tooltip("Higher = snappier. Lower = floatier.")]
    public float followSmooth = 10f;

    [Tooltip("Max move speed cap (optional). Set to 0 for no cap.")]
    public float maxSpeed = 0f;

    [Header("History Sampling")]
    [Tooltip("How often we sample the leader position (seconds). Smaller = more accurate, more memory.")]
    public float sampleInterval = 0.02f;

    [Tooltip("How many samples to keep. Must be large enough for your max followDistance.")]
    public int maxSamples = 300;

    private readonly List<Vector3> history = new List<Vector3>();
    private float sampleTimer;

    void Reset()
    {
        followDistance = 1.2f;
        minSeparation = 0.6f;
        followSmooth = 10f;
        sampleInterval = 0.02f;
        maxSamples = 300;
    }

    void LateUpdate()
    {
        if (!leader) return;

        // Sample the leader position into history
        sampleTimer += Time.deltaTime;
        if (sampleTimer >= sampleInterval)
        {
            sampleTimer = 0f;
            history.Insert(0, leader.position);

            if (history.Count > maxSamples)
                history.RemoveAt(history.Count - 1);
        }

        // If we don't have enough history yet, do nothing
        if (history.Count < 2) return;

        // Find a point in history that is approximately followDistance behind leader
        Vector3 desired = GetPointAlongHistory(followDistance);

        // Enforce a minimum separation (prevents "on top" stacking when stopping/turning)
        Vector3 toDesired = desired - transform.position;
        float distToDesired = toDesired.magnitude;

        if (distToDesired < minSeparation && distToDesired > 0.0001f)
        {
            // Push back away from desired point slightly to keep space
            desired = transform.position - toDesired.normalized * (minSeparation - distToDesired);
        }

        // Smoothly move toward desired
        Vector3 next = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-followSmooth * Time.deltaTime));

        if (maxSpeed > 0f)
        {
            Vector3 delta = next - transform.position;
            float maxDelta = maxSpeed * Time.deltaTime;
            if (delta.magnitude > maxDelta)
                next = transform.position + delta.normalized * maxDelta;
        }

        transform.position = next;
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

        // Not enough history; return the oldest point
        return history[history.Count - 1];
    }
}
