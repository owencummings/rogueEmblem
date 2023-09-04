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
    public Vector3 _cachedRallyPoint = Vector3.zero;
    private Vector3 direction;

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
            // Isn't doing these checks just as bad as updating the value every tick?
        {    
            _cachedRallyPoint = _rallyData.destinationObject.transform.position + _rallyData.destination;
        }
        direction = (_cachedRallyPoint - _transform.position);
        direction.y = _rb.transform.position.y;
        Vector3 torque = Vector3.Cross(direction.normalized, _rb.transform.forward);
        _rb.AddTorque(_transform.up * Vector3.Dot(direction.normalized, _rb.transform.right) * Time.deltaTime * 50f * _transform.localScale.magnitude);
        _rb.AddForce((_transform.up * 10 + _transform.forward * 400) * _transform.localScale.magnitude * 1.5f * Time.deltaTime);
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
