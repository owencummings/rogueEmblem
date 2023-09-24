using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class NavMeshManager : MonoBehaviour
{

    public NavMeshSurface navSurface;
    private NavMeshData navData;
    private Bounds navBounds;
    private List<NavMeshBuildSource> sources;
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

    public void InitializeNavMesh()
    {
        navData = new NavMeshData();
        NavMesh.AddNavMeshData(navData);
        navBounds = new Bounds(new Vector3(0,0,0), new Vector3(100,100,100));
        sources = new List<NavMeshBuildSource>();
        NavMeshBuilder.CollectSources(navBounds, navSurface.layerMask, navSurface.useGeometry, navSurface.defaultArea, new List<NavMeshBuildMarkup>(), sources);
        NavMeshBuilder.UpdateNavMeshData(navData, navSurface.GetBuildSettings(), sources, navBounds);
        //navData = NavMeshBuilder.BuildNavMeshData(navSurface.GetBuildSettings(), sources, new Bounds(), navSurface.transform.position, Quaternion.identity);
        navSurface.navMeshData = navData;
    }

    //public void BakeNavMesh(){ navSurface.BuildNavMesh(); }

    private IEnumerator UpdateNavMeshCoroutine(Transform root)
    {
        List<NavMeshBuildSource> newSources = new List<NavMeshBuildSource>();
        NavMeshBuilder.CollectSources(root, navSurface.layerMask, navSurface.useGeometry, navSurface.defaultArea, new List<NavMeshBuildMarkup>(), newSources);
        sources.AddRange(newSources);
        AsyncOperation operation = NavMeshBuilder.UpdateNavMeshDataAsync(navData, navSurface.GetBuildSettings(), sources, navBounds);
        while (!operation.isDone)
        {
            yield return new WaitForSeconds(0.016f);
        }
        navSurface.navMeshData = navData;
    }

    public void UpdateNavMesh(Transform root)
    {
        StartCoroutine(UpdateNavMeshCoroutine(root));
    }
}
