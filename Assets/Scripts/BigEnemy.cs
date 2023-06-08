using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BigEnemy : MonoBehaviour, IDamageable
{

    private StateMachine _stateMachine;

    public float Health { get; set; }
    public TeamEnum Team { get; set; }
    public Transform SourceTransform { get; set; }
    public int ObjectID { get; set; }
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;
    public RallyData rallyData;
    public AttackData attackData;
    private Queue<DamageInstance> _damageQueue;

    private Collider[] _aggroHit;
    private float _aggroRange = 3f;

    private int playerUnitMask;
    private int walkableMask;
    private float _attackRange = 2.5f;
    public float timeGrounded = 0f;
    public bool attackFinished = false;

    // Start is called before the first frame update
    void Awake()
    {
        Health = 10f;
        Team = TeamEnum.Enemy;
        SourceTransform = transform;
        ObjectID = gameObject.GetInstanceID();

        rallyData = new RallyData();
        rallyData.destination = transform.position;
        rallyData.destinationObject = null;

        attackData = new AttackData();
        attackData.attackFinished = false;
        attackData.team = Team;

        _damageQueue = new Queue<DamageInstance>();

        _navMeshAgent = GetComponent<NavMeshAgent>();
        _rb = GetComponent<Rigidbody>();
        RigidbodyUtils.StandardizeRigidbody(_rb);

        playerUnitMask = LayerMask.GetMask("PlayerUnit");
        walkableMask = LayerMask.GetMask("Walkable");

        // Set up state machine
        _stateMachine = new StateMachine();
        var idle = new UnitIdle(_navMeshAgent, _rb);
        var findNavMesh = new UnitFindNavMesh(_navMeshAgent, _rb);
        var attackApproach = new UnitRally(_navMeshAgent, _rb, rallyData);
        var attack = new UnitBigAttack(_navMeshAgent, _rb, transform, attackData);
        var takeDamage = new UnitDamage(_navMeshAgent, _rb, _damageQueue, (this as IDamageable));
        var lookAt = new UnitLookAt(_navMeshAgent, transform);

        Func<bool> InAggroRange = () =>
        {
            _aggroHit = Physics.OverlapSphere(transform.position, _aggroRange, playerUnitMask);
            if (_aggroHit.Length > 0){
                attackData.attackTarget = _aggroHit[0].gameObject;
            }
            return (_aggroHit.Length > 0);
        };
        Func<bool> NearAttackTarget = () => (attackData.attackTarget != null && 
                                             Vector3.Distance(attackData.attackTarget.transform.position, this.transform.position) < _attackRange);
        Func<bool> AttackFinished = () => (timeGrounded > 2f && attackData.attackFinished);
        Func<bool> FoundNavMesh = () => (_navMeshAgent.isOnNavMesh);
        Func<bool> DamageFinished = () => (timeGrounded > 0.7f);

        void At(IState to, IState from, Func<bool> condition) => _stateMachine.AddTransition(to, from, condition);
        At(idle, attackApproach, InAggroRange);
        At(attackApproach, attack, NearAttackTarget);
        At(attack, findNavMesh, AttackFinished);
        At(findNavMesh, idle, FoundNavMesh);
        _stateMachine.AddAnyTransition(takeDamage, () => _damageQueue.Count > 0);
        At(takeDamage, findNavMesh, DamageFinished);

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

    public void OnDamage(DamageInstance damage){
        _damageQueue.Enqueue(damage);
    }
}
