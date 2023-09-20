using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitIngest : IState
{
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;
    private Transform _transform;
    public float ingestProgress = 0f;
    public float ingestDuration = 1f;

    public UnitIngest(NavMeshAgent navMeshAgent, Rigidbody rb, Transform transform)
    {
        _navMeshAgent = navMeshAgent;
        _transform = transform;
        _rb = rb;
    }

    public void Tick()
    {
        ingestProgress += Time.deltaTime;
    }

    public void OnEnter()
    {
        ingestProgress = 0f;
        _navMeshAgent.enabled = false;
        _rb.isKinematic = false;
        _rb.AddForce((_transform.up * 1000f) * _transform.localScale.magnitude);
    }

    public void OnExit(){}

    public void OnCollisionEnter(Collision collision){}
}

