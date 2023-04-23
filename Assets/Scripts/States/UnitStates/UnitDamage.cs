using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitDamage : IState
{

    private UnityEngine.AI.NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;
    private Queue<DamageInstance> _damageQueue;

    private float _timeToRecoil = 0f;
    private float _timeRecoiled = 0f;

    public UnitDamage(NavMeshAgent navMeshAgent, Rigidbody rb, Queue<DamageInstance> damageQueue)
    {
        _navMeshAgent = navMeshAgent;
        _rb = rb;
        _damageQueue = damageQueue;
    }

    private void ConsumeDamageQueue(){
        while (_damageQueue.Count > 0)
        {
            DamageInstance damageInstance = _damageQueue.Dequeue();
            _rb.AddForce(Vector3.up * 100); // placeholder
            _timeToRecoil += 0.5f;
        }
    }

    public void OnEnter()
    {
        _navMeshAgent.enabled = false;
        _rb.isKinematic = false;
    }

    public void Tick(){
        ConsumeDamageQueue();
        _timeRecoiled += Time.deltaTime;
        // Something to end state
    }

    public void OnExit(){}
    public void OnCollisionEnter(Collision collision){}
}
