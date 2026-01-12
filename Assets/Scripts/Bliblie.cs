using UnityEngine;
using UnityEngine.AI;

public class Bliblie : MonoBehaviour
{
    public Transform target;
    public float chaseRange = 10f;

    NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    void Update()
    {
        if (!target) return;

        if (Vector2.Distance(transform.position, target.position) <= chaseRange)
            agent.SetDestination(target.position);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("spit"))
            Destroy(gameObject);
    }
}