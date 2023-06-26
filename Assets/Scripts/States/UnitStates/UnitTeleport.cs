using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Could make a TeleportData object, but using RallyData feels good

public class UnitTeleport : IState
{
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;
    private Transform _tf;
    private RallyData _rallyData;
    private float tpTime = 2.0f;
    private float timer = 0f;
    private float spinIncr = 500.0f;
    private float currSpin = 0.0f;
    private bool warped = false;

    public UnitTeleport(NavMeshAgent navMeshAgent, Rigidbody rb, Transform tf, RallyData rallyData)
    {
        _navMeshAgent = navMeshAgent;
        _rb = rb;
        _tf = tf;
        _rallyData = rallyData;
    }

    public void Tick()
    {
        if (!warped){
            currSpin += spinIncr * Time.deltaTime;
            _tf.RotateAround(_tf.position, Vector3.up, currSpin * Time.deltaTime);
            timer += Time.deltaTime;
            if (timer > tpTime)
            {
                warped = true;
                // Change location, emit particles, etc.
            }
        } 
        else 
        {
            currSpin -= spinIncr * Time.deltaTime;
            _tf.RotateAround(_tf.position, Vector3.up, currSpin * Time.deltaTime);
        }

    }

    public void OnEnter()
    {
        _navMeshAgent.enabled = true;
        _rb.isKinematic = true;
        warped = false;
        currSpin = 0f;
        timer = 0f;
    }

    public void OnExit(){

    }

    public void OnCollisionEnter(Collision collision){}

}
