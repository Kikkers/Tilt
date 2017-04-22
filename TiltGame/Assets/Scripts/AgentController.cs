using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Rigidbody))]
public class AgentController : MonoBehaviour
{
    public IslandController owner;

    private Rigidbody _body;
    private NavMeshAgent _navAgent;

    public float Mass;

    [SerializeField] private float _teeteringTime;

    void Awake()
    {
        _body = GetComponent<Rigidbody>();
        _navAgent = GetComponent<NavMeshAgent>();
        _navAgent.SetDestination(Vector3.zero);
    }

    void FixedUpdate()
    {
        _body.AddForce(new Vector3(1, 1, 1));

        // determine teetering
        NavMeshHit hit;
        _navAgent.FindClosestEdge(out hit);
        if (hit.distance != 0)
            _teeteringTime = 0;
        else
        {
            _teeteringTime += Time.fixedDeltaTime;
        }
    }
}
