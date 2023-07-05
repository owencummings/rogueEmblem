using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using Unity.Mathematics;
using TerrainGeneration;
using CustomGeometry;
using GridSpace;

namespace GridSpace {
    [RequireComponent(typeof(MeshFilter))]
    public partial class GridManager : MonoBehaviour
    {
        // Eventually there has to be a LevelBuilder that sets parameters and generates this
        public NavMeshSurface navSurface;
        public GameObject[,,] cubes;
        public int[,] heights;
        public GameObject[,] features;
        public GameObject cubePrefab;
        public List<Mesh> meshList;
        public MeshFilter meshFilter;
        public int gridSize = 20;
        public float squareSize = 1f;
        public float cubeSize = 1f;

        public int macroTileResolution = 10;
        public int tilesPerMacroTile = 10;
        
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
            meshFilter = GetComponent<MeshFilter>();
            CreateMacroTileTerrain();
        }

        void Start(){
            navSurface.BuildNavMesh();
            CreateSquad(Resources.Load("Archer") as GameObject, 15, 15);
            CreateSquad(Resources.Load("Melee") as GameObject, 14, 14);
            LazySlamFeature(Resources.Load("BigEnemy") as GameObject, 23, 23);
            LazySlamFeature(Resources.Load("Wizard") as GameObject, 10, 23);
            LazySlamWaterFeature(Resources.Load("Lurker") as GameObject, 25, 10); 
            LazySlamFeature(Resources.Load("Carryable") as GameObject, 20, 20);
        }

    }
}