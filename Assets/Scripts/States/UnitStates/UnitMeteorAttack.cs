using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitMeteorAttack : IState
{
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;
    private Transform _tf;
    private AttackData _attackData;
    private Vector3 _cachedRallyPoint = Vector3.zero;
    private float attackTime = 0f;
    private float attackDuration = 2f;
    private GameObject meteorPrefab = Resources.Load("Meteor") as GameObject;


    public UnitMeteorAttack(NavMeshAgent navMeshAgent, Rigidbody rb, Transform tf, AttackData attackData)
    {
        _navMeshAgent = navMeshAgent;
        _rb = rb;
        _tf = tf;
        _attackData = attackData;
    }

    public void Tick(){
        attackTime += Time.deltaTime;
        if (attackTime > attackDuration)
        {
            _attackData.attackFinished = true;
        }
    }

    public void OnEnter()
    {
        _navMeshAgent.enabled = true;
        _rb.isKinematic = true;
        _tf.LookAt(_attackData.attackTarget.transform);
        _navMeshAgent.SetDestination(_tf.position);
        attackTime = 0f;
        _attackData.attackFinished = false;
        GameObject.Instantiate(meteorPrefab, _tf.position + Vector3.up * 3f, _tf.rotation);

    }

    public void OnExit(){}

    public void OnCollisionEnter(Collision collision){}

}
