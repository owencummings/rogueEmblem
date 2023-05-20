using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnitCommands;

public interface ICommandable{
    void OnCommand(UnitCommand unitCommand);
}

public class Unit : MonoBehaviour, IDamageable, ICommandable
{
    private StateMachine _stateMachine;

    // Set up data
    protected NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;
    public RallyData rallyData;
    public AttackData attackData;
    public CarryData carryData;
    private Queue<DamageInstance> _damageQueue;
    private bool newCommand = false;
    private UnitCommand mostRecentCommand;
    public float Health { get; set; }
    public TeamEnum Team { get; set; }
    public Transform SourceTransform { get; set; }
    public int ObjectID { get; set; }
    private Squad parentSquad; 

    private int walkableMask;
    private float attackRange = 2f;
    public float timeGrounded = 0f;


    void Awake(){
        Health = 3f;
        Team = TeamEnum.Player;
        SourceTransform = transform;
        ObjectID = gameObject.GetInstanceID();

        mostRecentCommand = new UnitCommand(UnitCommandEnum.None, Vector3.zero, null);

        rallyData = new RallyData();
        rallyData.destination = transform.position;
        rallyData.destinationObject = null;

        attackData = new AttackData();
        attackData.attackFinished = false;
        attackData.team = Team;

        carryData = new CarryData();
        carryData.carryTarget = null;
        carryData.carryPivot = Vector3.zero;

        _damageQueue = new Queue<DamageInstance>();

        _navMeshAgent = GetComponent<NavMeshAgent>();
        _rb = GetComponent<Rigidbody>();
        RigidbodyUtils.StandardizeRigidbody(_rb);

        walkableMask = LayerMask.GetMask("Walkable");

        // Set up state machine
        _stateMachine = new StateMachine();
        var idle = new UnitIdle(_navMeshAgent, _rb);
        var findNavMesh = new UnitFindNavMesh(_navMeshAgent, _rb);
        var rally = new UnitRally(_navMeshAgent, _rb, rallyData);
        var attackRally = new UnitRally(_navMeshAgent, _rb, rallyData);
        var attack = new UnitAttack(_navMeshAgent, _rb, transform, attackData);
        var takeDamage = new UnitDamage(_navMeshAgent, _rb, _damageQueue, (this as IDamageable));
        var death = new UnitDeath(_navMeshAgent, _rb, this.gameObject);
        var carryRally = new UnitRally(_navMeshAgent, _rb, rallyData);
        var carry = new UnitCarry(_navMeshAgent, _rb);

        #region TransitionConditions        
        Func<bool> NearAttackTarget = () => (attackData.attackTarget != null &&
                                             Vector3.Distance(attackData.attackTarget.transform.position, this.transform.position) < attackRange);
        Func<bool> NearCarryTarget = () => {
            return (rallyData.destinationObject != null && rallyData.destination != null &&
                    Vector3.Distance(rallyData.destinationObject.transform.position + rallyData.destination, this.transform.position) < 0.1f);
        };
        Func<bool> AttackFinished = () => (timeGrounded > 0.25f && attackData.attackFinished);
        Func<bool> FoundNavMesh = () => (_navMeshAgent.isOnNavMesh);
        Func<bool> DamageFinished = () => (timeGrounded > 0.5f && takeDamage.timeRecoiled > 0.5f);
        Func<bool> NoHealth = () => (Health <= 0.0f);
        Func<bool> NewValidCommand = () => {
            if (newCommand == true){
                newCommand = false;
                return true;
            }
            return false;
        };

        Func<bool> NewRally = () => {
            if (mostRecentCommand.CommandEnum == UnitCommandEnum.Rally)
            {
                rallyData.destination = mostRecentCommand.TargetDestination;
                rallyData.destinationObject = null;
                return true;
            }
            return false;
        };

        Func<bool> NewAttack = () => {
            if (mostRecentCommand.CommandEnum == UnitCommandEnum.Attack)
            {
                rallyData.destination = Vector3.zero;
                rallyData.destinationObject = mostRecentCommand.TargetGameObject;
                attackData.attackTarget = mostRecentCommand.TargetGameObject;
                return true;
            }
            return false;
        };

        Func<bool> NewCarry = () => {
            if (mostRecentCommand.CommandEnum == UnitCommandEnum.Carry)
            {
                rallyData.destination = mostRecentCommand.TargetDestination * 1.5f;
                rallyData.destinationObject = mostRecentCommand.TargetGameObject;
                carryData.carryPivot = mostRecentCommand.TargetDestination;
                carryData.carryTarget = mostRecentCommand.TargetGameObject;
                return true;
            }
            return false;
        };
        #endregion

        #region StateMachineTransitions
        void At(IState to, IState from, Func<bool> condition) => _stateMachine.AddTransition(to, from, condition);
        At(rally, idle, NewValidCommand);
        At(attackRally, idle, NewValidCommand);
        At(carryRally, idle, NewValidCommand);
        At(idle, rally, NewRally);
        At(idle, attackRally, NewAttack);
        At(idle, carryRally, NewCarry);
        At(attackRally, attack, NearAttackTarget);
        At(attack, findNavMesh, AttackFinished);
        At(carryRally, carry, NearCarryTarget);
        At(findNavMesh, idle, FoundNavMesh);
        _stateMachine.AddAnyTransition(takeDamage, () => _damageQueue.Count > 0);
        At(takeDamage, findNavMesh, DamageFinished);
        At(takeDamage, death, NoHealth);
        #endregion

        _stateMachine.SetState(idle);

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

    public void OnCommand(UnitCommand unitCommand)
    {
        newCommand = true;
        mostRecentCommand = unitCommand;
    }

    public void OnDamage(DamageInstance damage){
        _damageQueue.Enqueue(damage);
    }
}
