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


        public enum NodeAvailability {
            Available, 
            Occupied,
            Unavailable
        } 
        
        static void PropogateNodeAvailability(NodeAvailability[,] unavailableNodes, List<Vector2Int> availableNodes,
                                              Dictionary<Vector2Int,List<Vector2Int>> neighborNodes, Vector2Int node)
        {
            for (int i=-2; i<3; i++)
            {
                for (int j=-2; j<3; j++)
                {
                    if (i + node.x >= 0 && i + node.x < unavailableNodes.GetLength(0) && j + node.y >= 0 && j + node.y < unavailableNodes.GetLength(1))
                    {
                        if(i == -2 || i == 2 || j == -2 || j == 2)
                        {
                            if (unavailableNodes[i+node.x,j+node.y] == NodeAvailability.Unavailable){
                                unavailableNodes[i+node.x, j+node.y] = NodeAvailability.Available;
                                availableNodes.Add(new Vector2Int(node.x + i, node.y + j));
                                if (!neighborNodes.ContainsKey(new Vector2Int(node.x + i, node.y + j)))
                                {
                                    neighborNodes.Add(new Vector2Int(node.x + i, node.y + j), new List<Vector2Int>());
                                }
                                neighborNodes[new Vector2Int(node.x + i, node.y + j)].Add(node);
                            }
                        } else {
                            if (unavailableNodes[i+node.x,j+node.y] == NodeAvailability.Available){ availableNodes.Remove(new Vector2Int(node.x + i, node.y + j)); }
                            unavailableNodes[i+node.x, j+node.y] = NodeAvailability.Occupied;
                        }

                    }
                }
            }
        }

        void CreateNodeTerrain()
        {
            GameObject terrainMeshes = new GameObject("TerrainMeshes");
            GameObject newMesh = null;
            tilesPerMacroTile = 26;
            macroTileResolution = 11;
            fullResolution = tilesPerMacroTile * macroTileResolution;
            List<Vector2Int> availableNodes = new List<Vector2Int>(){ new Vector2Int(5,5) };
            Dictionary<Vector2Int, List<Vector2Int>> neighborNodes = new Dictionary<Vector2Int, List<Vector2Int>>();
            NodeAvailability[,] unavailableNodes = new NodeAvailability[macroTileResolution, macroTileResolution];
                for (int i=0;i<macroTileResolution*macroTileResolution;i++){ unavailableNodes[i%macroTileResolution,i/macroTileResolution]=NodeAvailability.Unavailable; }
            unavailableNodes[5, 5] = NodeAvailability.Available;
            int nodesToBuild = 5;
            TerrainGeneration.MacroNodeType typeToBuild;
            MacroNode currNode = null;
            offsetXZ = (fullResolution/2f) % 1;
            offsetY = 0.5f;
            cubes = new GameObject[fullResolution, 20, fullResolution];
            ResetHeights();
            nodeList = new List<MacroNode>();
            List<CombineInstance> combineList = new List<CombineInstance>();

            for (int curr = 0; curr < nodesToBuild; curr++){
                Vector2Int location = availableNodes[UnityEngine.Random.Range(0, availableNodes.Count)];
                typeToBuild = MacroNode.SimpleNodes[UnityEngine.Random.Range(0, MacroNode.SimpleNodes.Count)];
                if (curr == 0) { typeToBuild = MacroNodeType.Start; }
                Vector2Int startCorner = new Vector2Int(location.x*tilesPerMacroTile, location.y*tilesPerMacroTile);
                Vector2Int endCorner = new Vector2Int((location.x + 1)*tilesPerMacroTile - 1, (location.y + 1)*tilesPerMacroTile - 1);
                currNode = new MacroNode(typeToBuild, heights, startCorner, endCorner);
                currNode.PopulateGrid();
                newMesh = currNode.GenerateMeshFromHeights();
                newMesh.transform.SetParent(terrainMeshes.transform);
                currNode.RehydrateMainHeights();
                nodeList.Add(currNode);
                PropogateNodeAvailability(unavailableNodes, availableNodes, neighborNodes, location);
                if (neighborNodes.ContainsKey(location)){
                    Vector2Int prev = neighborNodes[location][UnityEngine.Random.Range(0, neighborNodes[location].Count)];
                    int prevX = (prev.x * tilesPerMacroTile) + tilesPerMacroTile/2;
                    int prevY = (prev.y * tilesPerMacroTile) + tilesPerMacroTile/2;
                    startCorner = new Vector2Int(Mathf.Max(0, Mathf.Min(prevX, currNode.GetCenter().x, (currNode.GetCenter().x + prevX)/2 - 20)),
                                                 Mathf.Max(0, Mathf.Min(prevY, currNode.GetCenter().y, (currNode.GetCenter().y + prevY)/2 - 20)));
                    endCorner = new Vector2Int(Mathf.Min(fullResolution-1, Mathf.Max(prevX, currNode.GetCenter().x, (currNode.GetCenter().x + prevX)/2 + 20)),
                                               Mathf.Min(fullResolution-1, Mathf.Max(prevY, currNode.GetCenter().y, (currNode.GetCenter().y + prevY)/2 + 20)));
                    MacroNode bridgeNode = new MacroNode(MacroNodeType.Bridge, heights, startCorner, endCorner);
                    bridgeNode.featureStart = currNode.GetCenter();
                    bridgeNode.featureEnd = new Vector2Int(prevX, prevY);
                    bridgeNode.PopulateGrid();
                    newMesh = bridgeNode.GenerateMeshFromHeights();
                    newMesh.transform.SetParent(terrainMeshes.transform);
                    bridgeNode.RehydrateMainHeights();
                    nodeList.Add(bridgeNode);
                }
            }

            StaticBatchingUtility.Combine(terrainMeshes);

            // Fill ocean simply
            MacroNode waterNode = new MacroNode(MacroNodeType.Water, heights, new Vector2Int(0, 0), new Vector2Int(fullResolution-1, fullResolution-1));
            waterNode.PopulateGrid();
            waterNode.RehydrateMainHeights();

            GenerateMeshFromHeights();
            Debug.Log(nodeList.Count);
        }
    
        void GenerateMeshFromHeights()
        {
            // Create terrain meshes + rigidbodies
            for (int i = 0; i < fullResolution; i++){
                for (int j = 0; j < fullResolution; j++){
                    int height = heights[i, j];
                    if (height > 0){
                        for (int k = -4; k < height + 1; k++)
                        {
                            Vector3 location = WorldPointFromGridCoordinate(new Vector3Int(i, k + 10, j));
                            if (k > 0){
                                cubes[i,k+10,j] = Instantiate(cubePrefab, location, Quaternion.identity, this.transform);                               
                                if (k != height)
                                {
                                    cubes[i,k+10,j].layer = LayerMask.NameToLayer("NonWalkableTerrain");
                                }
                            }
                        }
                    }

                }
            }

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
