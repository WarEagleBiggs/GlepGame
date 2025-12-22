using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public Transform target;
    public float repathRate = 0.1f;

    [Tooltip("Keeps the agent on this Z plane (top-down 2D usually uses Z=0).")]
    public float lockedZ = 0f;

    NavMeshAgent agent;
    float timer;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        // Critical for 2D navmesh usage
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        // Usually best for constant chasing
        agent.autoBraking = false;
    }

    void Update()
    {
        // Hard lock to the 2D plane
        Vector3 p = transform.position;
        if (p.z != lockedZ)
        {
            p.z = lockedZ;
            transform.position = p;
        }

        if (!target) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Vector3 dest = target.position;
            dest.z = lockedZ;
            agent.SetDestination(dest);

            timer = repathRate;
        }
    }
}
