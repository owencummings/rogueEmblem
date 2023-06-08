using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class NavMeshManager : MonoBehaviour
{

    public NavMeshSurface navSurface;
    public static NavMeshManager Instance { get; private set; }

    private void Awake() 
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

    }


    void Start()
    {
        navSurface.BuildNavMesh();
    }

    public void BakeNavMesh(){ navSurface.BuildNavMesh(); }
    public void UpdateNavMesh(NavMeshData navMeshData){ navSurface.UpdateNavMesh(navMeshData); }
}
