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
        void CreateFlatTerrain()
        {
            cubes = new GameObject[fullResolution,fullResolution,1];
            heights = new int[fullResolution,fullResolution];
            for (int i = 0; i < fullResolution; i++){
                for (int j = 0; j < fullResolution; j++){
                    float height = 1;
                    cubes[i,0,j] = Instantiate(cubePrefab, new Vector3((i-fullResolution/2f)*cubeSize, cubeSize * squareSize * height/2, (j-fullResolution/2f)*cubeSize),
                                            Quaternion.identity, this.transform);
                    cubes[i,0,j].transform.localScale = new Vector3(cubeSize*squareSize, cubeSize * squareSize * height, cubeSize * squareSize);
                }
            }
        }

        void CreateNodeTerrain()
        {
            fullResolution = 200;
            offsetXZ = (fullResolution/2f) % 1;
            offsetY = 0.5f;
            cubes = new GameObject[fullResolution, 20, fullResolution];
            ResetHeights();
            List<MacroNode> nodeList = new List<MacroNode>();
            meshList = new List<Mesh>();
            List<CombineInstance> combineList = new List<CombineInstance>();

            // Start node
            MacroNode startNode = new MacroNode(MacroNodeType.StartNode, heights,
                                                new Vector2Int(fullResolution/2 - 10, fullResolution/2 - 10),
                                                new Vector2Int(fullResolution/2 + 10, fullResolution/2 + 10));
            startNode.PopulateGrid();
            startNode.RehydrateMainHeights();
            nodeList.Add(startNode);


            // Get random coord in the dumbest way possible
            // TODO: MUST change this
            bool good = false;
            int nodeX = 0;
            int nodeY = 0;
            while (!good) {
                nodeX = UnityEngine.Random.Range(0, fullResolution);
                nodeY = UnityEngine.Random.Range(0, fullResolution);

                if (((nodeX > 110 && nodeX < 160) || (nodeX < 60 && nodeX > 10)) && ((nodeY < 60 && nodeY > 10) || (nodeY > 110 && nodeY < 160)))
                {
                    good = true;
                }
            }

            // Land node
            int cornerEndX = Mathf.Min(nodeX + 30, fullResolution - 1);
            int cornerEndY = Mathf.Min(nodeY + 30, fullResolution - 1);

            MacroNode landNode = new MacroNode(MacroNodeType.Pillars, heights, new Vector2Int(nodeX,nodeY), new Vector2Int(cornerEndX,cornerEndY));
            landNode.ObscureRandomSubset();
            landNode.PopulateGrid();
            landNode.RehydrateMainHeights();
            
            // Bridge node
            Vector2Int startCorner = new Vector2Int(Mathf.Max(0, Mathf.Min(startNode.GetCenter().x, landNode.GetCenter().x, (landNode.GetCenter().x + startNode.GetCenter().x)/2 - 20)),
                                                    Mathf.Max(0, Mathf.Min(startNode.GetCenter().y, landNode.GetCenter().y, (landNode.GetCenter().y + startNode.GetCenter().y)/2 - 20)));
            Vector2Int endCorner = new Vector2Int(Mathf.Min(fullResolution-1, Mathf.Max(startNode.GetCenter().x, landNode.GetCenter().x, (landNode.GetCenter().x + startNode.GetCenter().x)/2 + 20)),
                                                  Mathf.Min(fullResolution-1, Mathf.Max(startNode.GetCenter().y, landNode.GetCenter().y, (landNode.GetCenter().y + startNode.GetCenter().y)/2 + 20)));
            MacroNode bridgeNode = new MacroNode(MacroNodeType.Bridge, heights, startCorner, endCorner);
            bridgeNode.featureStart = landNode.GetCenter();
            bridgeNode.featureEnd = startNode.GetCenter();
            bridgeNode.PopulateGrid();
            bridgeNode.RehydrateMainHeights();


            // Fill ocean simply
            MacroNode waterNode = new MacroNode(MacroNodeType.Water, heights, new Vector2Int(0, 0), new Vector2Int(fullResolution-1, fullResolution-1));
            waterNode.PopulateGrid();
            waterNode.RehydrateMainHeights();

            GenerateMeshFromHeights();
        }
    
        void GenerateMeshFromHeights()
        {
            Vector3[] vertArray;
            int[] triangleArray;
            List<CombineInstance> combineList = new List<CombineInstance>();
            int density = 10;
            // Create terrain meshes + rigidbodies
            for (int i = 0; i < fullResolution; i++){
                for (int j = 0; j < fullResolution; j++){
                    int height = heights[i, j];
                    if (height > 0){
                        for (int k = -4; k < height + 1; k++)
                        {
                            Vector3 location = new Vector3((i-fullResolution/2f)*cubeSize, cubeSize * squareSize * k - 0.5f, (j-fullResolution/2f)*cubeSize);
                            cubes[i,k+10,j] = Instantiate(cubePrefab, location, Quaternion.identity, this.transform);                               
                            if (k != height)
                            {
                                cubes[i,k+10,j].layer = LayerMask.NameToLayer("NonWalkableTerrain");
                            }

                            // Create mesh
                            Mesh mesh = new Mesh();
                            List<Vector3> vertices = new List<Vector3>();
                            List<int> triangles = new List<int>();
                            CombineInstance combine = new CombineInstance();

                            // Determine which faces of cube to render
                            if (k == height){
                                CubeGenerator.CreateTop(vertices, triangles,density);
                            }
                            if (k == -4){
                                // Not sure if we really ever need this..
                                CubeGenerator.CreateBottom(vertices, triangles, density);
                            }
                            if (i == heights.GetLength(0) - 1 || (i+1 < heights.GetLength(0) && heights[i+1,j] < height))
                            {
                                CubeGenerator.CreateRight(vertices, triangles, density);
                            }
                            if (i == 0 || (i-1 >= 0 && heights[i-1,j] < height))
                            {
                                CubeGenerator.CreateLeft(vertices, triangles, density);
                            }
                            if (j == heights.GetLength(1) - 1 || (j+1 < heights.GetLength(1) && heights[i,j+1] < height))
                            {
                                CubeGenerator.CreateForward(vertices, triangles, density);
                            }
                            if (j == 0 || (j-1 >= 0 && heights[i,j-1] < height))
                            {
                                CubeGenerator.CreateBack(vertices, triangles, density);
                            }

                            
                            vertArray = vertices.ToArray();
                            triangleArray = triangles.ToArray();

                            //Offset mesh by noise
                            float coeff = 1;
                            for (int v = 0; v < vertArray.Length; v++)
                            {
                                if (k==height){
                                    coeff = 1 - vertArray[v].y - 0.5f; 
                                }
                                vertArray[v][0] = vertArray[v][0] + coeff*(Mathf.PerlinNoise((vertArray[v].y + k) * 0.1f, vertArray[v].z + j) * 0.5f - 0.25f);
                                vertArray[v][2] = vertArray[v][2] + coeff*(Mathf.PerlinNoise((vertArray[v].y + k) * 0.1f, vertArray[v].x + i) * 0.5f - 0.25f);
                            }

                            CubeGenerator.RenderMesh(mesh, vertArray, triangleArray);
                            if (k == height){ CubeGenerator.ShrinkMeshTop(mesh); }
                            meshList.Add(mesh);

                            // Memoize mesh to combine later
                            combine.mesh = mesh;
                            combine.transform = cubes[i,k+10,j].transform.localToWorldMatrix;
                            combineList.Add(combine);
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
            combinedMesh.RecalculateNormals();
            combinedMesh.RecalculateTangents();
            combinedMesh.RecalculateBounds();
            combinedMesh.Optimize();
            meshFilter.sharedMesh = combinedMesh;

            CreateGrassBase();
        }

        void CreateGrassBase(){
            // Add grass mesh on top of heights
            GameObject grassObj = new GameObject("GrassObject");
            MeshFilter grassFilter = grassObj.AddComponent<MeshFilter>();
            MeshRenderer grassRenderer = grassObj.AddComponent<MeshRenderer>();
            grassRenderer.material = Resources.Load("Grass") as Material;
            int density = 5;
            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();
            List<Vector3> newVerts = new List<Vector3>();
            List<int> newTris = new List<int>();
            List<Vector3> edgeVerts = new List<Vector3>();
            List<int> edgeTris = new List<int>();
            Vector3 offsetVector = new Vector3();
            for (int i=0; i< heights.GetLength(0); i++)
            {
                for (int j=0; j < heights.GetLength(1); j++)
                {
                    if (heights[i,j] < 1) { continue; }
                    newTris.Clear();
                    newVerts.Clear();
                    edgeVerts.Clear();
                    edgeTris.Clear();
                    offsetVector[0] = (i-fullResolution/2f) * cubeSize;
                    offsetVector[1] = heights[i,j]          * cubeSize;
                    offsetVector[2] = (j-fullResolution/2f) * cubeSize;
                    CubeGenerator.CreateTop(newVerts, newTris, density);

                    bool shorterLeft = (i-1 >= 0 && i-1 < heights.GetLength(0) && heights[i-1, j] < heights[i,j]);
                    bool shorterRight = (i+1 >= 0 && i+1 < heights.GetLength(0) && heights[i+1, j] < heights[i,j]);
                    bool shorterBack = (j-1 >= 0 && j-1 < heights.GetLength(1) && heights[i, j-1] < heights[i,j]);
                    bool shorterForward = (j+1 >= 0 && j+1 < heights.GetLength(1) && heights[i, j+1] < heights[i,j]);

                    // Add edges as needed
                    if (shorterBack){
                        CubeGenerator.CreateBack(edgeVerts, edgeTris, density);
                    }
                    if (shorterForward){
                        CubeGenerator.CreateForward(edgeVerts, edgeTris, density);
                    }
                    if (shorterRight){
                        CubeGenerator.CreateRight(edgeVerts, edgeTris, density);
                    }
                    if (shorterLeft){
                        CubeGenerator.CreateLeft(edgeVerts, edgeTris, density);
                    }

                    // Add edge tris
                    for (int t=0; t< edgeTris.Count; t++){
                        edgeTris[t] += newVerts.Count;
                    }
                    newVerts.AddRange(edgeVerts);
                    newTris.AddRange(edgeTris);

                    Vector3[] newVertsArr = newVerts.ToArray();
                    int[] newTrisArr = newTris.ToArray();

                    // Transform this mesh square
                    float coeff;
                    int k = heights[i,j];
                    for (int t=0; t < newVertsArr.Length; t++)
                    {
                        newVertsArr[t].y = (newVertsArr[t].y/4) + 0.5f * (3f/4f);
                    }

                    if (shorterBack){
                        for (int t=0; t < newVertsArr.Length; t++)
                        {
                            if (newVertsArr[t].z < 0f){
                                coeff = 1f - newVertsArr[t].y - 0.5f;
                                newVertsArr[t].z *= (.2f - Mathf.Abs(newVertsArr[t].y - 0.4f)) + 1f;
                                newVertsArr[t].x = newVertsArr[t].x + coeff*(Mathf.PerlinNoise((newVertsArr[t].y + k) * 0.1f, newVertsArr[t].z + j) * 0.5f - 0.25f);
                                newVertsArr[t].z = newVertsArr[t].z + coeff*(Mathf.PerlinNoise((newVertsArr[t].y + k) * 0.1f, newVertsArr[t].x + i) * 0.5f - 0.25f);
                            }
                        }
                    }
                    if (shorterForward){
                        for (int t=0; t < newVertsArr.Length; t++)
                        {
                            if (newVertsArr[t].z > 0f){
                                coeff = 1f - newVertsArr[t].y - 0.5f;
                                newVertsArr[t].z *= (.2f - Mathf.Abs(newVertsArr[t].y - 0.4f)) + 1f;
                                newVertsArr[t].x = newVertsArr[t].x + coeff*(Mathf.PerlinNoise((newVertsArr[t].y + k) * 0.1f, newVertsArr[t].z + j) * 0.5f - 0.25f);
                                newVertsArr[t].z = newVertsArr[t].z + coeff*(Mathf.PerlinNoise((newVertsArr[t].y + k) * 0.1f, newVertsArr[t].x + i) * 0.5f - 0.25f);
                            }
                        }
                    }
                    if (shorterLeft){
                        for (int t=0; t < newVertsArr.Length; t++)
                        {
                            if (newVertsArr[t].x < 0f){
                                coeff = 1f - newVertsArr[t].y - 0.5f;
                                newVertsArr[t].x *= (.2f - Mathf.Abs(newVertsArr[t].y - 0.4f)) + 1f;
                                newVertsArr[t].x = newVertsArr[t].x + coeff*(Mathf.PerlinNoise((newVertsArr[t].y + k) * 0.1f, newVertsArr[t].z + j) * 0.5f - 0.25f);
                                newVertsArr[t].z = newVertsArr[t].z + coeff*(Mathf.PerlinNoise((newVertsArr[t].y + k) * 0.1f, newVertsArr[t].x + i) * 0.5f - 0.25f);
                            }
                        }
                    }
                    if (shorterRight){
                        for (int t=0; t < newVertsArr.Length; t++)
                        {
                            if (newVertsArr[t].x > 0f){
                                coeff = 1f - newVertsArr[t].y - 0.5f;
                                newVertsArr[t].x *= (.2f - Mathf.Abs(newVertsArr[t].y - 0.4f)) + 1f;
                                newVertsArr[t].x = newVertsArr[t].x + coeff*(Mathf.PerlinNoise((newVertsArr[t].y + k) * 0.1f, newVertsArr[t].z + j) * 0.5f - 0.25f);
                                newVertsArr[t].z = newVertsArr[t].z + coeff*(Mathf.PerlinNoise((newVertsArr[t].y + k) * 0.1f, newVertsArr[t].x + i) * 0.5f - 0.25f);
                            }
                        }
                    }



                    // Offset square
                    offsetVector.x = (i-fullResolution/2f) * cubeSize;
                    offsetVector.y = (heights[i,j] - 0.5f) * cubeSize;
                    offsetVector.z = (j-fullResolution/2f) * cubeSize;
                    for (int t=0; t < newVertsArr.Length; t++)
                    {
                        newVertsArr[t] += offsetVector;
                    }

                    // Aggregate this squares tris
                    for (int t=0; t< newTrisArr.Length; t++){
                        newTrisArr[t] += verts.Count;
                    }
                    verts.AddRange(newVertsArr);
                    tris.AddRange(newTrisArr);
                }
            }

            Mesh grassMesh = new Mesh();
            grassMesh.indexFormat = IndexFormat.UInt32;
            grassMesh.vertices = verts.ToArray();
            grassMesh.triangles = tris.ToArray();
            grassMesh.Optimize();
            grassMesh.RecalculateNormals();
            grassFilter.sharedMesh = grassMesh;
        }
    
        void ResetHeights(){
            heights = new int[fullResolution,fullResolution];
            for (int i=0; i < fullResolution; i++)
            {
                for (int j=0; j < fullResolution; j++)
                {
                    heights[i,j] = MacroNode.UndeterminedHeight;
                }
            }
        }
    }
}
