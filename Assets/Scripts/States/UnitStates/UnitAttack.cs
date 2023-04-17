using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitAttack : IState
{
    private Unit _unit;
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;
    private GameObject _attackTarget;

    public UnitAttack(Unit unit, NavMeshAgent navMeshAgent, Rigidbody rb)
    {
        _unit = unit;
        _navMeshAgent = navMeshAgent;
        _rb = rb;
    }

    public void Tick()
    {
    }

    public void OnEnter(){
        Debug.Log("yo");
        _navMeshAgent.enabled = false;
        _rb.isKinematic = false;
        _rb.AddForce(Vector3.up * 100 + Vector3.forward * 300);
    }
    public void OnExit(){}
}
