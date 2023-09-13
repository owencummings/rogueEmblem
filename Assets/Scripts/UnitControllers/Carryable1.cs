using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Carryable1 : NavBody, ICarryable
{
    // Start is called before the first frame update
    public int Carriers { get; set; }
    public int CarriersNeeded { get; set; }
    public float CarryRadius { get; set; }
    public float CarryBase { get; set; }
    public float CarryOffset { get; set; }
    public float CurrCarryOffset { get; set; }
    public Vector3[] CarryPivots { get; set; }
    public NavMeshAgent NavAgent { get; set; }

    new void Awake(){
        base.Awake();
        Carriers = 0;
        CarriersNeeded = 3;
        CarryRadius = 0.4f;
        CarryBase = 0.5f;
        CarryOffset = 0.3f;
        (this as ICarryable).GetCarryPivots();
        NavAgent = _navMeshAgent;
        
        var carried = new UnitCarried(_navMeshAgent, _rb, (this as ICarryable));
        _rb.constraints = 0;

        Func<bool> Carried = () => (Carriers >= CarriersNeeded);
        Func<bool> Uncarried = () => {
            if (Carriers < CarriersNeeded)
            {
                rigidIdle.entryForce = 200f * Vector3.up;
                return true;
            }
            return false;
        };

        void At(IState to, IState from, Func<bool> condition) => _stateMachine.AddTransition(to, from, condition);
        At(idle, carried, Carried);
        At(carried, rigidIdle, Uncarried);

        _stateMachine.SetState(rigidIdle);
    }
}
