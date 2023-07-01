using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Vector3Utils;

public class UnitPounce : IState
{
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;
    private Transform _tf;
    private float pounceSpeed = 7f;
    private AttackData _attackData;
    private bool pounced = false;

    public UnitPounce(NavMeshAgent navMeshAgent,  Rigidbody rb, Transform tf, AttackData attackData)
    {
        _navMeshAgent = navMeshAgent;
        _rb = rb;
        _tf = tf;
        _attackData = attackData;
    }

    public void Tick(){
        if (!pounced){
            if (_tf.position.y < _attackData.attackTarget.transform.position.y + 2f){
                _tf.position = _tf.position + Vector3.up * Time.deltaTime * pounceSpeed;
            } else {
                _rb.isKinematic = false;
                _rb.detectCollisions = true;
                _rb.AddForce((_tf.forward * 4 + -_tf.up) * 300f);
                pounced = true;
            }
        }
        Vector3Utils.Vector3UtilsClass.LookAtXZ(_tf, _attackData.attackTarget.transform.position);
    }

    public void OnEnter()
    {
        Debug.Log("Pouncing");
        _navMeshAgent.enabled = false; 
        _rb.isKinematic = true;
        _rb.detectCollisions = false;
    }

    public void OnExit(){ _rb.detectCollisions = true; }

    public void OnCollisionEnter(Collision collision){}

}

