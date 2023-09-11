using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class UnitFindNavMesh : IState
{
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;
    private Vector3 entryPosition;

    public UnitFindNavMesh(NavMeshAgent navMeshAgent, Rigidbody rb)
    {
        _navMeshAgent = navMeshAgent;
        _rb = rb;
    }

    public void Tick(){}

    public void OnEnter()
    {
        entryPosition = _rb.position;
        _navMeshAgent.enabled = true;
        _rb.isKinematic = true;
        _navMeshAgent.ResetPath();
    }

    public void OnExit(){}

    public void OnCollisionEnter(Collision collision){}

}
