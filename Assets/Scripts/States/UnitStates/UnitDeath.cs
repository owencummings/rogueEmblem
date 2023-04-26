using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitDeath : IState
{
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;
    private GameObject _go;
    private float _deathTime = 2f;
    private float _deathProgress = 0f;

    public UnitDeath(NavMeshAgent navMeshAgent, Rigidbody rb, GameObject go)
    {
        _navMeshAgent = navMeshAgent;
        _rb = rb;
        _go = go;
    }

    public void Tick(){
        //targetToLook = new Vector3(targetObject.transform.x, 0, targetObject.transform.z);
        if (_deathProgress > _deathTime) {
            Object.Destroy(_go);
        }
        _deathProgress += Time.deltaTime;
    }

    public void OnEnter()
    {
        _navMeshAgent.enabled = false;
        _rb.isKinematic = false;
        _rb.constraints = RigidbodyConstraints.None;
        RigidbodyUtils.AddRandomTorque(_rb, 100f);
    }

    public void OnExit(){}

    public void OnCollisionEnter(Collision collision){}

}
