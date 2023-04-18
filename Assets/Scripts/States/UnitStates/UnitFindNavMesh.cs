using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class UnitFindNavMesh : IState
{
    private Unit _unit;
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;

    public UnitFindNavMesh(Unit unit, NavMeshAgent navMeshAgent, Rigidbody rb)
    {
        _unit = unit;
        _navMeshAgent = navMeshAgent;
        _rb = rb;
    }

    public void Tick(){}

    public void OnEnter()
    {
        _navMeshAgent.enabled = true;
        _rb.isKinematic = true;
    }

    public void OnExit(){}
}
