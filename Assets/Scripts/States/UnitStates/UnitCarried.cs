using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitCarried : IState
{
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;
    private ICarryable _carryable;

    public UnitCarried(NavMeshAgent navMeshAgent, Rigidbody rb, ICarryable carryable)
    {
        _navMeshAgent = navMeshAgent;
        _rb = rb;
        _carryable = carryable;
    }

    public void OnEnter()
    {
        _navMeshAgent.enabled = true;
        _rb.isKinematic = true;
    }

    public void Tick(){
        _navMeshAgent.baseOffset = Mathf.Lerp(_navMeshAgent.baseOffset, _carryable.CarryBase + _carryable.CarryOffset, 0.03f);
        _carryable.CurrCarryOffset = _navMeshAgent.baseOffset - _carryable.CarryBase;
    }
    public void OnExit(){
        _navMeshAgent.baseOffset = _carryable.CarryBase; // May eventually need to pass along a non-carried offset param as well
        _carryable?.ForceExit();
    }
    public void OnCollisionEnter(Collision collision){}
}
