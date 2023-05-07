using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    //public void AsyncBakeNavMesh() { navSurface.BuildNavMeshAsync(); }
}
