using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitRigidIdle : IState
{
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;
    public Vector3 entryForce = Vector3.zero;
    public float timeIdling = 0f;
    public float timeToIdle = 0.1f;

    public UnitRigidIdle(NavMeshAgent navMeshAgent, Rigidbody rb)
    {
        _navMeshAgent = navMeshAgent;
        _rb = rb;
    }

    public void OnEnter()
    {
        _rb.isKinematic = false;
        _navMeshAgent.enabled = false;
        _rb.AddForce(entryForce);
    }

    public void Tick(){
        Debug.Log(_rb.velocity);
        timeIdling += Time.deltaTime;
    }

    public void OnExit(){
        timeIdling = 0f;
    }
    public void OnCollisionEnter(Collision collision){}
}
