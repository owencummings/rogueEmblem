using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// WIP
public class UnitLookAt : IState
{
    private NavMeshAgent _navMeshAgent;
    private Transform _tf;
    private Vector3 _targetToLook;

    public UnitLookAt(NavMeshAgent navMeshAgent, Transform tf)
    {
        _navMeshAgent = navMeshAgent;
        _tf = tf;
    }

    public void Tick(){
        //targetToLook = new Vector3(targetObject.transform.x, 0, targetObject.transform.z);
        _tf.LookAt(_targetToLook);
    }

    public void OnEnter()
    {
        _navMeshAgent.enabled = true;
    }

    public void OnExit(){}

    public void OnCollisionEnter(Collision collision){}

}

