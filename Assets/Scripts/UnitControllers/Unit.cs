using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnitCommands;
using UnitAttributes;

public class Unit : NavBody, IDamageable 
{
    public RallyData rallyData;
    public LookData lookData;
    private Queue<DamageInstance> _damageQueue;

    public float Health { get; set; }
    public TeamEnum Team { get; set; }
    public Transform SourceTransform { get; set; }
    public int ObjectID { get; set; }
    public Squad parentSquad;
    public UnitType unitTypeEnum;

    #region Behaviors
    internal UnitRally rally;
    internal UnitDamage takeDamage;
    internal UnitDeath death;
    #endregion
    

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
        unitTypeEnum = UnitType.None;

        rallyData = new RallyData();
        rallyData.destination = transform.position;
        rallyData.destinationObject = null;

        lookData = new LookData();

        _damageQueue = new Queue<DamageInstance>();

        rally = new UnitRally(_navMeshAgent, _rb, rallyData);
        takeDamage = new UnitDamage(_navMeshAgent, _rb, _damageQueue, (this as IDamageable));
        takeDamage.team = Team;
        death = new UnitDeath(_navMeshAgent, _rb, this.gameObject);


        #region TransitionConditions        
        Func<bool> DamageFinished = () => (timeGrounded > 0.5f && takeDamage.timeRecoiled > takeDamage.timeToRecoil);
        Func<bool> NoHealth = () => (Health <= 0.0f);
        #endregion

        #region StateMachineTransitions
        void At(IState to, IState from, Func<bool> condition) => _stateMachine.AddTransition(to, from, condition);
        _stateMachine.AddAnyTransition(takeDamage, () => _damageQueue.Count > 0);
        At(takeDamage, findNavMesh, DamageFinished);
        At(takeDamage, death, NoHealth);
        #endregion

        _stateMachine.SetState(rigidIdle);
    }

    void Start ()
    {
        // Just do this after Awake() so that subclasses make their necessary changes.
        UnitAttributes.BirdPalettes.ColorizeTexture(transform, unitTypeEnum);
    }

    public void OnDamage(DamageInstance damage){
        _damageQueue.Enqueue(damage);
    }
}
