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

        void CreateMacroTileTerrain()
        {
            macroTileResolution = 6;
            fullResolution = macroTileResolution * tilesPerMacroTile;
            offsetXZ = (fullResolution/2f) % 1;
            offsetY = 0.5f; 
            float rand;
            MacroTileType tileType;
            cubes = new GameObject[fullResolution, 20, fullResolution];
            heights = new int[fullResolution,fullResolution];
            meshList = new List<Mesh>();
            List<CombineInstance> combineList = new List<CombineInstance>();

            for (int i1 = 0; i1 < macroTileResolution; i1++){
                for (int j1 = 0; j1 < macroTileResolution; j1++){
                    // Determine tile type
                    tileType = MacroTileType.Null;
                    if ((i1 == 1) || (j1 == 1) || (i1 == macroTileResolution-2) || (j1 == macroTileResolution-2))
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
                    for (int i2 = 0; i2 < tilesPerMacroTile; i2++)
                    {
                        for (int j2 = 0; j2 < tilesPerMacroTile; j2++)
                        {
                            heights[i1*tilesPerMacroTile + i2, j1*tilesPerMacroTile + j2] = macroTile.gridHeights[i2, j2];
                        }
                    }
                }
            }
            GenerateMeshFromHeights();
        }
    
        void CreateNodeTerrain()
        {
            fullResolution = 100;
            offsetXZ = (fullResolution/2f) % 1;
            offsetY = 0.5f;
            cubes = new GameObject[fullResolution, 20, fullResolution];
            ResetHeights();
            List<MacroNode> nodeList = new List<MacroNode>();
            meshList = new List<Mesh>();
            List<CombineInstance> combineList = new List<CombineInstance>();

            // Start node
            MacroNode startNode = new MacroNode(MacroTileType.StartNode, heights,
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

                if ((nodeX < 10 || nodeX > 50) && (nodeY < 10 || nodeY > 50))
                {
                    good = true;
                }
            }

            // Bridge node
            Vector2Int startCorner = new Vector2Int(Mathf.Max(0, Mathf.Min(50, nodeX) - 10),
                                                    Mathf.Max(0, Mathf.Min(50, nodeY) - 10));
            Vector2Int endCorner = new Vector2Int(Mathf.Min(fullResolution-1, Mathf.Max(50, nodeX) + 10),
                                                  Mathf.Min(fullResolution-1, Mathf.Max(50, nodeY) + 10));
            MacroNode bridgeNode = new MacroNode(MacroTileType.Bridge, heights, startCorner, endCorner);
            bridgeNode.featureStart = new Vector2Int(nodeX, nodeY);
            bridgeNode.featureEnd = new Vector2Int(50, 50);
            bridgeNode.PopulateGrid();
            bridgeNode.RehydrateMainHeights();

            // Land node
            int cornerEndX = Mathf.Min(nodeX + UnityEngine.Random.Range(10, 20), fullResolution - 1);
            int cornerEndY = Mathf.Min(nodeY + UnityEngine.Random.Range(10, 20), fullResolution - 1);

            MacroNode landNode = new MacroNode(MacroTileType.Featureless, heights, new Vector2Int(nodeX,nodeY), new Vector2Int(cornerEndX,cornerEndY));
            landNode.PopulateGrid();
            landNode.RehydrateMainHeights();
            
            // Path node

            GenerateMeshFromHeights();
        }
    
        void GenerateMeshFromHeights()
        {
            Vector3[] vertArray;
            int[] triangleArray;
            List<CombineInstance> combineList = new List<CombineInstance>();
            int density = 5;
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
            Debug.Log(combinedMesh.vertices.Length);
            combinedMesh.Optimize();
            combinedMesh.RecalculateNormals();
            meshFilter.sharedMesh = combinedMesh;

            // Add grass mesh on top
            GameObject grassObj = new GameObject("GrassObject");
            MeshFilter grassFilter = grassObj.AddComponent<MeshFilter>();
            MeshRenderer grassRenderer = grassObj.AddComponent<MeshRenderer>();
            grassRenderer.material = Resources.Load("Grass") as Material;
            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();
            Vector3 offsetVector = new Vector3();
            for (int i=0; i< heights.GetLength(0); i++)
            {
                for (int j=0; j < heights.GetLength(1); j++)
                {
                    if (heights[i,j] > 0)
                    {
                        offsetVector[0] = (i-fullResolution/2f) * cubeSize;
                        offsetVector[1] = heights[i,j]          * cubeSize;
                        offsetVector[2] = (j-fullResolution/2f) * cubeSize;
                        CubeGenerator.CreateTop(verts, tris, density, offsetVector);
                    }
                }
            }
            Mesh grassMesh = new Mesh();
            grassMesh.vertices = verts.ToArray();
            grassMesh.triangles = tris.ToArray();
            Debug.Log(grassMesh.vertices.Length);
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
