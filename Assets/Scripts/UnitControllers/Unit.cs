using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnitCommands;

public interface ICommandable{
    void OnCommand(UnitCommand unitCommand);
}

public class Unit : NavBody, IDamageable, ICommandable
{
    public RallyData rallyData;
    public CarryData carryData;
    private Queue<DamageInstance> _damageQueue;
    private bool newCommand = false;
    internal UnitCommand mostRecentCommand;
    public float Health { get; set; }
    public TeamEnum Team { get; set; }
    public Transform SourceTransform { get; set; }
    public int ObjectID { get; set; }
    private Squad parentSquad;

    #region Behaviors
    internal UnitRally rally;
    internal UnitDamage takeDamage;
    internal UnitDeath death;
    internal UnitRally carryRally;
    internal UnitCarry carry;
    #endregion
    
    internal Func<bool> NewValidCommand;

    new void Awake(){
        UnitAwake();
    }

    internal void UnitAwake()
    {
        NavBodyAwake();
        Health = 3f;
        Team = TeamEnum.Player;
        SourceTransform = transform;
        ObjectID = gameObject.GetInstanceID();

        mostRecentCommand = new UnitCommand(UnitCommandEnum.None, Vector3.zero, null);

        rallyData = new RallyData();
        rallyData.destination = transform.position;
        rallyData.destinationObject = null;

        carryData = new CarryData();
        carryData.carryTarget = null;
        carryData.carryPivot = Vector3.zero;

        _damageQueue = new Queue<DamageInstance>();

        rally = new UnitRally(_navMeshAgent, _rb, rallyData);
        takeDamage = new UnitDamage(_navMeshAgent, _rb, _damageQueue, (this as IDamageable));
        death = new UnitDeath(_navMeshAgent, _rb, this.gameObject);
        carryRally = new UnitRally(_navMeshAgent, _rb, rallyData);
        carry = new UnitCarry(_navMeshAgent, _rb, transform, carryData);
        Debug.Log("Carry init");

        #region TransitionConditions        

        Func<bool> NearCarryTarget = () => {
            return (rallyData.destinationObject != null && rallyData.destination != null &&
                    Vector3.Distance(rallyData.destinationObject.transform.position + rallyData.destination, this.transform.position) < 0.3f);
        };
        Func<bool> DamageFinished = () => (timeGrounded > 0.5f && takeDamage.timeRecoiled > 0.5f);
        Func<bool> NoHealth = () => (Health <= 0.0f);
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
        #endregion

        #region StateMachineTransitions
        void At(IState to, IState from, Func<bool> condition) => _stateMachine.AddTransition(to, from, condition);
        At(rally, idle, NewValidCommand);
        At(carryRally, idle, NewValidCommand);
        At(idle, rally, NewRally);
        At(idle, carryRally, NewCarry);
        At(carryRally, carry, NearCarryTarget);
        At(carry, findNavMesh, NewCancel);
        _stateMachine.AddAnyTransition(takeDamage, () => _damageQueue.Count > 0);
        At(takeDamage, findNavMesh, DamageFinished);
        At(takeDamage, death, NoHealth);
        #endregion

        _stateMachine.SetState(rigidIdle);
    }

    public void OnCommand(UnitCommand unitCommand)
    {
        // If carrying, pass destination to carry navmeshagent
        if (_stateMachine.currentState == carry && unitCommand.CommandEnum == UnitCommandEnum.Rally)
        {
            Debug.Log(carry);
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

    public void OnDamage(DamageInstance damage){
        _damageQueue.Enqueue(damage);
    }
}
