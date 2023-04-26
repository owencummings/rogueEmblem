using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnitCommands;

public class Unit : MonoBehaviour, IDamageable
{
    private StateMachine _stateMachine;

    // Set up data
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;
    public RallyVectors rallyVectors;
    public AttackData attackData;
    private Queue<DamageInstance> _damageQueue;
    public float Health { get; set; }
    public TeamEnum Team { get; set; }
    public Transform SourceTransform { get; set; }
    private Squad parentSquad; 

    private int walkableMask;
    private float attackRange = 2f;
    public float timeGrounded = 0f;


    void Awake(){
        Health = 3f;
        Team = TeamEnum.Player;
        SourceTransform = transform;

        rallyVectors = new RallyVectors();
        rallyVectors.rallyDestination = transform.position;
        rallyVectors.nextDestination = transform.position;

        attackData = new AttackData();
        attackData.attackFinished = false;
        attackData.team = Team;

        _damageQueue = new Queue<DamageInstance>();


        _navMeshAgent = GetComponent<NavMeshAgent>();
        _rb = GetComponent<Rigidbody>();
        RigidbodyUtils.StandardizeRigidbody(_rb);

        walkableMask = LayerMask.GetMask("Walkable");

        // Set up state machine
        _stateMachine = new StateMachine();
        var findNavMesh = new UnitFindNavMesh(_navMeshAgent, _rb);
        var rally = new UnitRally(_navMeshAgent, _rb, rallyVectors);
        var attackApproach = new UnitAttackApproach(_navMeshAgent, _rb, attackData);
        var attack = new UnitAttack(_navMeshAgent, _rb, transform, attackData);
        var takeDamage = new UnitDamage(_navMeshAgent, _rb, _damageQueue, (this as IDamageable));
        var death = new UnitDeath(_navMeshAgent, _rb, this.gameObject);

        // State machine transition conditions
        Func<bool> NewRally = () => rallyVectors.rallyDestination.Equals(rallyVectors.nextDestination);
        Func<bool> NewAttackTarget = () => ((attackData.nextAttackTarget != null) &&
                                            ((attackData.attackTarget == null) ||
                                             (attackData.attackTarget.GetInstanceID() != attackData.nextAttackTarget.GetInstanceID())));
        Func<bool> NearAttackTarget = () => (Vector3.Distance(attackData.attackTarget.transform.position, this.transform.position) < attackRange);
        Func<bool> AttackFinished = () => (timeGrounded > 0.25f && attackData.attackFinished);
        Func<bool> FoundNavMesh = () => (_navMeshAgent.isOnNavMesh);
        Func<bool> DamageFinished = () => (timeGrounded > 0.7f && takeDamage.timeRecoiled > 1f);
        Func<bool> NoHealth = () => (Health <= 0.0f);

        // State machine conditions
        void At(IState to, IState from, Func<bool> condition) => _stateMachine.AddTransition(to, from, condition);
        At(rally, attackApproach, NewAttackTarget);
        // Fix below to cancel attackapproaches
        //At(attackApproach, rally, NewRally);
        At(attackApproach, attack, NearAttackTarget);
        At(attack, findNavMesh, AttackFinished);
        At(findNavMesh, rally, FoundNavMesh);
        _stateMachine.AddAnyTransition(takeDamage, () => _damageQueue.Count > 0);
        At(takeDamage, findNavMesh, DamageFinished);
        At(takeDamage, death, NoHealth);


        _stateMachine.SetState(rally);

    }

    void Update()
    {
        if (PauseManager.paused){ return; }
        _stateMachine.Tick();
    }

    void FixedUpdate()
    {
        if (Physics.OverlapSphere(transform.position - Vector3.down * 0.1f,
                                  transform.localScale.x/2,
                                  walkableMask).Length != 0)
        {
            timeGrounded += Time.fixedDeltaTime;
        } else {
            timeGrounded = 0;
        }
    }

    void OnCollisionEnter(Collision collision){
        _stateMachine.OnCollisionEnter(collision);
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

    public void OnDamage(DamageInstance damage){
        _damageQueue.Enqueue(damage);
    }
}
