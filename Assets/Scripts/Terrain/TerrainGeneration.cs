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
                    if ((i == 0) || (j == 0) || (i == Resolution-1) || (j == Resolution-1))
                    {
                        gridHeights[i,j] = 1;
                    }
                }
            }

            // Populate
            float landPercentage = 0.9f;
            //float neighborWaterPercentage = 0.11f;
            float rand;
            for (int i = 0; i < Resolution; i++)
            {
                for (int j = 0; j < Resolution; j++)
                {
                    if (gridHeights[i,j] == -1)
                    {
                        rand = Random.Range(0f, 1f);
                        // Adjust landPercentage based on neighbor values
                        // Should iterate based on fill-status (random selection from tiles with most neighbors)
                        if (landPercentage > rand){
                            gridHeights[i,j] = 1;
                        } else {
                            gridHeights[i,j] = 0;

                        }
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
            rand = Random.Range(0f, 1f);
            int height = (int)(rand * 3) + 1;
            for (int i = 0; i < Resolution; i++)
            {
                for (int j = 0; j < Resolution; j++)
                {
                    if ((i == 0) || (j == 0) || (i == Resolution-1) || (j == Resolution-1))
                    {
                        rand = Random.Range(0f, 1f);
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
                        rand = Random.Range(0f, 1f);
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
                    if ((i < 2) || (j < 2) || (i > Resolution-3) || (j > Resolution-3))
                    {
                        gridHeights[i,j] = 0;
                    }
                }
            }

            // Populate
            float landPercentage = 0.1f;
            float rand;
            for (int i = 0; i < Resolution; i++)
            {
                for (int j = 0; j < Resolution; j++)
                {
                    if (gridHeights[i,j] == -1)
                    {
                        rand = Random.Range(0f, 1f);
                        if (landPercentage > rand){
                            gridHeights[i,j] = 1;
                        } else {
                            gridHeights[i,j] = 0;
                        }
                    }
                }
            }
        }
    }
}
