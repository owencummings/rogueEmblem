using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitAttackApproach : IState
{
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;
    private AttackData _attackData;

    //private float _attackRange = 2f;
    private const float _recalculateTime = 0.25f;
    private float _timer = 0f;


    public UnitAttackApproach(NavMeshAgent navMeshAgent, Rigidbody rb, AttackData attackData)
    {
        _navMeshAgent = navMeshAgent;
        _rb = rb;
        _attackData = attackData;
    }

    public void Tick()
    {
        _timer += Time.deltaTime;
        if (_timer > _recalculateTime)
        {
            if (_attackData.attackTarget != null)
            {
                _navMeshAgent.SetDestination(_attackData.attackTarget.transform.position);
            }
            _timer = 0f;
        }
    }

    public void OnEnter()
    {
        _navMeshAgent.enabled = true;
        _rb.isKinematic = true;
        _attackData.attackTarget = _attackData.nextAttackTarget; 
        _navMeshAgent.SetDestination(_attackData.attackTarget.transform.position);
    }

    public void OnExit(){}
    public void OnCollisionEnter(Collision collision){}
}
