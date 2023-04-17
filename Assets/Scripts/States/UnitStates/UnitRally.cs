using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitRally : IState
{
    private Unit _unit;
    private NavMeshAgent _navMeshAgent;

    public UnitRally(Unit unit, NavMeshAgent navMeshAgent)
    {
        _unit = unit;
        _navMeshAgent = navMeshAgent;
    }

    public void Tick(){
        // React to rally destination change
        if (_unit.rallyDestination != _unit.nextDestination)
        {
            _unit.rallyDestination = _unit.nextDestination;
            _navMeshAgent.SetDestination(_unit.rallyDestination);
        }
    }

    public void OnEnter()
    {
        _navMeshAgent.enabled = true;
        _unit.rallyDestination = _unit.nextDestination;
        if (_unit.rallyDestination != null){
            _navMeshAgent.SetDestination(_unit.rallyDestination);
        }
    }

    public void OnExit(){}
}