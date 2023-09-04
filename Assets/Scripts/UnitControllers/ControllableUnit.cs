using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnitCommands;


public interface ICommandable{
    void OnCommand(UnitCommand unitCommand);
}


public class ControllableUnit : Unit, ICommandable
{
    public CarryData carryData;
    public AttackData jumpData;
    public RallyData glideRallyData;

    private bool newCommand = false;
    internal UnitCommand mostRecentCommand;
    internal UnitCommand cachedCommand;
    internal UnitCommand nullCommand =  new UnitCommand(UnitCommandEnum.None);
    internal UnitCarry carry;
    internal UnitRally carryRally;
    internal UnitJump jump;
    internal UnitGlide glide;
    internal Func<bool> NewValidCommand;

    new void Awake()
    {
        ControllableUnitAwake();
    }

    internal void ControllableUnitAwake()
    {
        UnitAwake();
        mostRecentCommand = new UnitCommand(UnitCommandEnum.None, Vector3.zero, null);

        carryData = new CarryData();
        carryData.carryTarget = null;
        carryData.carryPivot = Vector3.zero;

        jumpData = new AttackData();
        jumpData.attackFinished = false;
        jumpData.team = Team;

        glideRallyData = new RallyData();

        carryRally = new UnitRally(_navMeshAgent, _rb, rallyData);
        carry = new UnitCarry(_navMeshAgent, _rb, transform, carryData);
        jump = new UnitJump(_navMeshAgent, _rb, transform, jumpData);
        glide = new UnitGlide(_navMeshAgent, _rb, transform, glideRallyData);

        Func<bool> NearCarryTarget = () => {
            return (rallyData.destinationObject != null && rallyData.destination != null &&
                    Vector3.Distance(rallyData.destinationObject.transform.position + rallyData.destination, this.transform.position) < 0.3f);
        };

        NewValidCommand = () => {
            if (newCommand == true){
                newCommand = false;
                return true;
            }
            return false;
        };

        Func<bool> NewRally = () => {
            if (mostRecentCommand.CommandEnum == UnitCommandEnum.Rally)
            {
                cachedCommand = mostRecentCommand;
                rallyData.destination = mostRecentCommand.TargetDestination;
                rallyData.destinationObject = null;
                glideRallyData.destination = mostRecentCommand.TargetDestination;
                glideRallyData.destinationObject = null;
                return true;
            }
            return false;
        };

        Func<bool> NewCarry = () => {
            if (mostRecentCommand.CommandEnum == UnitCommandEnum.Carry)
            {
                cachedCommand = mostRecentCommand;
                rallyData.destination = new Vector3 (mostRecentCommand.TargetDestination.x * 1.5f,
                                                     mostRecentCommand.TargetDestination.y,
                                                     mostRecentCommand.TargetDestination.z * 1.5f);
                rallyData.destinationObject = mostRecentCommand.TargetGameObject;
                carryData.carryPivot = mostRecentCommand.TargetDestination;
                carryData.carryTarget = mostRecentCommand.TargetGameObject;
                return true;
            }
            return false;
        };

        Func<bool> NewCancel = () => {
            if (mostRecentCommand.CommandEnum == UnitCommandEnum.Cancel)
            {
                cachedCommand = nullCommand;
                // TODO re-rally to new destination
                /*
                rallyData.destination = new Vector3 (mostRecentCommand.TargetDestination.x * 1.5f,
                                                     mostRecentCommand.TargetDestination.y,
                                                     mostRecentCommand.TargetDestination.z * 1.5f);
                rallyData.destinationObject = mostRecentCommand.TargetGameObject;
                */
                return true;
            }
            return false;
        };

        Func<bool> NewJump = () => {
            if (mostRecentCommand.CommandEnum == UnitCommandEnum.Jump)
            {
                mostRecentCommand = nullCommand;
                return true;
            }
            return false;
        };

        Func<bool> NewGlide = () =>  {
            if (mostRecentCommand.CommandEnum == UnitCommandEnum.Jump)
            {
                mostRecentCommand = nullCommand;
                return true;
            }
            return false;
        };

        Func<bool> JumpFinished = () => {
            if (timeGrounded > 0.25f && jumpData.attackFinished){
                mostRecentCommand = cachedCommand;
                newCommand = true;
                return true;
            }
            return false;
        };

        Func<bool> NearGround = () => {
            if (timeGrounded > 0.5f) {
                mostRecentCommand = cachedCommand;
                return true;
            }
            return false;
        };

        #region StateMachineTransitions
        void At(IState to, IState from, Func<bool> condition) => _stateMachine.AddTransition(to, from, condition);
        At(rally, idle, NewValidCommand);
        At(carryRally, idle, NewValidCommand);
        At(idle, rally, NewRally);
        At(idle, carryRally, NewCarry);
        At(carryRally, carry, NearCarryTarget);
        At(carry, findNavMesh, NewCancel);
        At(rally, jump, NewJump);
        At(idle, jump, NewJump);
        At(jump, rigidIdle, JumpFinished);
        At(jump, glide, NewGlide);
        At(glide, rigidIdle, NewGlide);
        At(rigidIdle, glide, NewGlide);
        At(glide, rigidIdle, NearGround);
        #endregion

    }

    public void OnCommand(UnitCommand unitCommand)
    {
        // If carrying, pass destination to carry navmeshagent
        if (_stateMachine.currentState == carry && unitCommand.CommandEnum == UnitCommandEnum.Rally)
        {
            if (carry.carryable.Carriers >= carry.carryable.CarriersNeeded)
            {
                // Convert back to square-centered destination
                // Should probably just pass this with unitCommand
                Vector3 newDestination = new Vector3(Mathf.RoundToInt(unitCommand.TargetDestination.x),
                                                    unitCommand.TargetDestination.y + 0.5f,
                                                    Mathf.RoundToInt(unitCommand.TargetDestination.z));
                carry.carryable.NavAgent.destination = newDestination;
            }
            return;
        } 

        if (_stateMachine.currentState == glide)
        {
            // Complete this functionality & probably put it somewhere else
            if (unitCommand.TargetGameObject != null){ glideRallyData.destinationObject = unitCommand.TargetGameObject; }
            else if (unitCommand.TargetDestination != null){ glideRallyData.destination = unitCommand.TargetDestination; glide._cachedRallyPoint = unitCommand.TargetDestination; }
        }

        newCommand = true;
        mostRecentCommand = unitCommand;
    }

}
