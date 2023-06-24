using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnitCommands;


public class Melee : Unit
{

    public AttackData attackData;
    private float attackRange = 2f;

    new void Awake()
    {
        UnitAwake();
        attackData = new AttackData();
        attackData.attackFinished = false;
        attackData.team = Team;

        var attackRally = new UnitRally(_navMeshAgent, _rb, rallyData);
        var attack = new UnitAttack(_navMeshAgent, _rb, transform, attackData);

        Func<bool> NearAttackTarget = () => (attackData.attackTarget != null &&
                                             Vector3.Distance(attackData.attackTarget.transform.position, this.transform.position) < attackRange);
        Func<bool> AttackFinished = () => (timeGrounded > 0.25f && attackData.attackFinished);
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

        void At(IState to, IState from, Func<bool> condition) => _stateMachine.AddTransition(to, from, condition);
        At(attackRally, idle, NewValidCommand);
        At(idle, attackRally, NewAttack);
        At(attackRally, attack, NearAttackTarget);
        At(attack, findNavMesh, AttackFinished);
    }
}
