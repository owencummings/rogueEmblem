using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vector3Utils
{
    static class Vector3UtilsClass 
    {
        public static Vector3[] getDestinationCircle(Vector3 center, int pointCount = 3, float radius = 0.3f)
        {
            int shift = Random.Range(0, pointCount);
            Vector3[] circleDestinations = new Vector3[pointCount];

            // If there is only one unit, just place it in the center, don't worry about radius
            if (pointCount == 1){
                circleDestinations[0] = center;
                return circleDestinations;
            }

            // Otherwise, go in circle
            for (int i =0; i < pointCount; i++){
                circleDestinations[i] = new Vector3(center.x + radius * Mathf.Cos(Mathf.PI * 2 * (i + shift)/pointCount),
                                                    center.y,
                                                    center.z + radius * Mathf.Sin(Mathf.PI * 2 * (i + shift)/pointCount));
            }
            return circleDestinations;
        }
    }
}
