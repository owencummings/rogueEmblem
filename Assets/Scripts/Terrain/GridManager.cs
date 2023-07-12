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
        public float squareSize = 1f;
        public float cubeSize = 1f;

        public int macroTileResolution = 10;
        public int tilesPerMacroTile = 10;
        public int fullResolution = 100;
        
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
            fullResolution = 100; // ??
            fullResolution = macroTileResolution * tilesPerMacroTile;
            meshFilter = GetComponent<MeshFilter>();
            GetComponent<MeshRenderer>().material.color = new Color(0.64f, 0.72f, 0.75f, 1f);
            CreateNodeTerrain();
        }

        void Start(){
            navSurface.BuildNavMesh();
            CreateSquad(Resources.Load("Archer") as GameObject, 50, 50);
            CreateSquad(Resources.Load("Melee") as GameObject, 52, 52);
            LazySlamFeature(Resources.Load("BigEnemy") as GameObject, 33, 33);
            LazySlamFeature(Resources.Load("Wizard") as GameObject, 20, 33);
            LazySlamWaterFeature(Resources.Load("Lurker") as GameObject, 35, 20); 
            LazySlamFeature(Resources.Load("Carryable") as GameObject, 48, 48);
        }

    }
}