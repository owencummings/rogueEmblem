using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lurker : Unit, IDamageable
{
    public AttackData attackData;
    private int playerUnitMask;
    private float _lurkRange = 2f;
    private float _aggroRange = 3f;
    private float _attackRange = 1.5f;
    private Collider[] _aggroHit;
    public bool attackFinished = false;
    private Vector3 anchor;

    new void Awake()
    {
        UnitAwake();
        
        transform.position = new Vector3(transform.position.x, -5, transform.position.z);

        Health = 10f;
        Team = TeamEnum.Enemy;

        attackData = new AttackData();
        attackData.attackFinished = false;
        attackData.team = Team;

        playerUnitMask = LayerMask.GetMask("PlayerUnit");

        var lurk = new UnitLurk(_navMeshAgent, _rb, transform);
        var pounce = new UnitPounce(_navMeshAgent, _rb, transform, attackData);
        var attack = new UnitAttack(_navMeshAgent, _rb, transform, attackData);

        Func<bool> InLurkRange = () =>
        {
            _aggroHit = Physics.OverlapBox(transform.position, new Vector3(_lurkRange, 10, _lurkRange), Quaternion.identity, playerUnitMask);
            if (_aggroHit.Length > 0){
                attackData.attackTarget = _aggroHit[0].gameObject;
            }
            return _aggroHit.Length > 0;
        };
        Func<bool> InAggroRange = () =>
        {
            _aggroHit = Physics.OverlapSphere(transform.position, _aggroRange, playerUnitMask);
            if (_aggroHit.Length > 0){
                rallyData.destination = Vector3.zero;
                rallyData.destinationObject = _aggroHit[0].gameObject;
            }
            return _aggroHit.Length > 0;
        };
        Func<bool> InAttackRange = () => 
        {
            _aggroHit = Physics.OverlapSphere(transform.position, _attackRange, playerUnitMask);
            if (_aggroHit.Length > 0){
                attackData.attackTarget = _aggroHit[0].gameObject;
            }
            return _aggroHit.Length > 0;
        };
        Func<bool> Targetless = () => { return rallyData.destinationObject == null; };
        Func<bool> IsGrounded = () => {
            if (timeGrounded > 0.5f && _rb.velocity.magnitude < 0.01f)
            {
                return true;
            }
            return false;
        };
        Func<bool> AttackDone = () => { return attackData.attackFinished; };

        void At(IState to, IState from, Func<bool> condition) => _stateMachine.AddTransition(to, from, condition);
        At(lurk, pounce, InLurkRange);
        At(pounce, rigidIdle, IsGrounded);
        At(idle, rally, InAggroRange);
        At(rally, idle, Targetless);
        At(rally, attack, InAttackRange);
        At(attack, rigidIdle, AttackDone);

        _stateMachine.SetState(lurk);

    }
}
