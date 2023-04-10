using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using Unity.Mathematics;

public class GridManager : MonoBehaviour
{
    // Eventually there has to be a LevelBuilder that sets parameters and generates this
    public NavMeshSurface navSurface;
    GameObject[,] cubes;
    public GameObject[,] features;
    public GameObject cubePrefab;
    public int gridSize = 20;
    public float squareSize = 1f;
    public float cubeSize = 10f;

    public GameObject navAgentPrefab; // temp

    public GameObject squadPrefab; // temp
    
    void Awake()
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

        cubes = new GameObject[gridSize,gridSize];
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
                cubes[i,j] = Instantiate(cubePrefab, new Vector3((i-gridSize/2)*cubeSize, cubeSize * squareSize * height/2, (j-gridSize/2)*cubeSize),
                                         Quaternion.identity, this.transform);
                cubes[i,j].transform.localScale = new Vector3(cubeSize*squareSize, cubeSize * squareSize * height, cubeSize * squareSize);
                //cubes[i,j].GetComponent<NavMeshSurface>().BuildNavMesh();


            }
        }
    }

    void Start(){
        navSurface.BuildNavMesh();

        // Get smarter way to slam features
        if (cubes[0,0] != null){
            Instantiate(squadPrefab, 
                        new Vector3(cubes[0,0].transform.position.x, 
                                    cubes[0,0].transform.localScale.y,
                                    cubes[0,0].transform.position.z), 
                        Quaternion.identity);
        }
        if (cubes[1,1] != null){
            Instantiate(squadPrefab, 
                        new Vector3(cubes[1,1].transform.position.x, 
                                    cubes[1,1].transform.localScale.y,
                                    cubes[1,1].transform.position.z), 
                        Quaternion.identity);
        }
    }
}
