using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class LookData{
    public GameObject lookTarget;
    public Vector3 lookLocation;
}

public class UnitLookAt : IState
{
    private NavMeshAgent _navMeshAgent;
    private Transform _tf;
    private LookData _lookData;
    private Vector3 _targetToLook;
    private float spinSpeed = 5f;

    public UnitLookAt(NavMeshAgent navMeshAgent, Transform tf, LookData lookData)
    {
        _navMeshAgent = navMeshAgent;
        _tf = tf;
        _lookData = lookData;
    }

    public void Tick(){
        Vector3 lookAt = Vector3.zero;
        if (_lookData.lookTarget != null)
        {
            lookAt = _lookData.lookTarget.transform.position;
        } 
        else
        {
            lookAt = _lookData.lookLocation;
        }
        _tf.rotation = Quaternion.Slerp(_tf.rotation, Quaternion.LookRotation(lookAt - _tf.position), spinSpeed * Time.deltaTime);
    }

    public void OnEnter()
    {
        _navMeshAgent.enabled = true; // this should always be true
    }

    public void OnExit(){}

    public void OnCollisionEnter(Collision collision){}

}

