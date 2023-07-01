using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RallyData {
    public Vector3 destination;
    public GameObject destinationObject;
}

public class UnitRally : IState
{
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;
    private RallyData _rallyData;
    private Vector3 _cachedRallyPoint = Vector3.zero;

    public UnitRally(NavMeshAgent navMeshAgent, Rigidbody rb, RallyData rallyData)
    {
        _navMeshAgent = navMeshAgent;
        _rb = rb;
        _rallyData = rallyData;
    }

    public void Tick(){
        if (_rallyData.destination != null && _rallyData.destinationObject != null && _navMeshAgent.isOnNavMesh &&
            (Vector3.Distance(_rallyData.destinationObject.transform.position + _rallyData.destination, _cachedRallyPoint) > 0.1f))
            // TODO: squared distance as optimization
        {    
            _navMeshAgent.SetDestination(_rallyData.destinationObject.transform.position + _rallyData.destination);
            _cachedRallyPoint = _rallyData.destinationObject.transform.position + _rallyData.destination;
        }
    }

    public void OnEnter()
    {
        _navMeshAgent.enabled = true;
        _rb.isKinematic = true;
        if (_rallyData.destinationObject != null && _navMeshAgent.isOnNavMesh)
        {
            Debug.Log(_navMeshAgent.SetDestination(_rallyData.destinationObject.transform.position + _rallyData.destination));
            _cachedRallyPoint = _rallyData.destinationObject.transform.position + _rallyData.destination;
        }
        else if (_rallyData.destination != null && _navMeshAgent.isOnNavMesh)
        {
            _navMeshAgent.SetDestination(_rallyData.destination);
        } 

    }

    public void OnExit(){}

    public void OnCollisionEnter(Collision collision){}

}
