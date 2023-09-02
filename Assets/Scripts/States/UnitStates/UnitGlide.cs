using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitGlide : IState
{
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;
    private Transform _transform;
    private RallyData _rallyData;
    private Vector3 _cachedRallyPoint = Vector3.zero;

    public UnitGlide(NavMeshAgent navMeshAgent, Rigidbody rb, Transform transform, RallyData rallyData)
    {
        _navMeshAgent = navMeshAgent;
        _transform = transform;
        _rb = rb;
        _rallyData = rallyData;
    }

    public void Tick()
    {
        if (_rallyData.destination != null && _rallyData.destinationObject != null && _navMeshAgent.isOnNavMesh &&
            (Vector3.Distance(_rallyData.destinationObject.transform.position + _rallyData.destination, _cachedRallyPoint) > 0.1f))
            // TODO: squared distance as optimization
        {    
            _cachedRallyPoint = _rallyData.destinationObject.transform.position + _rallyData.destination;
        }
        _rb.AddForce((_transform.up * 10 + _transform.forward * 300) * _transform.localScale.magnitude * 1.5f * Time.deltaTime);
    }

    public void OnEnter()
    {
        _navMeshAgent.enabled = false;
        _rb.isKinematic = false;
        if (_rallyData.destinationObject != null)
        {
            _cachedRallyPoint = _rallyData.destinationObject.transform.position + _rallyData.destination;
        }
        else if (_rallyData.destination != null)
        {
            _cachedRallyPoint = _rallyData.destination;
        } 
    }

    public void OnExit(){}

    public void OnCollisionEnter(Collision collision){}
}
