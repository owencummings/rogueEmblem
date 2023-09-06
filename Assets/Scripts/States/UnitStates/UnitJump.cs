using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitJump : IState
{
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;
    private Transform _transform;
    private AttackData _attackData;
    private float jumpCooldown = 0.2f;
    private float jumpTime = 0f;
    private Vector3 navMeshVelocity;

    public UnitJump(NavMeshAgent navMeshAgent, Rigidbody rb, Transform transform, AttackData attackData)
    {
        _attackData = attackData;
        _navMeshAgent = navMeshAgent;
        _transform = transform;
        _rb = rb;
    }

    public void Tick()
    {
        jumpTime += Time.deltaTime;
        if (jumpTime > jumpCooldown){
            _attackData.attackFinished = true;
        }
    }

    public void OnEnter()
    {
        navMeshVelocity = new Vector3(_navMeshAgent.velocity.x, 0, _navMeshAgent.velocity.z);
        _navMeshAgent.enabled = false;
        _rb.isKinematic = false;
        _rb.AddForce((_transform.up * 250f + navMeshVelocity * 60f) * _transform.localScale.magnitude);

        jumpTime = 0f;
        _attackData.attackFinished = false;
    }

    public void OnExit()
    {
        _attackData.attackFinished = false;
    }

    public void OnCollisionEnter(Collision collision){}
}
