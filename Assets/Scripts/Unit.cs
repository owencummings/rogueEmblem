using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnitCommands;

public class Unit : MonoBehaviour
{
    private StateMachine _stateMachine;

    // Set up data
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;
    public Vector3 rallyDestination;
    public Vector3 nextDestination;
    public GameObject attackTarget;
    public GameObject nextAttackTarget;
    private Squad parentSquad; 


    void Awake(){
        rallyDestination = transform.position;
        nextDestination = transform.position;
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _rb = GetComponent<Rigidbody>();
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // Set up state machine
        _stateMachine = new StateMachine();
        var idle = new UnitIdle(this, _navMeshAgent);
        var rally = new UnitRally(this, _navMeshAgent);
        var attackApproach = new UnitAttackApproach(this, _navMeshAgent);
        var attack = new UnitAttack(this, _navMeshAgent, _rb);


        // State machine transition conditions
        Func<bool> NewRally = () => rallyDestination.Equals(nextDestination);
        Func<bool> NewAttackTarget = () => ((nextAttackTarget != null && attackTarget == null) 
                                            || (attackTarget.GetInstanceID() != nextAttackTarget.GetInstanceID()));

        // State machine conditions
        void At(IState to, IState from, Func<bool> condition) => _stateMachine.AddTransition(to, from, condition);
        //At(attack, rally, NewRally);
        At(rally, attack, NewAttackTarget);


        _stateMachine.SetState(rally);

    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        _stateMachine.Tick();
    }

    public void OnCommand(UnitCommand unitCommand){
        if (unitCommand.CommandEnum  == UnitCommandEnum.Rally)
        {
            nextDestination = unitCommand.TargetDestination;
        }

        if (unitCommand.CommandEnum == UnitCommandEnum.Attack)
        {
            nextAttackTarget = unitCommand.TargetGameObject;
        }
    }
}
