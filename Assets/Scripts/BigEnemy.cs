using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BigEnemy : MonoBehaviour
{

    private StateMachine _stateMachine;

    // Set up data
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;
    public RallyVectors rallyVectors;
    public AttackData attackData;

    private Collider[] _aggroHit;
    private float _aggroRange = 3f;

    private int playerUnitMask;
    private int walkableMask;
    private float _attackRange = 1.5f;
    public float timeGrounded = 0f;
    public bool attackFinished = false;

    // Start is called before the first frame update
    void Awake()
    {
        rallyVectors = new RallyVectors();
        rallyVectors.rallyDestination = transform.position;
        rallyVectors.nextDestination = transform.position;

        attackData = new AttackData();
        attackData.attackFinished = false;

        _navMeshAgent = GetComponent<NavMeshAgent>();
        _rb = GetComponent<Rigidbody>();
        _rb.angularDrag = 1f;
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        playerUnitMask = LayerMask.GetMask("PlayerUnit");
        walkableMask = LayerMask.GetMask("Walkable");


        // Set up state machine
        _stateMachine = new StateMachine();
        var findNavMesh = new UnitFindNavMesh(_navMeshAgent, _rb);
        var rally = new UnitRally(_navMeshAgent, _rb, rallyVectors);
        var attackApproach = new UnitAttackApproach(_navMeshAgent, _rb, attackData);
        var attack = new UnitBigAttack(_navMeshAgent, _rb, transform, attackData);

        Func<bool> NewRally = () => rallyVectors.rallyDestination.Equals(rallyVectors.nextDestination);
        Func<bool> InAggroRange = () =>
        {
            _aggroHit = Physics.OverlapSphere(transform.position, _aggroRange, playerUnitMask);
            if (_aggroHit.Length > 0){
                attackData.nextAttackTarget = _aggroHit[0].gameObject;
            }
            return (_aggroHit.Length > 0);
        };
        Func<bool> NearAttackTarget = () => (Vector3.Distance(attackData.attackTarget.transform.position, this.transform.position) < _attackRange);
        Func<bool> AttackFinished = () => (timeGrounded > 1f && attackData.attackFinished);
        Func<bool> FoundNavMesh = () => (_navMeshAgent.isOnNavMesh);

        void At(IState to, IState from, Func<bool> condition) => _stateMachine.AddTransition(to, from, condition);
        At(rally, attackApproach, InAggroRange);
        At(attackApproach, attack, NearAttackTarget);
        At(attack, findNavMesh, AttackFinished);
        At(findNavMesh, rally, FoundNavMesh);
        _stateMachine.SetState(rally);

    }

    void FixedUpdate(){
        if (Physics.Raycast(transform.position, Vector3.down, 0.3f, walkableMask)) {
            timeGrounded += Time.fixedDeltaTime;
        } else {
            timeGrounded = 0;
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        if (PauseManager.paused){ return; }
        _stateMachine.Tick();
    }
}
