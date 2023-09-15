using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainGeneration {

    public enum MacroNodeType {
        Null,
        Land,
        Bridge,
        Water,
        Ring,
        StartNode,
        Featureless,
        Pillars
    }

    public class MacroNode {
        MacroNodeType TileType;
        int[,] GridHeights;
        static int[,] TargetHeights;
        public Vector2Int StartCorner;
        public Vector2Int EndCorner;
        public Vector2Int featureStart;
        public Vector2Int featureEnd;
        public Dictionary<MacroNodeType, Action> tilePopulationMap;

        public const int Water = -1;
        public const int ObscuredHeight = -2;
        public const int UndeterminedHeight = -3;
        public const int Resolved = -1; // For entropy

        #region WFCUtils
        public struct WfcCell
        {
            public WfcCell(int height){
                Height = height;
                Depth = 1;
                Entropy = 0;
            }

            public WfcCell(int height, int depth){
                Height = height;
                Depth = depth;
                Entropy = 0;
            }

            public int Height;
            public int Depth;
            public int Entropy;

            public void AssignValue(int height)
            {
                if (Height != ObscuredHeight) {
                    Height = height;
                    Depth = 1;
                    Entropy = -1;
                }
            }

            public void AssignValue(int height, int depth){
                if (Height != ObscuredHeight) {
                    Height = height;
                    Depth = depth;
                    Entropy = -1;
                }
            }
        }

        public static void ClearWfcCells(WfcCell[,] wfcCells)
        {
            for (int i=0; i < wfcCells.GetLength(0); i++)
            {
                for (int j=0; j < wfcCells.GetLength(1); j++)
                {
                    wfcCells[i,j].Height = 0;
                    wfcCells[i,j].Depth = 1;
                    wfcCells[i,j].Entropy = 0;
                }
            }
        }

        public static void AssignWfcCell(ref WfcCell cell, int val)
        {
            cell.Height = val;
            cell.Entropy = Resolved;
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
                if (n.x >= 0 && n.x < wfcCells.GetLength(0) && n.y >= 0 && n.y < wfcCells.GetLength(1))
                {
                    if (wfcCells[n.x, n.y].Entropy != Resolved)
                    {
                        wfcCells[n.x, n.y].Entropy += wfcCells[x,z].Depth;
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
                        if (n.x >= 0 && n.x < wfcCells.GetLength(0) && n.y >= 0 && n.y < wfcCells.GetLength(1))
                        {
                            if (wfcCells[n.x, n.y].Entropy == Resolved){
                                ent += wfcCells[n.x, n.y].Depth;
                            }
                        }
                    }

                    wfcCells[i,j].Entropy = ent;
                }
            }

        }

        public static void CopyHeightsToWfc(int[,] heights, WfcCell[,] wfcCells)
        {
            for (int i=0; i < heights.GetLength(0); i++)
            {
                for (int j=0; j < heights.GetLength(1); j++)
                {
                    if (heights[i,j] != UndeterminedHeight) {
                        wfcCells[i,j] = new WfcCell(heights[i,j]);
                        wfcCells[i,j].Entropy = -1;
                    }
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
                        outList.Clear();
                        outList.Add(new Vector2Int(i,j));
                    }
                }
            }
            return new Tuple<List<Vector2Int>, int>(outList, maxEntropy);
        }
        
        public static List<WfcCell> GetNeighborCells(WfcCell[,] wfcCells, int x, int z)
        {
            List<Vector2Int> neighbors = new List<Vector2Int>{
                new Vector2Int(x+1, z),
                new Vector2Int(x-1, z),
                new Vector2Int(x, z+1),
                new Vector2Int(x, z-1)
            };
            List<WfcCell> cells = new List<WfcCell>();

            foreach (Vector2Int n in neighbors)
            {
                if (n.x >= 0 && n.x < wfcCells.GetLength(0) && n.y >= 0 && n.y < wfcCells.GetLength(1))
                {
                    if (wfcCells[n.x, n.y].Entropy == Resolved)
                    {
                        cells.Add(wfcCells[n.x, n.y]);
                    }
                }
            }

            return cells;
        }

        #endregion

        public MacroNode(MacroNodeType tileType, int[,] gridHeights, Vector2Int startCorner, Vector2Int endCorner)
        {
            TileType = tileType;
            GridHeights = gridHeights;
            StartCorner = startCorner;
            EndCorner = endCorner;
            TargetHeights = new int[EndCorner.x - StartCorner.x + 1, EndCorner.y - StartCorner.y + 1];
            tilePopulationMap = new Dictionary<MacroNodeType, Action>()
            {
                { MacroNodeType.StartNode, () => PopulateWithWfc(InitializeStart, ResolveStart) },
                { MacroNodeType.Bridge, () => PopulateBridge() },
                { MacroNodeType.Land, () => PopulateLand() },
                { MacroNodeType.Featureless, () => PopulateWithWfc(InitializeFeatureless, ResolveFeatureless) },
                { MacroNodeType.Water, () => PopulateWater() },
                { MacroNodeType.Pillars, () => PopulateWithWfc(InitializePillars, ResolvePillars)}
            };

            HydrateTargetHeights();
        }

        public Vector2Int GetCenter(){
            return new Vector2Int((StartCorner.x + EndCorner.x)/2, (StartCorner.y + EndCorner.y)/2);
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

        public void ObscureRandomSubset(){
            int obscuredAreas = UnityEngine.Random.Range(0, 4);
            Debug.Log(obscuredAreas);
            for (int t=0; t < obscuredAreas; t++){
                int x = UnityEngine.Random.Range(0, 3);
                int y = UnityEngine.Random.Range(0, 3);
                if (x == 1 && y == 1){ continue; }
                for (int i= (x * TargetHeights.GetLength(0))/3; i < ((x+1) * TargetHeights.GetLength(0))/3; i++)
                {
                    for (int j=(y * TargetHeights.GetLength(1))/3; j < ((y+1) * TargetHeights.GetLength(1))/3; j++)
                    {
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
            if (TileType == MacroNodeType.Land || TileType == MacroNodeType.Water || TileType == MacroNodeType.Null){ ObscureExistingData(); }
            tilePopulationMap[TileType]();
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
            int fillHeight = 12;
            int[,] holderHeights = (int[,]) TargetHeights.Clone();
            Vector2Int curr;


            bool connected = false;
            int connectionTries = 0;
            int maxConnectionTries = 10;
            int distance;
            int jumps = 0;
            List<Vector2Int> directions = new List<Vector2Int>(){
                new Vector2Int(1, 0),
                new Vector2Int(-1, 0),
                new Vector2Int(0, 1),
                new Vector2Int(0, -1)
            };
            List<Vector2Int> viableDirections = new List<Vector2Int>();
            Vector2Int chosenD;

            // Make path
        
            while (!connected && connectionTries < maxConnectionTries)
            {
                connectionTries++;
                holderHeights = (int[,]) TargetHeights.Clone();
                holderHeights[featureStart.x - StartCorner.x, featureStart.y - StartCorner.y] = startHeight;
                holderHeights[featureEnd.x - StartCorner.x, featureEnd.y - StartCorner.y] = endHeight;
                jumps = 0;
                Vector2Int currStart = new Vector2Int(featureStart.x - StartCorner.x, featureStart.y - StartCorner.y);
                Vector2Int currEnd = new Vector2Int(featureEnd.x - StartCorner.x, featureEnd.y - StartCorner.y);
                while (!connected && jumps < 1000){
                    // Move start first
                    // Determine a direction to move in
                    viableDirections.Clear();
                    foreach(Vector2Int d in directions){
                        if (currStart.x + d.x*4 > 0 && currStart.x + d.x*4 < holderHeights.GetLength(0) && 
                            currStart.y + d.y*4  > 0 && currStart.y + d.y*4  < holderHeights.GetLength(1) &&
                            holderHeights[currStart.x + d.x*4, currStart.y + d.y*4] != startHeight)
                            {
                                viableDirections.Add(d);
                                // TODO: also check covering of obscured cells
                            } 
                    }

                    //if (viableDirections.Count == 0) { Debug.Log("too far gone"); break; } // Probably need a reset here

                    if (viableDirections.Count > 0)
                    {
                        chosenD = viableDirections[UnityEngine.Random.Range(0, viableDirections.Count)];
                        bool moving = true;
                        int squaresMoved = 0;
                        while (moving){
                            if (currStart.x + chosenD.x < 0 || currStart.x + chosenD.x > holderHeights.GetLength(0) - 1 || 
                                currStart.y + chosenD.y  < 0 || currStart.y + chosenD.y > holderHeights.GetLength(1) - 1 ||
                                holderHeights[currStart.x + chosenD.x, currStart.y + + chosenD.y] == startHeight)
                            { 
                                break; 
                            }
                            else
                            {
                                currStart += chosenD;
                                if (holderHeights[currStart.x, currStart.y] == endHeight) { connected = true; break; }
                                holderHeights[currStart.x, currStart.y] = startHeight;
                                squaresMoved++;
                                if (squaresMoved > 4)
                                {
                                    if (UnityEngine.Random.value < 0.2f){ break; }
                                }
                            } 
                        }
                    }

                    // Move end as well... could generalize approach but a little overcomplicating
                    viableDirections.Clear();
                    foreach(Vector2Int d in directions){
                        if (currEnd.x + d.x*4 > 0 && currEnd.x + d.x*4 < holderHeights.GetLength(0) && 
                            currEnd.y + d.y*4  > 0 && currEnd.y + d.y*4  < holderHeights.GetLength(1) &&
                            holderHeights[currEnd.x + d.x*4, currEnd.y + d.y*4] != endHeight)
                            {
                                viableDirections.Add(d);
                                // TODO: also check covering of obscured cells
                            } 
                    }

                    if (viableDirections.Count > 0)
                    {
                        chosenD = viableDirections[UnityEngine.Random.Range(0, viableDirections.Count)];
                        bool moving = true;
                        int squaresMoved = 0;
                        while (moving){
                            if (currEnd.x + chosenD.x < 0 || currEnd.x + chosenD.x > holderHeights.GetLength(0) - 1 || 
                                currEnd.y + chosenD.y  < 0 || currEnd.y + chosenD.y > holderHeights.GetLength(1) - 1 ||
                                holderHeights[currEnd.x + chosenD.x, currEnd.y + + chosenD.y] == endHeight) 
                            { 
                                break; 
                            }
                            else
                            {
                                currEnd += chosenD;
                                if (holderHeights[currEnd.x, currEnd.y] == startHeight) { connected = true; break; }
                                holderHeights[currEnd.x, currEnd.y] = endHeight;
                                squaresMoved++;
                                if (squaresMoved > 4)
                                {
                                    if (UnityEngine.Random.value < 0.2f){ break; }
                                }
                            } 
                        }
                    }
                    
                    jumps++;
                }
            }

            //TargetHeights = (int[,]) holderHeights.Clone();
            // Create border around path (so that path is 3 squares wide instead of 1)
            List<Vector2Int> neighborDirections = new List<Vector2Int>(){
                new Vector2Int(1, 0),
                new Vector2Int(-1, 0),
                new Vector2Int(0, 1),
                new Vector2Int(0, -1),
                new Vector2Int(1, 1),
                new Vector2Int(-1, -1),
                new Vector2Int(-1, 1),
                new Vector2Int(1, -1)
            };
            Vector2Int neighbor;
            for (int i=0; i<holderHeights.GetLength(0); i += 1)
            {
                for (int j=0; j < holderHeights.GetLength(1); j += 1)
                {
                    if (holderHeights[i,j] == startHeight || holderHeights[i,j] == endHeight)
                    {
                        foreach(Vector2Int direction in neighborDirections)
                        {
                            neighbor = direction + new Vector2Int(i, j);
                            if (neighbor.x >= 0 && neighbor.x < holderHeights.GetLength(0) && 
                                neighbor.y >= 0 && neighbor.y < holderHeights.GetLength(1) && 
                                holderHeights[neighbor.x, neighbor.y] != startHeight && 
                                holderHeights[neighbor.x, neighbor.y] != endHeight)
                            {
                                holderHeights[neighbor.x, neighbor.y] = fillHeight;        
                            }
                        }
                    } 
                }
            }

            // Set path heights to 1
            Debug.Log(jumps);
            for (int i=0; i<holderHeights.GetLength(0); i += 1)
            {
                for (int j=0; j < holderHeights.GetLength(1); j += 1)
                {
                    if (holderHeights[i,j] == startHeight || holderHeights[i,j] == endHeight
                        || holderHeights[i,j] == fillHeight)
                    {
                        holderHeights[i,j] = 1;
                        if (TargetHeights[i,j] < 1){
                            TargetHeights[i,j] = holderHeights[i,j];
                        }
                    } 
                }
            }


        }

        public void PopulateWater(){
            for (int i=0; i < TargetHeights.GetLength(0); i++)
            {
                for (int j=0; j < TargetHeights.GetLength(1); j++)
                {
                    if (TargetHeights[i,j] != ObscuredHeight) {
                        TargetHeights[i,j] = MacroNode.Water;
                    }
                }
            }  
        }
    
        public void PopulateWithWfc(Action<WfcCell[,]> InitializeWfcCells, 
                                    Action<Vector2Int, WfcCell[,]> ResolveCellDecision)
        {
            WfcCell[,] wfcCells = new WfcCell[EndCorner.x - StartCorner.x + 1, EndCorner.y - StartCorner.y + 1];
            ClearWfcCells(wfcCells);
            CopyHeightsToWfc(TargetHeights, wfcCells);

            InitializeWfcCells(wfcCells);
            AssignAllEntropies(wfcCells);

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
                    ResolveCellDecision(chosen,wfcCells);
                    PropogateEntropy(wfcCells, chosen.x, chosen.y);
                    count += 1;
                }
            }
            CopyWfcHeightsToNodeHeights(wfcCells, TargetHeights);
        }

        public WfcCell GetDeepestCell(List<WfcCell> wfcCells){
            WfcCell deepest = wfcCells[0];
            foreach(WfcCell cell in wfcCells){
                if (cell.Depth > deepest.Depth){
                    deepest = cell;
                }
            }
            return deepest;
        }

        public void InitializeStart(WfcCell[,] wfcCells)
        {
            wfcCells[8, 8].AssignValue(4, 3);
            wfcCells[12, 8].AssignValue(4, 3);
            wfcCells[8, 12].AssignValue(4, 3);
            wfcCells[12, 12].AssignValue(4, 3);
            /*
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
            */
        }

        public void ResolveStart(Vector2Int chosen, WfcCell[,] wfcCells)
        {
            List<WfcCell> neighborCells = GetNeighborCells(wfcCells, chosen.x, chosen.y);
            WfcCell deepest = GetDeepestCell(neighborCells);
            int height = 0;
            int depth = 0;
            if (deepest.Depth > 1){
                if (UnityEngine.Random.value > (depth + 10f)/(depth + 11f))
                {
                    height = deepest.Height;
                    depth = deepest.Depth + 1;
                } else {
                    height = deepest.Height;
                    depth = deepest.Depth - 1;
                }
            } else {
                if (UnityEngine.Random.value > 0.5f){
                    height = 1;
                    if (deepest.Height == 1 || deepest.Height == 0) { height = 0; }
                    depth = UnityEngine.Random.Range(2, 5);
                } else {
                    height = deepest.Height;
                    depth = deepest.Depth + 1;
                }
            }
            wfcCells[chosen.x, chosen.y].AssignValue(height, depth);
        }

        public void InitializeFeatureless(WfcCell[,] wfcCells)
        {
            int x2 = wfcCells.GetLength(0)/2;
            int y2 = wfcCells.GetLength(1)/2;
            int x4 = wfcCells.GetLength(0)/4;
            int y4 = wfcCells.GetLength(1)/4;
            int x34 = 3*wfcCells.GetLength(0)/4;
            int y34 = 3*wfcCells.GetLength(1)/4;           
            wfcCells[x2, y2].AssignValue(3, 4);
            /*
            wfcCells[x4, y4].AssignValue(0);
            wfcCells[x34, y4].AssignValue(1, 2);
            wfcCells[x4, y34].AssignValue(1, 2);
            wfcCells[x34, y34].AssignValue(0);
            wfcCells[x4, y4].AssignValue(0);
            wfcCells[x34, y4].AssignValue(0);
            wfcCells[x4, y34].AssignValue(0);
            wfcCells[x34, y34].AssignValue(1, 2);
            wfcCells[x4, y2].AssignValue(1, 2);
            wfcCells[x2, y4].AssignValue(1, 2);
            wfcCells[x2, y34].AssignValue(1, 2);
            wfcCells[x34, y2].AssignValue(1, 2);
            */
        }

        public void ResolveFeatureless(Vector2Int chosen, WfcCell[,] wfcCells)
        {
            List<WfcCell> neighborCells = GetNeighborCells(wfcCells, chosen.x, chosen.y);
            WfcCell deepest = GetDeepestCell(neighborCells);
            int height = 0;
            int depth = 0;
            if (deepest.Depth > 1){
                if (UnityEngine.Random.value > (depth + 2f)/(depth + 3f))
                {
                    height = deepest.Height;
                    depth = deepest.Depth + 1;
                } else if (UnityEngine.Random.value > 0.8f) {
                    height = deepest.Height + 1;
                    depth = UnityEngine.Random.Range(2, 5);
                } else {
                    height = deepest.Height;
                    depth = deepest.Depth - 1;
                }
            } else {
                if (UnityEngine.Random.value > 0.5f){
                    height = deepest.Height - 1;
                    depth = UnityEngine.Random.Range(2, 5);
                } else {
                    height = deepest.Height;
                    depth = deepest.Depth + 1;
                }
            }
            wfcCells[chosen.x, chosen.y].AssignValue(height, depth);
        }

        public void InitializePillars(WfcCell[,] wfcCells)
        {
            int x2 = wfcCells.GetLength(0)/2;
            int y2 = wfcCells.GetLength(1)/2;
            int x4 = wfcCells.GetLength(0)/4;
            int y4 = wfcCells.GetLength(1)/4;
            int x34 = 3*wfcCells.GetLength(0)/4;
            int y34 = 3*wfcCells.GetLength(1)/4;           
            wfcCells[x2, y2].AssignValue(1, 3);
            wfcCells[x4, y4].AssignValue(6, 3);
            wfcCells[x34, y4].AssignValue(6, 3);
            wfcCells[x4, y34].AssignValue(6, 3);
            wfcCells[x34, y34].AssignValue(6, 3);
        }

        public void ResolvePillars(Vector2Int chosen, WfcCell[,] wfcCells)
        {
            List<WfcCell> neighborCells = GetNeighborCells(wfcCells, chosen.x, chosen.y);
            WfcCell deepest = GetDeepestCell(neighborCells);
            int height = 0;
            int depth = 0;
            if (deepest.Depth > 1){
                if (UnityEngine.Random.value > (depth + 2f)/(depth + 3f))
                {
                    height = deepest.Height;
                    depth = deepest.Depth;
                } else {
                    height = deepest.Height;
                    depth = deepest.Depth - 1;
                }
            } else {
                if (UnityEngine.Random.value > 0.5f){
                    height = 1;
                    if (deepest.Height == 1 || deepest.Height == 0) { height = 0; }
                    depth = UnityEngine.Random.Range(2, 5);
                } else {
                    height = deepest.Height;
                    depth = deepest.Depth + 1;
                }
            }
            wfcCells[chosen.x, chosen.y].AssignValue(height, depth);
        }


    }
}
