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
    private float spinIncr = 700.0f;
    private float currSpin = 0.0f;
    private float spinDirection;
    private bool warped = false;
    public bool warpDone = false;
    private Vector3 anchor = Vector3.zero;

    public UnitTeleport(NavMeshAgent navMeshAgent, Rigidbody rb, Transform tf, RallyData rallyData)
    {
        _navMeshAgent = navMeshAgent;
        _rb = rb;
        _tf = tf;
        _rallyData = rallyData;
        anchor = tf.position;
    }

    public void Tick()
    {
        if (!warped){
            currSpin += spinIncr * Time.deltaTime;
            _tf.RotateAround(_tf.position, Vector3.up, currSpin * Time.deltaTime * spinDirection);
            timer += Time.deltaTime;
            if (timer > tpTime)
            {
                warped = true;
                Vector3Int randomCoord = GridManager.Instance.GetRandomGridCoordinateInRange(anchor, 6);
                _navMeshAgent.Warp(GridManager.Instance.WorldPointFromGridCoordinate(randomCoord));
                // Change location, emit particles, etc.
                // Probably add offset for navAgent + ground diff 
            }
        } 
        else 
        {
            currSpin -= spinIncr * Time.deltaTime;
            _tf.RotateAround(_tf.position, Vector3.up, currSpin * Time.deltaTime * spinDirection);
            timer += Time.deltaTime;
            if (timer > tpTime * 2)
            {
                warpDone = true;
            }
        }

    }

    public void OnEnter()
    {
        _navMeshAgent.enabled = true;
        _rb.isKinematic = true;
        warped = false;
        currSpin = 0f;
        timer = 0f;
        warpDone = false;
        float[] spinList = new float[]{-1f, 1f};
        spinDirection = spinList[Random.Range(0, spinList.Length)];
    }

    public void OnExit(){

    }

    public void OnCollisionEnter(Collision collision){}

}
