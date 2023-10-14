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
    private float attackCooldown = 1f;
    private float attackTime = 0f;
    private GameObject sword;

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
        Debug.Log("Attacking");
        _navMeshAgent.enabled = true;
        _rb.isKinematic = true;
        _navMeshAgent.SetDestination(_tf.position);
        _tf.LookAt(_attackData.attackTarget.transform);
        attackTime = 0f;
        _attackData.attackFinished = false;
        sword = ObjectPool.Instance.GetPooledObject("Sword");
        if (sword.TryGetComponent<Sword>(out Sword swordComponent)){
            swordComponent.team = _attackData.team;
        }
        sword.transform.rotation = _tf.rotation;
        sword.transform.position = _tf.position + _tf.forward * 0.8f + Vector3.up * 0.5f;
    }

    public void OnExit()
    {
        _attackData.attackTarget = null;
        _attackData.attackFinished = false;
        ObjectPool.Instance.Release(sword);
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
