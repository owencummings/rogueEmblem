using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnitCommands;
using UnitAttributes;

public class Archer : ControllableUnit
{
    public AttackData attackData;
    private float attackRange = UnitArcherAttack.attackRange;

    new void Awake(){
        ControllableUnitAwake();

        unitTypeEnum = UnitType.Archer;

        attackData = new AttackData();
        attackData.attackFinished = false;
        attackData.team = Team;

        var attackApproach = new UnitRally(_navMeshAgent, _rb, rallyData);
        var attack = new UnitArcherAttack(_navMeshAgent, _rb, transform, attackData);


        // State machine transition conditions
        Func<bool> NearAttackTarget = () => (attackData.attackTarget != null && Vector3.Distance(attackData.attackTarget.transform.position, this.transform.position) < attackRange);
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
        At(attackApproach, idle, NewValidCommand);
        At(idle, attackApproach, NewAttack);
        At(attackApproach, attack, NearAttackTarget);
        At(attack, idle, AttackFinished);

        _stateMachine.SetState(rigidIdle);

    }
}
