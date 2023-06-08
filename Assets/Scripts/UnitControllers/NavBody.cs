using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
public abstract class NavBody : MonoBehaviour
{

    internal StateMachine _stateMachine;
    internal NavMeshAgent _navMeshAgent;
    internal Rigidbody _rb;
    internal float timeGrounded;
    internal int walkableMask;

    internal UnitIdle idle;
    internal UnitFindNavMesh findNavMesh;
    internal UnitRigidIdle rigidIdle; 

    internal void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _rb = GetComponent<Rigidbody>();
        RigidbodyUtils.StandardizeRigidbody(_rb);
        walkableMask = LayerMask.GetMask("Walkable");

        // Set up state machine
        _stateMachine = new StateMachine();
        idle = new UnitIdle(_navMeshAgent, _rb);
        findNavMesh = new UnitFindNavMesh(_navMeshAgent, _rb);
        rigidIdle  = new UnitRigidIdle(_navMeshAgent, _rb);

        Func<bool> IsGrounded = () => (timeGrounded > 0.5f && _rb.velocity.magnitude < 0.01f && rigidIdle.timeIdling > rigidIdle.timeToIdle);
        Func<bool> FoundNavMesh = () => (_navMeshAgent.isOnNavMesh);

        void At(IState to, IState from, Func<bool> condition) => _stateMachine.AddTransition(to, from, condition);
        At(rigidIdle, findNavMesh, IsGrounded);
        At(findNavMesh, idle, FoundNavMesh);
    }

    internal void Update()
    {
        if (PauseManager.paused){ return; }
        _stateMachine.Tick();
    }

    // Could break this out into a modular, grounded-check part
    internal void FixedUpdate()
    {
        if (Physics.OverlapSphere(transform.position + Vector3.down * 0.1f,
                                  transform.localScale.x/2,
                                  walkableMask).Length != 0)
        {
            timeGrounded += Time.fixedDeltaTime;
        } else {
            timeGrounded = 0;
        }
    }

    internal void OnCollisionEnter(Collision collision){
        _stateMachine.OnCollisionEnter(collision);
    }
}
