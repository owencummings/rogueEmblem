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
    public TeamEnum team = TeamEnum.Neutral;
    private HashSet<int> objectsHit = new HashSet<int>();


    public float timeToRecoil = 0f;
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
            timeToRecoil += 0.5f;
        }
    }

    public void OnEnter()
    {
        _navMeshAgent.enabled = false;
        _rb.isKinematic = false;
        timeRecoiled = 0f;
        timeToRecoil = 0f;
        objectsHit.Clear();
    }

    public void Tick(){
        ConsumeDamageQueue();
        timeRecoiled += Time.deltaTime;
    }

    public void OnExit(){}

    public void OnCollisionEnter(Collision collision){
        // Spread impact to nearby units
        if (collision.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            if (damageable.Team != team || objectsHit.Contains(damageable.ObjectID) || _rb.velocity.magnitude < 0.1f) { return; }
            DamageInstance damage = new DamageInstance();
            damage.damageValue = 0;
            damage.sourcePosition = _rb.position;
            damage.forceVector = _rb.velocity * _rb.mass * 60f;
            damageable.OnDamage(damage);
            objectsHit.Add(damageable.ObjectID);
        }
    }
}
