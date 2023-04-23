using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RallyVectors {
    public Vector3 rallyDestination;
    public Vector3 nextDestination;
}

public class UnitRally : IState
{
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;
    private RallyVectors _rallyVectors;

    public UnitRally(NavMeshAgent navMeshAgent, Rigidbody rb, RallyVectors rallyVectors)
    {
        _navMeshAgent = navMeshAgent;
        _rb = rb;
        _rallyVectors = rallyVectors;
    }

    public void Tick(){
        // React to rally destination change
        if (_rallyVectors.rallyDestination != _rallyVectors.nextDestination)
        {
            _rallyVectors.rallyDestination = _rallyVectors.nextDestination;
            _navMeshAgent.SetDestination(_rallyVectors.rallyDestination);
        }
    }

    public void OnEnter()
    {
        _navMeshAgent.enabled = true;
        _rb.isKinematic = true;
        _rallyVectors.rallyDestination = _rallyVectors.nextDestination;
        if (_rallyVectors.rallyDestination != null && _navMeshAgent.isOnNavMesh){
            _navMeshAgent.SetDestination(_rallyVectors.rallyDestination);
        }
    }

    public void OnExit(){}

    public void OnCollisionEnter(Collision collision){}

}
