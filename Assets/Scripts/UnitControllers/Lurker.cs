using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lurker : Unit, IDamageable
{
    public AttackData attackData;

    private Collider[] _aggroHit;
    private float _aggroRange = 5f;

    private int playerUnitMask;
    private float _lurkRange = 2f;
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

        var lurk = new UnitLurk(_navMeshAgent, _rb, transform);
        var pounce = new UnitPounce(_navMeshAgent, _rb, transform);

        Func<bool> InAggroRange = () =>
        {
            bool go = false;
            _aggroHit = Physics.OverlapSphere(transform.position, _aggroRange, playerUnitMask); // Use overlapbox with big height
            if (_aggroHit.Length > 0 && lastMoveWasTeleport){
                attackData.attackTarget = _aggroHit[0].gameObject;
                lastMoveWasTeleport = false;
                go = true;
            }
            return (_aggroHit.Length > 0 && go);
        };
        Func<bool> True = () => { return true; };

        void At(IState to, IState from, Func<bool> condition) => _stateMachine.AddTransition(to, from, condition);
        At(lurk, pounce, True);
        At(pounce, rigidIdle, True);

        _stateMachine.SetState(lurk);

    }
}
