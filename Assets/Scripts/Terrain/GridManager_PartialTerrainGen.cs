using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using TerrainGeneration;
using Unity.Mathematics;
using CustomGeometry;

namespace GridSpace{
    public partial class GridManager
    {
        void CreateRandomTerrain()
        {
            // Assign some oscillation parameters for terrain variation
            float xSinAmp = UnityEngine.Random.Range(0.0f, 2.0f);
            float xSinPeriod = UnityEngine.Random.Range(0.2f, 0.5f);
            float xSinShift = UnityEngine.Random.Range(0.0f, 2*Mathf.PI);
            float ySinAmp = UnityEngine.Random.Range(0.0f, 2.0f);
            float ySinPeriod = UnityEngine.Random.Range(0.2f, 0.5f);
            float ySinShift = UnityEngine.Random.Range(0.0f, 2*Mathf.PI);
            float xCellShift = UnityEngine.Random.Range(0.0f, 100.0f);
            float yCellShift = UnityEngine.Random.Range(0.0f, 100.0f);

            cubes = new GameObject[gridSize,gridSize,1];
            heights = new int[gridSize,gridSize];
            for (int i = 0; i < gridSize; i++){
                for (int j = 0; j < gridSize; j++){

                    // Input into a noise function to see if they should exist...
                    float iScaled = (float)i/(float)gridSize * 4; // 4 is magic number that looks good with this grid size
                    float jScaled =  (float)j/(float)gridSize * 4;
                    // TODO: Add a radial vignette here...
                    if (noise.cellular(new float2(iScaled + xCellShift, jScaled + yCellShift))[0] < 0.3f){
                        continue;
                    }

                    float height = Mathf.Floor(1 + cubeSize *
                                                    (xSinAmp*(1 + Mathf.Sin(xSinPeriod*(i + xSinShift)))
                                                    + ySinAmp*(1 + Mathf.Sin(ySinPeriod*(j + ySinShift)))));
                    cubes[i,0,j] = Instantiate(cubePrefab, new Vector3((i-gridSize/2)*cubeSize, cubeSize * squareSize * height/2, (j-gridSize/2)*cubeSize),
                                            Quaternion.identity, this.transform);
                    cubes[i,0,j].transform.localScale = new Vector3(cubeSize*squareSize, cubeSize * squareSize * height, cubeSize * squareSize);
                }
            }
        }

        void CreateFlatTerrain()
        {
            cubes = new GameObject[gridSize,gridSize,1];
            heights = new int[gridSize,gridSize];
            for (int i = 0; i < gridSize; i++){
                for (int j = 0; j < gridSize; j++){
                    float height = 1;
                    cubes[i,0,j] = Instantiate(cubePrefab, new Vector3((i-gridSize/2f)*cubeSize, cubeSize * squareSize * height/2, (j-gridSize/2f)*cubeSize),
                                            Quaternion.identity, this.transform);
                    cubes[i,0,j].transform.localScale = new Vector3(cubeSize*squareSize, cubeSize * squareSize * height, cubeSize * squareSize);
                }
            }
        }

        void CreateMacroTileTerrain()
        {
            macroTileResolution = 4;
            int fullResolution = macroTileResolution * tilesPerMacroTile;
            offsetXZ = (fullResolution/2f) % 1;
            offsetY = 0.5f; 
            float rand;
            MacroTileType tileType;
            cubes = new GameObject[macroTileResolution*tilesPerMacroTile, 20, macroTileResolution*tilesPerMacroTile];
            heights = new int[macroTileResolution*tilesPerMacroTile,macroTileResolution*tilesPerMacroTile];
            meshList = new List<Mesh>();
            List<CombineInstance> combineList = new List<CombineInstance>();

            for (int i1 = 0; i1 < macroTileResolution; i1++){
                for (int j1 = 0; j1 < macroTileResolution; j1++){

                    // Determine tile type
                    if ((i1 == 0) || (j1 == 0) || (i1 == macroTileResolution-1) || (j1 == macroTileResolution-1))
                    {
                        tileType = MacroTileType.Water;
                    }
                    else 
                    {
                        rand = UnityEngine.Random.Range(0f, 1f);
                        if (rand > 0.7){
                            tileType = MacroTileType.Ring;
                        } else {
                            tileType = MacroTileType.Land;
                        }
                    }

                    // Create tile
                    MacroTile macroTile = new MacroTile(tileType, tilesPerMacroTile);
                    macroTile.PopulateGrid();
                    for (int i = 0; i < tilesPerMacroTile; i++){
                        for (int j = 0; j < tilesPerMacroTile; j++){
                            int gridI = i1 * tilesPerMacroTile + i;
                            int gridJ = j1 * tilesPerMacroTile + j;
                            int height = macroTile.gridHeights[i, j];
                            if (height > 0){
                                for (int k = -4; k < height + 1; k++)
                                {
                                    Vector3 location = new Vector3((gridI-fullResolution/2f)*cubeSize, cubeSize * squareSize * k - 0.5f, (gridJ-fullResolution/2f)*cubeSize);
                                    cubes[gridI,k+10,gridJ] = Instantiate(cubePrefab, location, Quaternion.identity, this.transform);                               
                                    if (k != height)
                                    {
                                        cubes[gridI,k+10,gridJ].layer = LayerMask.NameToLayer("NonWalkableTerrain");
                                    }

                                    // Create mesh
                                    Mesh mesh = new Mesh();
                                    CombineInstance combine = new CombineInstance();
                                    CubeGenerator.CreateCube(mesh);
                                    float randomClamp = UnityEngine.Random.Range(0.0f, 1.0f);
                                    if (randomClamp < 0.1f){
                                        CubeGenerator.ClampMeshTopXZ(mesh);
                                    }
                                    meshList.Add(mesh);

                                    // Memoize mesh to combine later
                                    combine.mesh = mesh;
                                    combine.transform = cubes[gridI,k+10,gridJ].transform.localToWorldMatrix;
                                    combineList.Add(combine);

                                    //cubes[gridI,gridJ,height+10].transform.localScale = new Vector3(cubeSize*squareSize, cubeSize * squareSize * height, cubeSize * squareSize);
                                }
                                heights[gridI, gridJ] = height+10;
                            }

                        }
                    }
                }
            }

            // Combine meshes into one
            // TODO: Ensure this doesnt go over the vert limit (~32k)
            // Under those circumstances, we will crash.
            CombineInstance[] combineArray = new CombineInstance[combineList.Count];
            int index = 0;
            foreach (CombineInstance combInstance in combineList)
            {
                combineArray[index] = combInstance;
                index += 1;
            }

            Mesh combinedMesh = new Mesh();
            combinedMesh.indexFormat = IndexFormat.UInt32;
            combinedMesh.CombineMeshes(combineArray);
            Debug.Log(combinedMesh.vertices.Length);
            meshFilter.sharedMesh = combinedMesh;
        }
    }
}
