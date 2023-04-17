using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitAttackApproach : IState
{
    private Unit _unit;
    private NavMeshAgent _navMeshAgent;
    private GameObject _attackTarget;

    public UnitAttackApproach(Unit unit, NavMeshAgent navMeshAgent)
    {
        _unit = unit;
        _navMeshAgent = navMeshAgent;
    }

    public void Tick()
    {
        if (_attackTarget != null)
        {
            _navMeshAgent.SetDestination(_attackTarget.transform.position); // Is it bad to set this every tick?
        }
    }

    public void OnEnter(){
        _navMeshAgent.enabled = true;
    }
    public void OnExit(){}
}
