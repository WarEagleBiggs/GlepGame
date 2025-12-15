using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("target")]
    public Transform target;

    [Header("follow feel")]
    public float smoothTime = 0.14f;     // lower = snappier, higher = floatier
    public float maxSpeed = 50f;

    [Header("lead")]
    public float leadDistance = 0.6f;    // how far ahead camera biases
    public float leadResponsiveness = 10f;

    Vector3 velocity;
    Vector2 smoothedLead;
    Vector3 lastTargetPos;

    void Start()
    {
        if (target) lastTargetPos = target.position;
    }

    void LateUpdate()
    {
        if (!target) return;

        // estimate target velocity (frame-based, good enough for top-down feel)
        Vector3 currentPos = target.position;
        Vector3 targetVel = (currentPos - lastTargetPos) / Mathf.Max(Time.deltaTime, 0.0001f);
        lastTargetPos = currentPos;

        // compute lead in direction of movement (top-down x/y only)
        Vector2 v2 = new Vector2(targetVel.x, targetVel.y);
        Vector2 desiredLead = (v2.sqrMagnitude > 0.001f) ? v2.normalized * leadDistance : Vector2.zero;
        smoothedLead = Vector2.Lerp(smoothedLead, desiredLead, 1f - Mathf.Exp(-leadResponsiveness * Time.deltaTime));

        Vector3 desired = new Vector3(
            target.position.x + smoothedLead.x,
            target.position.y + smoothedLead.y,
            transform.position.z
        );

        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime, maxSpeed);
    }
}
