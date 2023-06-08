using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitFloat : IState
{
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;
    private Transform _tf;

    public UnitFloat(NavMeshAgent navMeshAgent, Rigidbody rb, Transform tf)
    {
        _navMeshAgent = navMeshAgent;
        _rb = rb;
        _tf = tf;
    }

    public void Tick()
    {
    }

    public void OnEnter()
    {
        _navMeshAgent.enabled = false;
        _rb.isKinematic = true;
    }

    public void OnExit(){
    }

    public void OnCollisionEnter(Collision collision){}
}
