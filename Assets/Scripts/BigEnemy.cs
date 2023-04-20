using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BigEnemy : MonoBehaviour
{

    private StateMachine _stateMachine;

    // Set up data
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;
    public Vector3 rallyDestination;
    public Vector3 nextDestination;
    public GameObject attackTarget;
    public GameObject nextAttackTarget;

    private int walkableMask;
    private float attackRange = 2f;
    public float timeGrounded = 0f;
    public bool attackFinished = false;
    
    // Start is called before the first frame update
    void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _rb = GetComponent<Rigidbody>();
        _rb.angularDrag = 1f;
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        walkableMask = LayerMask.NameToLayer("Walkable");

        // Set up state machine
        _stateMachine = new StateMachine();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
