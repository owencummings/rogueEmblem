using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TerrainGeneration;

namespace GridSpace{
    public partial class GridManager
    {
        void CreateSquad(GameObject unitPrefab, int x, int z){
            GameObject squad = Resources.Load("Squad") as GameObject;
            squad.GetComponent<Squad>().unitPrefab = unitPrefab;
            LazySlamFeature(squad, x, z);
        }

        void LazySlamFeature(GameObject prefab, int x, int z)
        {
            int outputX = x;
            int outputZ = z;
            int outputY = 0;
            bool found = false;
            int rand;
            if (heights[x, z] != MacroNode.Water && heights[x, z] != MacroNode.UndeterminedHeight && heights[x, z] != MacroNode.ObscuredHeight)
            {
                outputX = x;
                outputZ = z;
                outputY = heights[x,z] + 10;
                if (cubes[outputX, outputY, outputZ] != null) {
                }
                found = true;
            } 
            else 
            {
                int i = 0;
                int tryX = 0;
                int tryZ = 0;
                while (i < 10){
                    rand = UnityEngine.Random.Range(-5, 6);
                    tryX = x + rand;
                    rand = UnityEngine.Random.Range(-5, 6);
                    tryZ = z + rand;
                    if (heights[tryX, tryZ] != MacroNode.Water && heights[tryX, tryZ] != MacroNode.UndeterminedHeight && heights[tryX, tryZ] != MacroNode.ObscuredHeight)
                    {
                        outputX = tryX;
                        outputZ = tryZ;
                        outputY = heights[tryX, tryZ] + 10;
                        if (cubes[outputX, outputY, outputZ] != null) {

                        }
                        found = true;
                        break;
                    } 
                    i += 1;
                }
            }
            if (found)
            {
                Instantiate(prefab, 
                            WorldPointFromGridCoordinate(new Vector3Int(outputX,outputY,outputZ)) + Vector3.up * 0.5f,
                            Quaternion.identity);
            }
        }

        void LazySlamWaterFeature(GameObject prefab, int x, int z)
        {
            int outputX = x;
            int outputZ = z;
            int outputY = 0;
            bool found = false;
            int rand;
            if (heights[x, z] == MacroNode.Water &&
                (heights[x-1,z] >= 0 || heights[x+1,z] >= 0 || heights[x,z-1] >= 0 || heights[x,z+1] >= 0 ))
            {
                outputX = x;
                outputZ = z;
                outputY = heights[x,z];
                found = true;
            } 
            else 
            {
                int i = 0;
                int tryX = 0;
                int tryZ = 0;
                while (i < 100){
                    rand = UnityEngine.Random.Range(-5, 6);
                    tryX = x + rand;
                    rand = UnityEngine.Random.Range(-5, 6);
                    tryZ = z + rand;
                    if (heights[tryX, tryZ] == MacroNode.Water &&
                        (heights[tryX-1,tryZ] >= 0 || heights[tryX+1,tryZ] >= 0 || heights[tryX,tryZ-1] >= 0 || heights[tryX,tryZ+1] >= 0 ))
                    {
                        outputX = tryX;
                        outputZ = tryZ;
                        outputY = heights[tryX, tryZ];
                        found = true;
                        break;
                    } 
                    i += 1;
                }
            }
            if (found)
            {
                Instantiate(prefab, 
                            WorldPointFromGridCoordinate(new Vector3Int(outputX,outputY,outputZ)),
                            Quaternion.identity);
            }
        }

        // TODO: finish smarter feature slamming
        /*
        void SlamFeature2(GameObject prefab, int x, int z)
        {
            // Gets closest square...
            int tryX;
            int tryZ;
            // Find closest appropriate spot
            for (int i=0; i < 5; i += 1){
                for (int j = 0; j < 2*i; j += 1){
                    // Slam it
                    Instantiate(enemyPrefab, 
                                new Vector3(cubes[5,5].transform.position.x, 
                                            cubes[5,5].transform.localScale.y,
                                            cubes[5,5].transform.position.z), 
                                Quaternion.identity);
                    }
            }

        }
        */
    }
}
