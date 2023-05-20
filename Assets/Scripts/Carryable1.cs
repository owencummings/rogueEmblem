using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Carryable1 : MonoBehaviour, ICarryable
{
    // Start is called before the first frame update
    public int Carriers { get; set; }
    public int CarriersNeeded { get; set; }
    public float CarryRadius { get; set; }
    public Vector3[] CarryPivots { get; set; }
    public NavMeshAgent NavAgent { get; set; }

    void Awake(){
        Carriers = 0;
        CarriersNeeded = 3;
        CarryRadius = 0.6f;
        (this as ICarryable).GetCarryPivots();
        Debug.Log(CarryPivots);

        NavAgent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Carriers == CarriersNeeded) {
            // Do the thing
        } else {
            // Don't
        }
    }
}
