using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
public class AgentController : MonoBehaviour
{
    public IslandController owner;

    private Rigidbody _body;
    private NavMeshAgent _navAgent;

    public float Mass;

    [SerializeField] private float _teeteringTime;

    private TiltController _tiltController;

    void Awake()
    {
        _body = GetComponent<Rigidbody>();
        _navAgent = GetComponent<NavMeshAgent>();
        _navAgent.SetDestination(Vector3.zero);
    }

    void FixedUpdate()
    {
        if (owner != null)
        {
            var vector = owner.COMPivotOffset;
            vector.y = 0;
            _body.AddForce(vector);
        }

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

    internal void SetDestination(Tile tile)
    {
        _navAgent.SetDestination(tile.transform.position);
    }
}
