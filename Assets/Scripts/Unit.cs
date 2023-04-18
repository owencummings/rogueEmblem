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

    private int walkableMask;
    private float attackRange = 2f;
    public float timeGrounded = 0f;
    public bool attackFinished = false;


    void Awake(){
        rallyDestination = transform.position;
        nextDestination = transform.position;
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _rb = GetComponent<Rigidbody>();
        _rb.angularDrag = 1f;
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        walkableMask = LayerMask.NameToLayer("Walkable");

        // Set up state machine
        _stateMachine = new StateMachine();
        var findNavMesh = new UnitFindNavMesh(this, _navMeshAgent, _rb);
        var rally = new UnitRally(this, _navMeshAgent, _rb);
        var attackApproach = new UnitAttackApproach(this, _navMeshAgent, _rb);
        var attack = new UnitAttack(this, _navMeshAgent, _rb);


        // State machine transition conditions
        Func<bool> NewRally = () => rallyDestination.Equals(nextDestination);
        Func<bool> NewAttackTarget = () => ((nextAttackTarget != null) &&
                                            ((attackTarget == null) || (attackTarget.GetInstanceID() != nextAttackTarget.GetInstanceID())));
        Func<bool> NearAttackTarget = () => (Vector3.Distance(attackTarget.transform.position, this.transform.position) < attackRange);
        Func<bool> AttackFinished = () => (timeGrounded > 0.25f && attackFinished);
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
        _stateMachine.Tick();
    }

    void FixedUpdate()
    {
        if (Physics.Raycast(transform.position, Vector3.down, 0.1f, ~walkableMask)) {
            timeGrounded += Time.fixedDeltaTime;
        } else {
            timeGrounded = 0;
        }
        Debug.Log(timeGrounded);
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
