using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BigEnemy : Unit, IDamageable
{
    public AttackData attackData;

    private Collider[] _aggroHit;
    private float _aggroRange = 3f;

    private int playerUnitMask;
    private float _attackRange = 2.5f;
    private float _lookRange = 5f;
    public bool attackFinished = false;

    // Start is called before the first frame update
    new void Awake()
    {
        UnitAwake();
        unitTypeEnum = UnitAttributes.UnitType.BigEnemy;
        Health = 10f;
        Team = TeamEnum.Enemy;

        attackData = new AttackData();
        attackData.attackFinished = false;
        attackData.team = Team;

        playerUnitMask = LayerMask.GetMask("PlayerUnit");

        var attackApproach = new UnitRally(_navMeshAgent, _rb, rallyData);
        var attack = new UnitBigAttack(_navMeshAgent, _rb, transform, attackData);
        var lookAt = new UnitLookAt(_navMeshAgent, transform, lookData);

        Func<bool> InLookRange = () =>
        {
            _aggroHit = Physics.OverlapSphere(transform.position, _lookRange, playerUnitMask);
            if (_aggroHit.Length > 0){
                lookData.lookTarget = _aggroHit[0].gameObject;
            }
            return (_aggroHit.Length > 0);
        };
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

        void At(IState to, IState from, Func<bool> condition) => _stateMachine.AddTransition(to, from, condition);
        At(idle, lookAt, InLookRange);
        At(lookAt, attackApproach, InAggroRange);
        At(attackApproach, attack, NearAttackTarget);
        At(attack, findNavMesh, AttackFinished);

        _stateMachine.SetState(rigidIdle);

    }
}
