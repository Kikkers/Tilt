using System;
using System.Collections;
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
        //NavMeshHit hit;
        //_navAgent.FindClosestEdge(out hit);
        //if (hit.distance != 0)
        //    _teeteringTime = 0;
        //else
        //{
        //    _teeteringTime += Time.fixedDeltaTime;
        //}
    }

    internal void SetDestination(Tile tile)
    {
        if (_navAgent.enabled)
            _navAgent.SetDestination(tile.transform.position);
    }

    internal void DoImpact(float impactMin, float impactMult)
    {
        _navAgent.enabled = false;
        var deltaCenter = owner.Pivot.position - transform.position;
        deltaCenter.y = 0;
        _body.AddForce(Vector3.up * (deltaCenter.magnitude * impactMult + impactMin));
        StartCoroutine(AgentEnabler());
    }

    private IEnumerator AgentEnabler()
    {
        int num = 0;
        do
        {
            yield return new WaitForFixedUpdate();
            num++;
        } while (transform.localPosition.y > -0.49f);
        _navAgent.enabled = true;
    }

}
