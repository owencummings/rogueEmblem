using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitArcherAttack : IState
{
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;
    private Transform _transform;
    private AttackData _attackData;
    private float attackCooldown = 1f;
    private float attackTime = 0f;
    private GameObject arrowPrefab = Resources.Load("Arrow") as GameObject;

    public static float attackRange = 4f;

    public UnitArcherAttack(NavMeshAgent navMeshAgent, Rigidbody rb, Transform transform, AttackData attackData)
    {
        _attackData = attackData;
        _navMeshAgent = navMeshAgent;
        _transform = transform;
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
        _navMeshAgent.enabled = true;
        _rb.isKinematic = true;
        _navMeshAgent.velocity = Vector3.zero;
        _transform.LookAt(_attackData.attackTarget.transform);
        _navMeshAgent.SetDestination(_transform.position);
        attackTime = 0f;
        _attackData.attackFinished = false;
        GameObject.Instantiate(arrowPrefab, _transform.position + Vector3.up, _transform.rotation);
    }

    public void OnExit()
    {
        _attackData.attackTarget = null;
        _attackData.attackFinished = false;
    }

    public void OnCollisionEnter(Collision collision){}
}
