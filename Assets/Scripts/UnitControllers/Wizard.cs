using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wizard : Unit, IDamageable
{
    public AttackData attackData;

    private Collider[] _aggroHit;
    private float _aggroRange = 5f;

    private int playerUnitMask;
    private float _attackRange = 5f;
    public bool attackFinished = false;
    
    new void Awake()
    {
        UnitAwake();
        Health = 10f;
        Team = TeamEnum.Enemy;

        attackData = new AttackData();
        attackData.attackFinished = false;
        attackData.team = Team;

        playerUnitMask = LayerMask.GetMask("PlayerUnit");

        var teleport = new UnitTeleport(_navMeshAgent, _rb, transform, rallyData);
        var meteorAttack = new UnitMeteorAttack(_navMeshAgent, _rb, rallyData);
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
        Func<bool> Warp = () => (true);

        void At(IState to, IState from, Func<bool> condition) => _stateMachine.AddTransition(to, from, condition);
        At(idle, teleport, Warp);

        _stateMachine.SetState(rigidIdle);

    }
}
