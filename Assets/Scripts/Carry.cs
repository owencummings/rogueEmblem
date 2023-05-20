using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Vector3Utils;

public interface ICarryable
{
    int Carriers { get; set; }
    int CarriersNeeded { get; set; }
    NavMeshAgent NavAgent { get; set; }
    float CarryRadius { get; set; }
    Vector3[] CarryPivots {get; set; }
    public void GetCarryPivots()
    {
        CarryPivots =  Vector3UtilsClass.getDestinationCircle(Vector3.down, CarriersNeeded, CarryRadius);
    }
}
