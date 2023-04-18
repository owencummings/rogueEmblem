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
    private float attackCooldown = 0.25f;
    private float attackTime = 0f;

    public UnitAttack(Unit unit, NavMeshAgent navMeshAgent, Rigidbody rb)
    {
        _unit = unit;
        _navMeshAgent = navMeshAgent;
        _rb = rb;
    }

    public void Tick()
    {
        attackTime += Time.deltaTime;
        if (attackTime > attackCooldown){
            _unit.attackFinished = true;
        }
    }

    public void OnEnter(){
        _navMeshAgent.enabled = false;
        _unit.transform.LookAt(_unit.attackTarget.transform);
        _rb.isKinematic = false;
        _rb.AddForce(_unit.transform.up * 200 + _unit.transform.forward * 200);
        attackTime = 0f;
        _unit.attackFinished = false;
    }
    public void OnExit(){
        _unit.attackTarget = null;
        _unit.nextAttackTarget = null;
        _unit.attackFinished = false;
    }
}
