using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using Unity.Mathematics;
using TerrainGeneration;

public class GridManager : MonoBehaviour
{
    // Eventually there has to be a LevelBuilder that sets parameters and generates this
    public NavMeshSurface navSurface;
    public GameObject[,,] cubes;
    public int[,] heights;
    public GameObject[,] features;
    public GameObject cubePrefab;
    public int gridSize = 20;
    public float squareSize = 1f;
    public float cubeSize = 1f;

    public int macroTileResolution = 10;
    public int tilesPerMacroTile = 10;

    public GameObject squadPrefab; // temp
    public GameObject enemyPrefab; // temp
    
    public float offsetXZ = 0f;
    public float offsetY = 0.5f;

    public static GridManager Instance { get; private set; }

    void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            Instance = this; 
        } 
        gridSize = 100;
        CreateMacroTileTerrain();
    }

    void Start(){
        navSurface.BuildNavMesh();
        CreateSquad(Resources.Load("Archer") as GameObject, 15, 15);
        CreateSquad(Resources.Load("Melee") as GameObject, 14, 14);
        LazySlamFeature(enemyPrefab, 23, 23);
        LazySlamFeature(Resources.Load("Wizard") as GameObject, 10, 23);
        LazySlamFeature(Resources.Load("Lurker") as GameObject, 25, 10); 
        LazySlamFeature(Resources.Load("Carryable") as GameObject, 20, 20);
    }

    void CreateSquad(GameObject unitPrefab, int x, int z){
        GameObject squad = Resources.Load("Squad") as GameObject;
        squad.GetComponent<Squad>().unitPrefab = unitPrefab;
        LazySlamFeature(squad, x, z);
    }

    public Vector3 GetClosestGridPoint(Vector3 inputPoint){
        float outX, outY, outZ;
        // Get nearest point lower than inputPoint
        // gridPointRoundedDown = (inPoint - inpoint%gridSize) + offset
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
        Vector3Int gridManagerCoordinates = new Vector3Int((int)inputPoint.x + macroTileResolution*tilesPerMacroTile/2,
                                                           (int)inputPoint.y+10,
                                                           (int)inputPoint.z + macroTileResolution*tilesPerMacroTile/2);
        return gridManagerCoordinates;
    }

    public Vector3 WorldPointFromGridCoordinate(Vector3Int gridCoord){
        Vector3 worldPoint = new Vector3(gridCoord.x - macroTileResolution*tilesPerMacroTile/2,
                                         gridCoord.y - 10,
                                         gridCoord.z - macroTileResolution*tilesPerMacroTile/2);
        return worldPoint;
    }

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

    void LazySlamFeature(GameObject prefab, int x, int z)
    {
        int outputX = x;
        int outputZ = z;
        int outputY = 0;
        bool found = false;
        int rand;
        if (heights[x, z] != 0)
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
            while (i < 10){
                rand = UnityEngine.Random.Range(-5, 6);
                tryX = x + rand;
                rand = UnityEngine.Random.Range(-5, 6);
                tryZ = z + rand;
                if (heights[tryX, tryZ] != 0)
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
                        cubes[outputX,outputY,outputZ].transform.position + Vector3.up,
                        Quaternion.identity);
        }
    }

    void LazySlamWaterFeature(GameObject prefab, int x, int z){}

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
                                cubes[gridI,k+10,gridJ] = Instantiate(cubePrefab, new Vector3((gridI-fullResolution/2f)*cubeSize, cubeSize * squareSize * k - 0.5f, (gridJ-fullResolution/2f)*cubeSize),
                                                                           Quaternion.identity, this.transform);
                                if (k != height)
                                {
                                    cubes[gridI,k+10,gridJ].layer = LayerMask.NameToLayer("NonWalkableTerrain");
                                }
                                //cubes[gridI,gridJ,height+10].transform.localScale = new Vector3(cubeSize*squareSize, cubeSize * squareSize * height, cubeSize * squareSize);
                            }
                            heights[gridI, gridJ] = height+10;
                        }

                    }
                }
            }
        }
    }

    
}
