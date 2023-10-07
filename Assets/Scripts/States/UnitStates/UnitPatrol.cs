using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using GridSpace;


public class UnitPatrol : IState
{
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;
    private Transform _tf;
    private Vector3 anchor;
    private float timeToMove = 0f;
    private float timer = 0f;
    private int walkableMask = LayerMask.GetMask("Walkable");
    private GridManager gridManager = GridManager.Instance;
    private Vector3 destination;
    private int moves;
    private Vector3 biasD;


    public UnitPatrol(NavMeshAgent navMeshAgent, Rigidbody rb, Transform tf)
    {
        _navMeshAgent = navMeshAgent;
        _rb = rb;
        _tf = tf;
        anchor = tf.position;
    }

    public void Tick(){
        // Check time since moving
        timer += Time.deltaTime;
        if (moves > 0 && timer > timeToMove){
            if ((destination - _tf.position).sqrMagnitude < 0.2f){
                RaycastHit hit;
                // Note: raycast must start outside the collider and actually go through it. Dumb as hell.
                // TODO: handle angled tiles as neighbors
                if (Physics.Raycast(_tf.position + 0.5f*Vector3.up, Vector3.down, out hit, walkableMask)){
                    Vector3Int currentSquare = gridManager.GetGridCoordinatesFromPoint(hit.transform.position);
                    List<Vector3Int> neighbors = gridManager.GetNeighborSquares(currentSquare);
                    if (neighbors.Count > 0){
                        destination = gridManager.WorldPointFromGridCoordinate(neighbors[UnityEngine.Random.Range(0, neighbors.Count)]) + 0.5f * Vector3.up;
                        _navMeshAgent.SetDestination(destination);
                        moves--;
                        timer = 0f;
                    } else {
                        moves = 0;
                        timer = 0f;
                    }
                } 

            }
        } else {
            if (timer == 0f){ timeToMove = UnityEngine.Random.Range(2f, 5f);} 
            if (moves == 0){ moves = UnityEngine.Random.Range(3, 10); }
            timer += Time.deltaTime;
        }
    }

    public void OnEnter()
    {
        _navMeshAgent.enabled = true;
        _rb.isKinematic = true;
        Debug.Log("Patrolling");
        walkableMask = LayerMask.GetMask("Walkable");
        moves = 0;
        timer = 0f;
        timeToMove = UnityEngine.Random.Range(2f, 5f);
        destination = _tf.position;
        // TODO: Maybe change agent speed to be slower?
    }

    public void OnExit(){}

    public void OnCollisionEnter(Collision collision){}

}