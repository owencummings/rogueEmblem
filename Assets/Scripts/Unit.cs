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
    public RallyVectors rallyVectors;
    public AttackData attackData;
    private Squad parentSquad; 

    private int walkableMask;
    private float attackRange = 2f;
    public float timeGrounded = 0f;


    void Awake(){
        rallyVectors = new RallyVectors();
        rallyVectors.rallyDestination = transform.position;
        rallyVectors.nextDestination = transform.position;

        attackData = new AttackData();
        attackData.attackFinished = false;

        _navMeshAgent = GetComponent<NavMeshAgent>();
        _rb = GetComponent<Rigidbody>();
        _rb.angularDrag = 1f;
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        walkableMask = LayerMask.GetMask("Walkable");

        // Set up state machine
        _stateMachine = new StateMachine();
        var findNavMesh = new UnitFindNavMesh(_navMeshAgent, _rb);
        var rally = new UnitRally(_navMeshAgent, _rb, rallyVectors);
        var attackApproach = new UnitAttackApproach(_navMeshAgent, _rb, attackData);
        var attack = new UnitAttack(_navMeshAgent, _rb, transform, attackData);


        // State machine transition conditions
        Func<bool> NewRally = () => rallyVectors.rallyDestination.Equals(rallyVectors.nextDestination);
        Func<bool> NewAttackTarget = () => ((attackData.nextAttackTarget != null) &&
                                            ((attackData.attackTarget == null) ||
                                             (attackData.attackTarget.GetInstanceID() != attackData.nextAttackTarget.GetInstanceID())));
        Func<bool> NearAttackTarget = () => (Vector3.Distance(attackData.attackTarget.transform.position, this.transform.position) < attackRange);
        Func<bool> AttackFinished = () => (timeGrounded > 0.25f && attackData.attackFinished);
        Func<bool> FoundNavMesh = () => (_navMeshAgent.isOnNavMesh);

        // State machine conditions
        void At(IState to, IState from, Func<bool> condition) => _stateMachine.AddTransition(to, from, condition);
        At(rally, attackApproach, NewAttackTarget);
        // Fix below to cancel attackapproaches
        // At(attackApproach, rally, NewRally);
        At(attackApproach, attack, NearAttackTarget);
        At(attack, findNavMesh, AttackFinished);
        At(findNavMesh, rally, FoundNavMesh);

        _stateMachine.SetState(rally);

    }

    void Update()
    {
        if (PauseManager.paused){ return; }
        _stateMachine.Tick();
    }

    void FixedUpdate()
    {
        if (Physics.Raycast(transform.position, Vector3.down, 0.1f, walkableMask)) {
            timeGrounded += Time.fixedDeltaTime;
        } else {
            timeGrounded = 0;
        }
    }

    public void OnCommand(UnitCommand unitCommand){
        if (unitCommand.CommandEnum  == UnitCommandEnum.Rally)
        {
            rallyVectors.nextDestination = unitCommand.TargetDestination;
        }

        if (unitCommand.CommandEnum == UnitCommandEnum.Attack)
        {
            attackData.nextAttackTarget = unitCommand.TargetGameObject;
        }
    }
}
