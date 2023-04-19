using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitAttackApproach : IState
{
    private Unit _unit;
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;

    private GameObject _attackTarget;
    //private float _attackRange = 2f;
    private const float _recalculateTime = 0.25f;
    private float _timer = 0f;


    public UnitAttackApproach(Unit unit, NavMeshAgent navMeshAgent, Rigidbody rb)
    {
        _unit = unit;
        _navMeshAgent = navMeshAgent;
        _rb = rb;
    }

    public void Tick()
    {
        _timer += Time.deltaTime;
        if (_timer > _recalculateTime)
        {
            if (_attackTarget != null)
            {
                _navMeshAgent.SetDestination(_attackTarget.transform.position);
            }
            _timer = 0f;
        }
    }

    public void OnEnter()
    {
        _navMeshAgent.enabled = true;
        _rb.isKinematic = true;
        _unit.attackTarget = _unit.nextAttackTarget; 
        _attackTarget = _unit.attackTarget;
        _navMeshAgent.SetDestination(_attackTarget.transform.position);
    }

    public void OnExit(){}
}
