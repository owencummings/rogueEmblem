using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainGeneration {

    public enum MacroTileType {
        Null,
        Land,
        Bridge,
        Water,
        Ring,
        StartNode,
    }

    public class MacroTile {

        public MacroTileType TileType;
        public int Resolution;
        public int[,] gridHeights;

        public MacroTile(MacroTileType tileType, int resolution)
        {
            TileType = tileType;
            Resolution = resolution;
        }

        public List<Tuple<int, int>> GetNeighborList(int x, int z)
        {
            List<Tuple<int, int>> outList = new  List<Tuple<int, int>>();
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if ((x+i >= 0) && (z + j >= 0) && (x+i < Resolution) && (z+j < Resolution))
                    {
                        Tuple<int, int> nextTuple = new Tuple<int, int>(x + i, z + j);
                        outList.Add(nextTuple);
                    }
                }
            }
            return outList;
        }

        public void PopulateGrid()
        {
            // Will eventually need a way of linking these enum types together
            if (TileType == MacroTileType.Land)
            {
                PopulateLand();
            } else if (TileType == MacroTileType.Ring){
                PopulateRing();
            }  else if (TileType == MacroTileType.Water){
                PopulateWater();
            } 
        }


        public void PopulateNull(){
            gridHeights = new int[Resolution, Resolution];
            for (int i = 0; i < Resolution; i++)
            {
                for (int j = 0; j < Resolution; j++)
                {
                    gridHeights[i,j] = -1;
                }
            }
        }

        public void PopulateLand()
        {
            PopulateNull();

            // Populate tile-defining features
            for (int i = 0; i < Resolution; i++)
            {
                for (int j = 0; j < Resolution; j++)
                {
                    // This is just the ring...
                    // if ((i == 0) || (j == 0) || (i == Resolution-1) || (j == Resolution-1))
                    gridHeights[i,j] = 1;

                }
            }

            // Populate lakes
            int numIslands = UnityEngine.Random.Range(0,4);
            for (int island = 0; island < numIslands; island++)
            {
                // Get location and island size
                int islandLength = UnityEngine.Random.Range(2,5);
                int islandWidth = UnityEngine.Random.Range(2,5);
                int islandX = UnityEngine.Random.Range(0,Resolution - islandLength);
                int islandZ = UnityEngine.Random.Range(0, Resolution - islandWidth);
                for (int i = 0; i < islandLength; i++)
                {
                    for (int j = 0; j < islandWidth; j++)
                    {
                        gridHeights[islandX + i, islandZ + j] = 0;
                    }
                }
            }
        }

        public void PopulateRing()
        {
            float rand;
            float ringWaterPercentage = 0.05f;
            PopulateNull();

            // Populate tile-defining features
            rand = UnityEngine.Random.Range(0f, 1f);
            int height = (int)(rand * 3) + 1;
            for (int i = 0; i < Resolution; i++)
            {
                for (int j = 0; j < Resolution; j++)
                {
                    if ((i == 0) || (j == 0) || (i == Resolution-1) || (j == Resolution-1))
                    {
                        rand = UnityEngine.Random.Range(0f, 1f);
                        gridHeights[i,j] = rand > ringWaterPercentage ? 1 : 0;
                    } 
                    else if ((i > Resolution/2 - 3 && i < Resolution/2 + 2) && (j > Resolution/2 - 3 && j < Resolution/2 + 2))
                    {
                        gridHeights[i,j] = height;
                    }
                }
            }

            // Populate
            float landPercentage = 0.065f;
            for (int i = 0; i < Resolution; i++)
            {
                for (int j = 0; j < Resolution; j++)
                {
                    if (gridHeights[i,j] == -1)
                    {
                        rand = UnityEngine.Random.Range(0f, 1f);
                        if (landPercentage > rand){
                            gridHeights[i,j] = 1;
                        } else {
                            gridHeights[i,j] = 0;

                        }
                    }
                }
            }
        }

        public void PopulateWater()
        {
            PopulateNull();

            // Populate tile-defining features
            for (int i = 0; i < Resolution; i++)
            {
                for (int j = 0; j < Resolution; j++)
                {
                    gridHeights[i,j] = 0;
                }
            }


            // Do no islands fairly frequently
            float rand = UnityEngine.Random.Range(0f, 1f);
            if (rand > 0.33){ return; }

            // Populate islands
            int numIslands = UnityEngine.Random.Range(0,4);
            for (int island = 0; island < numIslands; island++)
            {
                // Get location and island size
                int islandLength = UnityEngine.Random.Range(2,5);
                int islandWidth = UnityEngine.Random.Range(2,5);
                int islandX = UnityEngine.Random.Range(2,Resolution - 2 - islandLength);
                int islandZ = UnityEngine.Random.Range(2, Resolution - 2 - islandWidth);
                int islandHeight = UnityEngine.Random.Range(1, 5);

                for (int i = 0; i < islandLength; i++)
                {
                    for (int j = 0; j < islandWidth; j++)
                    {
                        gridHeights[islandX + i, islandZ + j] = islandHeight;
                    }
                }
            }
        }
    }

    public class MacroNode {
        MacroTileType TileType;
        int[,] GridHeights;
        static int[,] TargetHeights;
        Vector2Int StartCorner;
        Vector2Int EndCorner;
        public Vector2Int featureStart;
        public Vector2Int featureEnd;
        public Dictionary<MacroTileType, Action> tilePopulationMap;

        public const int Water = -1;
        public const int ObscuredHeight = -2;
        public const int UndeterminedHeight = -3;

        #region WFCUtils
        public struct WfcCell
        {
            public WfcCell(int height){
                Height = height;
                Entropy = 0;
            }

            public int Height;
            public int Entropy;

            public void AssignValue(int val)
            {
                Height = val;
                Entropy = -1;
            }
        }

        public static void ClearWfcCells(WfcCell[,] wfcCells)
        {
            for (int i=0; i < wfcCells.GetLength(0); i++)
            {
                for (int j=0; j < wfcCells.GetLength(1); j++)
                {
                    wfcCells[i,j].Height = 0;
                    wfcCells[i,j].Entropy = 0;
                }
            }
        }

        public static void AssignWfcCell(ref WfcCell cell, int val)
        {
            cell.Height = val;
            cell.Entropy = -1;
        }

        public static void PropogateEntropy(WfcCell[,] wfcCells, int x, int z)
        {
            List<Vector2Int> neighbors = new List<Vector2Int>{
                new Vector2Int(x+1, z),
                new Vector2Int(x-1, z),
                new Vector2Int(x, z+1),
                new Vector2Int(x, z-1)
            };

            foreach (Vector2Int n in neighbors)
            {
                if (n.x > 0 && n.x < wfcCells.GetLength(0) && n.y > 0 && n.y < wfcCells.GetLength(1))
                {
                    if (wfcCells[n.x, n.y].Entropy != -1)
                    {
                        wfcCells[n.x, n.y].Entropy += 1;
                    }
                }
            }
        }

        public static void AssignAllEntropies(WfcCell[,] wfcCells)
        {
            int ent;
            List<Vector2Int> neighbors;
            for (int i=0; i < wfcCells.GetLength(0); i++)
            {
                for (int j=0; j < wfcCells.GetLength(1); j++)
                {
                    if (wfcCells[i,j].Entropy == -1){ continue; }

                    neighbors = new List<Vector2Int>{
                        new Vector2Int(i+1, j),
                        new Vector2Int(i-1, j),
                        new Vector2Int(i, j+1),
                        new Vector2Int(i, j-1)
                    };
                    ent = 0;

                    foreach (Vector2Int n in neighbors)
                    {
                        if (n.x > 0 && n.x < wfcCells.GetLength(0) && n.y > 0 && n.y < wfcCells.GetLength(1))
                        {
                            if (wfcCells[n.x, n.y].Entropy == -1){
                                ent += 1;
                            }
                        }
                    }

                    wfcCells[i,j].Entropy = ent;
                }
            }

        }

        public static void CopyWfcHeightsToNodeHeights(WfcCell[,] wfcCells, int[,] heights)
        {
            for (int i=0; i < wfcCells.GetLength(0); i++)
            {
                for (int j=0; j < wfcCells.GetLength(1); j++)
                {
                    heights[i,j] = wfcCells[i,j].Height;
                }
            }
        }
        
        public static Tuple<List<Vector2Int>, int> GetHighestEntropy(WfcCell[,] wfcCells)
        {
            int maxEntropy = 0;
            List<Vector2Int> outList = new List<Vector2Int>();
            for (int i=0; i < wfcCells.GetLength(0); i++)
            {
                for (int j=0; j < wfcCells.GetLength(1); j++)
                {
                    if (wfcCells[i,j].Entropy == maxEntropy) {
                        outList.Add(new Vector2Int(i,j));
                    }
                    if (wfcCells[i,j].Entropy > maxEntropy) {
                        maxEntropy = wfcCells[i,j].Entropy;
                        outList = new List<Vector2Int>();
                        outList.Add(new Vector2Int(i,j));
                    }
                }
            }
            return new Tuple<List<Vector2Int>, int>(outList, maxEntropy);
        }
        
        public static List<int> GetNeighborHeights(WfcCell[,] wfcCells, int x, int z)
        {
            List<Vector2Int> neighbors = new List<Vector2Int>{
                new Vector2Int(x+1, z),
                new Vector2Int(x-1, z),
                new Vector2Int(x, z+1),
                new Vector2Int(x, z-1)
            };
            List<int> heights = new List<int>();

            foreach (Vector2Int n in neighbors)
            {
                if (n.x > 0 && n.x < wfcCells.GetLength(0) && n.y > 0 && n.y < wfcCells.GetLength(1))
                {
                    if (wfcCells[n.x, n.y].Entropy == -1)
                    {
                        heights.Add(wfcCells[n.x, n.y].Height);
                    }
                }
            }

            return heights;
        }

        #endregion

        public MacroNode(MacroTileType tileType, int[,] gridHeights, Vector2Int startCorner, Vector2Int endCorner)
        {
            TileType = tileType;
            GridHeights = gridHeights;
            StartCorner = startCorner;
            EndCorner = endCorner;
            TargetHeights = new int[EndCorner.x - StartCorner.x + 1, EndCorner.y - StartCorner.y + 1];
            tilePopulationMap = new Dictionary<MacroTileType, Action>()
            {
                { MacroTileType.StartNode, () => PopulateStartIsland() },
                { MacroTileType.Bridge, () => PopulateBridge() },
                { MacroTileType.Land, () => PopulateLand() }
            };

            HydrateTargetHeights();
            ObscureExistingData();
        }

        public void ObscureExistingData(){
            for (int i=0; i < TargetHeights.GetLength(0); i++)
            {
                for (int j=0; j < TargetHeights.GetLength(1); j++)
                {
                    if (TargetHeights[i,j] != UndeterminedHeight) {
                        TargetHeights[i,j] = ObscuredHeight;
                    }
                }
            }
        }

        public void HydrateTargetHeights()
        {
            for (int i=0; i < TargetHeights.GetLength(0); i++)
            {
                for (int j=0; j < TargetHeights.GetLength(1); j++)
                {
                    TargetHeights[i, j] = GridHeights[StartCorner.x + i, StartCorner.y + j];
                }
            } 
        }

        public void RehydrateMainHeights()
        {
            for (int i=0; i < TargetHeights.GetLength(0); i++)
            {
                for (int j=0; j < TargetHeights.GetLength(1); j++)
                {
                    if (TargetHeights[i,j] != ObscuredHeight) {
                        GridHeights[StartCorner.x + i, StartCorner.y + j] = TargetHeights[i,j];
                    }
                }
            } 
        }

        public void PopulateGrid()
        {
            tilePopulationMap[TileType]();
        }

        public void PopulateStartIsland()
        {
            WfcCell[,] wfcCells = new WfcCell[EndCorner.x - StartCorner.x + 1, EndCorner.y - StartCorner.y + 1];
            ClearWfcCells(wfcCells);

            // Some seeding values to start building from
            wfcCells[8, 8].AssignValue(4);
            wfcCells[12, 8].AssignValue(4);
            wfcCells[8, 12].AssignValue(4);
            wfcCells[12, 12].AssignValue(4);
            wfcCells[5, 5].AssignValue(1);
            wfcCells[15, 5].AssignValue(1);
            wfcCells[5, 15].AssignValue(1);
            wfcCells[15, 15].AssignValue(1);
            wfcCells[2, 2].AssignValue(0);
            wfcCells[18, 2].AssignValue(0);
            wfcCells[2, 18].AssignValue(0);
            wfcCells[18, 18].AssignValue(0);
            wfcCells[1, 10].AssignValue(0);
            wfcCells[10, 1].AssignValue(0);
            wfcCells[10, 19].AssignValue(0);
            wfcCells[19, 10].AssignValue(0);
            AssignAllEntropies(wfcCells);

            // Can probably generalize this a little bit...
            bool collapsing = true;
            int count = 0;
            while (collapsing)
            {
                Tuple<List<Vector2Int>, int> maxEntropyResult = GetHighestEntropy(wfcCells);
                if (maxEntropyResult.Item2 == 0 || maxEntropyResult.Item1.Count == 0 || count > 1000)
                {
                    collapsing = false;
                } else {
                    int rand = UnityEngine.Random.Range(0, maxEntropyResult.Item1.Count);
                    Vector2Int chosen = maxEntropyResult.Item1[rand];
                    List<int> neighborHeights = GetNeighborHeights(wfcCells, chosen.x, chosen.y);
                    //neighborHeights.AddRange(neighborHeights);
                    //neighborHeights.Add(1);
                    wfcCells[chosen.x, chosen.y].AssignValue(neighborHeights[UnityEngine.Random.Range(0,neighborHeights.Count)]);
                    PropogateEntropy(wfcCells, chosen.x, chosen.y);
                    count += 1;
                }
            }

            CopyWfcHeightsToNodeHeights(wfcCells, TargetHeights);
        }
    
        public void PopulateLand()
        {
            for (int i=0; i < TargetHeights.GetLength(0); i++)
            {
                for (int j=0; j < TargetHeights.GetLength(1); j++)
                {
                    if (TargetHeights[i,j] != ObscuredHeight) {
                        TargetHeights[i,j] = 1;
                    }
                }
            }  
        }

        public void PopulateBridge()
        {
            // Ugly function, still WIP
            int startHeight = 10;
            int endHeight = 11;
            TargetHeights[featureStart.x - StartCorner.x, featureStart.y - StartCorner.y] = startHeight;
            TargetHeights[featureEnd.x - StartCorner.x, featureEnd.y - StartCorner.y] = endHeight;

            Vector2Int currStart = new Vector2Int(featureStart.x - StartCorner.x, featureStart.y - StartCorner.y);
            Vector2Int currEnd = new Vector2Int(featureEnd.x - StartCorner.x, featureEnd.y - StartCorner.y);

            bool connected = false;
            int distance;
            List<int> distanceChoices = new List<int>(){1, 2, 3, 3, 3, 4};
            List<Vector2Int> directions = new List<Vector2Int>(){
                new Vector2Int(1, 0),
                new Vector2Int(-1, 0),
                new Vector2Int(0, 1),
                new Vector2Int(0, -1)
            };
            List<Vector2Int> viableDirections;
            Vector2Int chosenDirection;
            int jumps = 0;

            while (!connected && jumps < 1000){
                // Move start first
                distance = distanceChoices[UnityEngine.Random.Range(0, distanceChoices.Count)];
                viableDirections = new List<Vector2Int>();
                foreach(Vector2Int d in directions){
                    if (currStart.x + d.x*distance > 0 && currStart.x + d.x*distance < TargetHeights.GetLength(0) && 
                        currStart.y + d.y*distance  > 0 && currStart.y + d.y*distance  < TargetHeights.GetLength(1) &&
                        TargetHeights[currStart.x + d.x*distance, currStart.y + d.y*distance] != startHeight)
                        {
                            viableDirections.Add(d);
                            // TODO: also check covering of obscured cells
                        } 
                }

                if (viableDirections.Count > 0)
                {
                    Debug.Log("viable");
                    chosenDirection = viableDirections[UnityEngine.Random.Range(0, viableDirections.Count)];
                    for (int i=1; i<=distance; i++)
                    {
                        if (TargetHeights[currStart.x + chosenDirection.x*i, currStart.y + chosenDirection.y*i] == endHeight)
                        {
                            connected = true;
                            break;
                        }
                        TargetHeights[currStart.x + chosenDirection.x*i, currStart.y + chosenDirection.y*i] = startHeight;
                    }
                    currStart = new Vector2Int(currStart.x + chosenDirection.x, currStart.y + chosenDirection.y);
                }


                // Then do the same for end
                distance = distanceChoices[UnityEngine.Random.Range(0, distanceChoices.Count)];
                directions = new List<Vector2Int>(){
                    new Vector2Int(1, 0),
                    new Vector2Int(-1, 0),
                    new Vector2Int(0, 1),
                    new Vector2Int(0, -1)
                };
                viableDirections = new List<Vector2Int>();
                foreach(Vector2Int d in directions){
                    if (currEnd.x + d.x*distance > 0 && currEnd.x + d.x*distance < TargetHeights.GetLength(0) && 
                        currEnd.y + d.y*distance  > 0 && currEnd.y + d.y*distance  < TargetHeights.GetLength(1) &&
                        TargetHeights[currEnd.x + d.x*distance, currEnd.y + d.y*distance] != endHeight)
                        {
                            viableDirections.Add(d);
                            // TODO: also check covering of obscured cells
                        } 
                }

                if (viableDirections.Count > 0)
                {
                    chosenDirection = viableDirections[UnityEngine.Random.Range(0, viableDirections.Count)];
                    for (int i=1; i<=distance; i++)
                    {
                        if (distance == 4 & i != distance) { continue; }
                        if (TargetHeights[currEnd.x + chosenDirection.x*i, currEnd.y + chosenDirection.y*i] == startHeight)
                        {
                            connected = true;
                            break;
                        }
                        TargetHeights[currEnd.x + chosenDirection.x*i, currEnd.y + chosenDirection.y*i] = endHeight;
                    }
                    currEnd = new Vector2Int(currEnd.x + chosenDirection.x, currEnd.y + chosenDirection.y);
                }

                jumps += 1;
            }

            // Set path heights to 1
            Debug.Log(jumps);
            for (int i=0; i<TargetHeights.GetLength(0); i += 1)
            {
                for (int j=0; j < TargetHeights.GetLength(1); j += 1)
                {
                    if (TargetHeights[i,j] == startHeight || TargetHeights[i,j] == endHeight)
                    {
                        TargetHeights[i,j] = 1;
                    } 
                }
            }
        }
    }
}