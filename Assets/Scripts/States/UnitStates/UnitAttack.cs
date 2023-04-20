using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AttackData{
    public GameObject attackTarget;
    public GameObject nextAttackTarget;
    public bool attackFinished;
}

public class UnitAttack : IState
{
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;
    private Transform _transform;
    private AttackData _attackData;
    private float attackCooldown = 0.25f;
    private float attackTime = 0f;

    public UnitAttack(NavMeshAgent navMeshAgent, Rigidbody rb, Transform transform, AttackData attackData)
    {
        _attackData = attackData;
        _navMeshAgent = navMeshAgent;
        _transform = transform;
        _rb = rb;
    }

    public void Tick()
    {
        attackTime += Time.deltaTime;
        if (attackTime > attackCooldown){
            _attackData.attackFinished = true;
        }
    }

    public void OnEnter(){
        _navMeshAgent.enabled = false;
        _transform.LookAt(_attackData.attackTarget.transform);
        _rb.isKinematic = false;
        _rb.AddForce(_transform.up * 200 + _transform.forward * 200);
        attackTime = 0f;
        _attackData.attackFinished = false;
    }
    public void OnExit(){
        _attackData.attackTarget = null;
        _attackData.nextAttackTarget = null;
        _attackData.attackFinished = false;
    }
}
