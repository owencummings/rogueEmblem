using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitLurk : IState
{
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;
    private Transform _tf;

    public UnitLurk(NavMeshAgent navMeshAgent,  Rigidbody rb, Transform tf)
    {
        _navMeshAgent = navMeshAgent;
        _rb = rb;
        _tf = tf;
    }

    public void Tick(){
 
    }

    public void OnEnter()
    {
        _navMeshAgent.enabled = false; 
        _rb.isKinematic = true;
        _rb.detectCollisions = false;
    }

    public void OnExit(){ _rb.detectCollisions = true; }

    public void OnCollisionEnter(Collision collision){}

}

