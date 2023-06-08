using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitDamage : IState
{

    private UnityEngine.AI.NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;
    private Queue<DamageInstance> _damageQueue;
    private IDamageable _damageable;

    private float _timeToRecoil = 0f;
    public float timeRecoiled = 0f;

    public UnitDamage(NavMeshAgent navMeshAgent, Rigidbody rb, Queue<DamageInstance> damageQueue, IDamageable damageable)
    {
        _navMeshAgent = navMeshAgent;
        _rb = rb;
        _damageQueue = damageQueue;
        _damageable = damageable;
    }

    private void ConsumeDamageQueue()
    {
        while (_damageQueue.Count > 0)
        {
            DamageInstance damageInstance = _damageQueue.Dequeue();
            _rb.AddForce(damageInstance.forceVector);
            _damageable.Health -= damageInstance.damageValue;
            _timeToRecoil += 0.5f;
        }
    }

    public void OnEnter()
    {
        _navMeshAgent.enabled = false;
        _rb.isKinematic = false;
        timeRecoiled = 0f;
    }

    public void Tick(){
        ConsumeDamageQueue();
        timeRecoiled += Time.deltaTime;
    }

    public void OnExit(){}
    public void OnCollisionEnter(Collision collision){}
}
