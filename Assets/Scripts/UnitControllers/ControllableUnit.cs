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

    private bool newCommand = false;
    internal UnitCommand mostRecentCommand;
    internal UnitCarry carry;
    internal UnitRally carryRally;
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

        carryRally = new UnitRally(_navMeshAgent, _rb, rallyData);
        carry = new UnitCarry(_navMeshAgent, _rb, transform, carryData);

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
                rallyData.destination = mostRecentCommand.TargetDestination;
                rallyData.destinationObject = null;
                return true;
            }
            return false;
        };
        Func<bool> NewCarry = () => {
            if (mostRecentCommand.CommandEnum == UnitCommandEnum.Carry)
            {
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

        #region StateMachineTransitions
        void At(IState to, IState from, Func<bool> condition) => _stateMachine.AddTransition(to, from, condition);
        At(rally, idle, NewValidCommand);
        At(carryRally, idle, NewValidCommand);
        At(idle, rally, NewRally);
        At(idle, carryRally, NewCarry);
        At(carryRally, carry, NearCarryTarget);
        At(carry, findNavMesh, NewCancel);
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

        newCommand = true;
        mostRecentCommand = unitCommand;
    }

}