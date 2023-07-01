using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AttackData{
    public GameObject attackTarget;
    public bool attackFinished;
    public TeamEnum team;
}

public class UnitAttack : IState
{
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;
    private Transform _tf;
    private AttackData _attackData;
    private float attackCooldown = 0.25f;
    private float attackTime = 0f;

    public UnitAttack(NavMeshAgent navMeshAgent, Rigidbody rb, Transform transform, AttackData attackData)
    {
        _attackData = attackData;
        _navMeshAgent = navMeshAgent;
        _tf = transform;
        _rb = rb;
    }

    public void Tick()
    {
        attackTime += Time.deltaTime;
        if (attackTime > attackCooldown){
            _attackData.attackFinished = true;
        }
    }

    public void OnEnter()
    {
        _navMeshAgent.enabled = false;
        _tf.LookAt(_attackData.attackTarget.transform);
        _rb.isKinematic = false;
        _rb.AddForce((_tf.up * 100 + _tf.forward * 300) * _tf.localScale.magnitude * 1.5f);
        attackTime = 0f;
        _attackData.attackFinished = false;
    }

    public void OnExit()
    {
        _attackData.attackTarget = null;
        _attackData.attackFinished = false;
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            if (damageable.Team ==  _attackData.team) { return; }
            DamageInstance damage = new DamageInstance();
            damage.damageValue = 1;
            damage.sourcePosition = _tf.position;
            damageable.OnDamage(damage);
        }
    }
}
