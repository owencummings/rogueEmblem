using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Food : Carryable1
{
    public bool toIngest = false;
    public bool ingested = false;
    public int value = 3;

    new void Awake(){
        CarryableAwake();
        UnitIngest ingest = new UnitIngest(_navMeshAgent, _rb, transform);
        UnitDeath death = new UnitDeath(_navMeshAgent, _rb, gameObject);

        Func<bool> ToIngest = () => {
            if (toIngest && !ingested) {
                ingested = true;
                return true;
            }
            return false;
        };

        _stateMachine.AddAnyTransition(ingest, ToIngest);
        _stateMachine.AddTransition(ingest, death, () => (ingest.ingestProgress > ingest.ingestDuration));
    }

    void Ingest(){
        toIngest = true;
    }
}
