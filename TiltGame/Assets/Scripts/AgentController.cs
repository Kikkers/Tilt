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
    private Vector3 _lastPos;

    public float Stamina = 1;
    public float MaxSpeed = 1;
    public float handicap;

    [SerializeField] private float _mass;
    public float Mass { get { return _navAgent.enabled ? _mass : 0; } }
    
    private TiltController _tiltController;

    void Awake()
    {
        _body = GetComponent<Rigidbody>();
        _navAgent = GetComponent<NavMeshAgent>();
        _navAgent.SetDestination(Vector3.zero);
    }

    void FixedUpdate()
    {
        var vector = Vector3.zero;
        if (owner != null)
        {
            vector = owner.COMPivotOffset;
            vector.y = 0;
            _body.AddForce(vector);
        }

        if (_navAgent.enabled)
        {   
            var pos = transform.position;
            var delta = pos - _lastPos;
            float downwards = Vector3.Dot(vector.normalized, _navAgent.velocity.normalized);
            if (float.IsNaN(downwards)) downwards = 0;
            float energyLost = Mathf.Max(-downwards * delta.magnitude * 0.05f, 0);
            Stamina -= energyLost;

            downwards = Mathf.Max(0, downwards);
            handicap = Mathf.Clamp(Stamina + downwards, 0, 1);
            _navAgent.speed = 0.01f + MaxSpeed * handicap;
            //_navAgent.velocity = _navAgent.velocity * handicap;
        }
        _lastPos = transform.position;

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
