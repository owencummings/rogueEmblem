using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;


public class UnitIdle : IState
{
    private Unit _unit;
    private NavMeshAgent _navMeshAgent;

    public UnitIdle(Unit unit, NavMeshAgent navMeshAgent)
    {
        _unit = unit;
        _navMeshAgent = navMeshAgent;
    }

    public void Tick(){}
    public void OnEnter(){}
    public void OnExit(){}
}
