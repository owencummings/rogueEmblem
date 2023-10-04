using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GridSpace {
    public partial class GridManager
    {    
        public Vector3 GetClosestGridPoint(Vector3 inputPoint){
            float outX, outY, outZ;
            // Get nearest point lower than inputPoint
            // gridPointRoundedDown = (inPoint - inpoint%fullResolution) + offset
            outX = (inputPoint.x - inputPoint.x % cubeSize) - offsetXZ;
            outY = (inputPoint.y - inputPoint.y % cubeSize) - offsetY;
            outZ = (inputPoint.z - inputPoint.z % cubeSize) - offsetXZ;

            if (Mathf.Abs(outX - inputPoint.x) > cubeSize/2f){
                outX += cubeSize;
            }
            if (Mathf.Abs(outY - inputPoint.y) > cubeSize/2f){
                outY += cubeSize;
            }
            if (Mathf.Abs(outZ - inputPoint.z) > cubeSize/2f){
                outZ += cubeSize;
            }

            Vector3 outputPoint = new Vector3(outX, outY, outZ);
            return outputPoint;
        }

        public Vector3Int GetGridCoordinatesFromPoint(Vector3 inputPoint)
        {
            Vector3Int gridManagerCoordinates = new Vector3Int((int) (inputPoint.x + fullResolution/2f),
                                                               (int) (inputPoint.y + 10.5f),
                                                               (int) (inputPoint.z + fullResolution/2f));
            return gridManagerCoordinates;
        }

        public Vector3 WorldPointFromGridCoordinate(Vector3Int gridCoord){
            Vector3 worldPoint = new Vector3(gridCoord.x - fullResolution/2f,
                                             gridCoord.y - 10.5f,
                                             gridCoord.z - fullResolution/2f);
            return worldPoint;
        }

        // TODO: Address some bug here
        public Vector3Int GetRandomGridCoordinateInRange(Vector3Int gridCoordinate, int range){
            List<Vector3Int> validCoords = new List<Vector3Int>();
            Vector3Int returnCoord = gridCoordinate;
            for (int i = 0; i < 2*range+1; i += 1)
            {
                for (int j = 0; j < 2*range+1; j += 1)
                {
                    for (int k = 0; k < 2*range+1; k += 1)
                    {
                        Vector3Int nearbyCoord = new Vector3Int(gridCoordinate.x - range + i,
                                                                gridCoordinate.y - range + j,
                                                                gridCoordinate.z - range + k);

                        if (nearbyCoord.x < 0 || nearbyCoord.x >= cubes.GetLength(0)
                            || nearbyCoord.y < 0 || nearbyCoord.y > cubes.GetLength(1) 
                            || nearbyCoord.z < 0 || nearbyCoord.z > cubes.GetLength(2))
                        {
                            // Not within addressable range
                            continue;
                        }
                        if (cubes[nearbyCoord.x,nearbyCoord.y,nearbyCoord.z] != null 
                            && cubes[nearbyCoord.x,nearbyCoord.y+1,nearbyCoord.z] == null)
                        {
                            validCoords.Add(nearbyCoord);
                        }
                    }
                }
            }

            if (validCoords.Count > 0){
                returnCoord =  validCoords[UnityEngine.Random.Range(0, validCoords.Count)];
            }
            return returnCoord;
        }

        public Vector3Int GetRandomGridCoordinateInRange(Vector3 worldPoint, int range){
            Vector3Int gridCoordinate = GetGridCoordinatesFromPoint(worldPoint);
            return GetRandomGridCoordinateInRange(gridCoordinate, range);
        }
    
        public List<Vector3Int> GetNeighborSquares(Vector3Int gridCoordinate){
            List<Vector3Int> neighbors = new List<Vector3Int>();
            List<Vector3Int> directions = new List<Vector3Int>(){
                new Vector3Int(1, 0, 0),
                new Vector3Int(-1, 0, 0),
                new Vector3Int(0, 0, 1),
                new Vector3Int(0, 0, -1)
            };
            Vector3Int dest;
            foreach (Vector3Int d in directions){
                // TODO: Refine this check to account for ramps
                dest = gridCoordinate + d;
                Debug.Log(cubes[dest.x, dest.y + 1, dest.z]);
                if (cubes[dest.x, dest.y, dest.z] != null && cubes[dest.x, dest.y + 1, dest.z] == null){
                    neighbors.Add(dest);
                    Debug.Log("Moving");
                }
            }
            return neighbors;
        }
    }
}
