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
    private float _lookRange = 7f;
    public bool attackFinished = false;
    private Vector3 anchor;
    private bool lastMoveWasTeleport = false;

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
        var meteorAttack = new UnitMeteorAttack(_navMeshAgent, _rb, transform, attackData);
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
            bool go = false;
            _aggroHit = Physics.OverlapSphere(transform.position, _aggroRange, playerUnitMask);
            if (_aggroHit.Length > 0 && lastMoveWasTeleport){
                attackData.attackTarget = _aggroHit[0].gameObject;
                lastMoveWasTeleport = false;
                go = true;
            }
            return (_aggroHit.Length > 0 && go);
        };
        Func<bool> InTeleportRange = () =>
        {
            bool go = false;
            _aggroHit = Physics.OverlapSphere(transform.position, _aggroRange, playerUnitMask);
            if (_aggroHit.Length > 0 && !lastMoveWasTeleport){
                // Pass teleport data
                lastMoveWasTeleport = true;
                go = true;
            }
            return (_aggroHit.Length > 0 && go);
        };
        Func<bool> NotInAggroRange = () =>
        {
            _aggroHit = Physics.OverlapSphere(transform.position, _aggroRange, playerUnitMask);
            return (_aggroHit.Length == 0);
        };
        Func<bool> NearAttackTarget = () => (attackData.attackTarget != null && 
                                             Vector3.Distance(attackData.attackTarget.transform.position, this.transform.position) < _attackRange);
        Func<bool> AttackFinished = () => (timeGrounded > 2f && attackData.attackFinished);
        Func<bool> Warp = () => (true);
        Func<bool> WarpFinished = () => (teleport.warpDone);

        void At(IState to, IState from, Func<bool> condition) => _stateMachine.AddTransition(to, from, condition);
        At(idle, lookAt, InLookRange);
        At(lookAt, meteorAttack, InAggroRange);
        At(lookAt, teleport, InTeleportRange);
        At(teleport, idle, WarpFinished);
        At(meteorAttack, idle, AttackFinished);

        _stateMachine.SetState(rigidIdle);

    }
}
