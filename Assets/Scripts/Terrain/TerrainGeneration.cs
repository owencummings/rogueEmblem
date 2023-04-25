using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainGeneration {
    public enum MacroTileType {
        Land,
        Bridge,
        Water,
        Ring
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
}
