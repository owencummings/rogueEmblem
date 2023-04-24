using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class RigidbodyUtils
{
    public static void StandardizeRigidbody(Rigidbody rb)
    {
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.centerOfMass = Vector3.zero;
        rb.inertiaTensor = new Vector3(1, 0, 1);
        rb.inertiaTensorRotation = Quaternion.identity;
    }
}
