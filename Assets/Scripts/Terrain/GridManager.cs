using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using Unity.Mathematics;
using TerrainGeneration;
using CustomGeometry;
using GridSpace;
using EntitySpawningSpace;

namespace GridSpace {
    [RequireComponent(typeof(MeshFilter))]
    public partial class GridManager : MonoBehaviour
    {
        // Eventually there has to be a LevelBuilder that sets parameters and generates this
        private bool started = false;
        public List<MacroNode> nodeList;
        public NavMeshSurface navSurface;
        public GameObject[,,] cubes;
        public int[,] heights;
        public GameObject[,] features;
        public GameObject cubePrefab;
        public MeshFilter meshFilter;
        public float squareSize = 1f;
        public float cubeSize = 1f;
        public EntitySpawning EntityManager;

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
            fullResolution = macroTileResolution * tilesPerMacroTile;
            meshFilter = GetComponent<MeshFilter>();
            GetComponent<MeshRenderer>().material.color = new Color(0.75f, 0.9f, 0.9f, 1f);
        }

        void Update(){
            if (!started){
                StartCoroutine(GenerateTerrain());
                started = true;
            }
        }

        private IEnumerator GenerateTerrain(){
            EntityManager = new EntitySpawning();
            yield return EntityManager.LoadEntities();
            CreateNodeTerrain();
            NavMeshManager.Instance.InitializeNavMesh();
            UnitAttributes.BirdPalettes.PopulatePalettes();
            CreateSquad(EntitySpawning.EntityLookup["Melee"] as GameObject, fullResolution/2 + 2, fullResolution/2 - 2);
            LazySlamFeature(EntitySpawning.EntityLookup["BigEnemy"] as GameObject, fullResolution/2 - 5, fullResolution/2 - 5);
            LazySlamFeature(EntitySpawning.EntityLookup["Wizard"] as GameObject, fullResolution/2 - 5, fullResolution/2 + 5);
            LazySlamWaterFeature(EntitySpawning.EntityLookup["Lurker"] as GameObject, fullResolution/2+10, fullResolution/2 + 10); 
            LazySlamFeature(EntitySpawning.EntityLookup["Carryable"] as GameObject, fullResolution/2 + 1, fullResolution/2 + 1);
            LazySlamFeature(EntitySpawning.EntityLookup["Food"] as GameObject, fullResolution/2 - 2, fullResolution/2 - 2);
            LazySlamFeature(EntitySpawning.EntityLookup["Ingestor"] as GameObject, fullResolution/2 + 2, fullResolution/2 + 2);
            CreateSquad(EntitySpawning.EntityLookup["Archer"], fullResolution/2, fullResolution/2);
            foreach(MacroNode node in nodeList){
                foreach(EntitySpawn entity in node.NodeEntities){
                    Instantiate(entity.Entity, WorldPointFromGridCoordinate(entity.Position), Quaternion.identity);
                }
            }
        
        }

    }
}