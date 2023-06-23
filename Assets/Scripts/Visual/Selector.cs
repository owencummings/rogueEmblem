using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using Selectable;

public class Selector : MonoBehaviour
{
    public Camera gameCam;
    private int gridLayer = 1;
    private ISelectable selectedObject;
    private MeshRenderer selectedMesh;

    private HashSet<ISelectable> selectableSet;

    public static Selector Instance { get; private set; }
    public GameObject blockPrefab;
    public GameObject rampPrefab;
    private int terrainMask;
    private GameObject go = null;

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

        selectableSet = new HashSet<ISelectable>();

        terrainMask = LayerMask.GetMask("Walkable") + LayerMask.GetMask("NonWalkableTerrain");
        Debug.Log(terrainMask);

    }

    void Update(){
        if (PauseManager.paused) { return ; }

        Ray ray = gameCam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        int gridMask = (1 << (gridLayer-1));

        // Test building feature here for now, will want to add this to a unit behavior
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, terrainMask))
            {
                Vector3 newBlockCoordinates;
                Vector3 newBlockPosition = hit.transform.position + hit.normal;
                if (hit.normal == Vector3.up || hit.normal == Vector3.down){
                    newBlockCoordinates = GridManager.Instance.GetClosestGridPoint(newBlockPosition);
                    go = Instantiate(blockPrefab, newBlockCoordinates, Quaternion.identity, GameManager.Instance.transform);
                } else if (hit.normal == Vector3.right || hit.normal == Vector3.left || hit.normal == Vector3.forward || hit.normal == Vector3.back){
                    newBlockCoordinates = GridManager.Instance.GetClosestGridPoint(newBlockPosition);
                    go = Instantiate(rampPrefab, newBlockCoordinates, Quaternion.LookRotation(hit.normal), GameManager.Instance.transform);
                } else {
                    Vector3 reverseFlatNormal = new Vector3(hit.normal.x, 0, hit.normal.z);
                    reverseFlatNormal = reverseFlatNormal.normalized;
                    reverseFlatNormal = new Vector3(1, -1, reverseFlatNormal.x);
                    reverseFlatNormal *= 90f;
                    reverseFlatNormal -= Vector3.forward * 90f;
                    newBlockCoordinates = GridManager.Instance.GetClosestGridPoint(hit.transform.position);
                    go = Instantiate(rampPrefab, newBlockCoordinates,  Quaternion.Euler(reverseFlatNormal), GameManager.Instance.transform);
                }

                // Make certain blocks non-walkable to improve navmesh compute speed
                Vector3Int gridManagerCoordinates = new Vector3Int((int)newBlockCoordinates.x + GridManager.Instance.macroTileResolution*GridManager.Instance.tilesPerMacroTile/2,
                                                                   (int)newBlockCoordinates.z + GridManager.Instance.macroTileResolution*GridManager.Instance.tilesPerMacroTile/2,
                                                                   (int)newBlockCoordinates.y+10);

                // For ramp-on-ramp
                if (GridManager.Instance.cubes[gridManagerCoordinates.x, gridManagerCoordinates.y, gridManagerCoordinates.z] != null){
                    GridManager.Instance.cubes[gridManagerCoordinates.x, gridManagerCoordinates.y, gridManagerCoordinates.z].layer = LayerMask.NameToLayer("NonWalkableTerrain");
                } 

                // Memoize
                GridManager.Instance.cubes[gridManagerCoordinates.x, gridManagerCoordinates.y, gridManagerCoordinates.z] = go;

                // For potential block underneath
                if (GridManager.Instance.cubes[gridManagerCoordinates.x, gridManagerCoordinates.y, gridManagerCoordinates.z-1] != null){
                    GridManager.Instance.cubes[gridManagerCoordinates.x, gridManagerCoordinates.y, gridManagerCoordinates.z-1].layer = LayerMask.NameToLayer("NonWalkableTerrain");
                } 

                NavMeshManager.Instance.BakeNavMesh();
                /*
                if (go != null){
                    Debug.Log(go);
                    NavMeshData navData = go.GetComponent<NavMeshSurface>().navMeshData;
                    NavMeshManager.Instance.UpdateNavMesh(navData);
                    //go = null;
                }
                */
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Display selectables
            foreach (ISelectable selectable in selectableSet){
                selectable.OnShow();
            }
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            // Hide selectables
            foreach (ISelectable selectable in selectableSet){
                selectable.OnHide();
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            // Deselect current selection
            if (selectedObject != null){
                selectedObject.OnDeselect();
                selectedObject.IsSelected = false;
            }
            selectedObject = null;
            
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~gridMask))
            {   // Make sure water exists on grid layer
                // Iterate through selectables
                // TODO this gridMask does nothing
                float currDistance = 3;
                float closestDistance = float.MaxValue;
                if (selectableSet.Count > 0){
                    ISelectable closestSelectable = null;
                    foreach (ISelectable selectable in selectableSet){
                        currDistance = Mathf.Pow(hit.transform.position.x - selectable.Position.x, 2)
                                        + Mathf.Pow(hit.transform.position.z - selectable.Position.z, 2);
                        if (currDistance < closestDistance && currDistance < 3){
                            closestDistance = currDistance;
                            closestSelectable = selectable;
                        }
                    }
                    if (closestSelectable != null){
                        closestSelectable.OnSelect();
                        selectedObject = closestSelectable;
                        closestSelectable.IsSelected = true;
                    }
                }

            }
        }

        if (Input.GetMouseButtonDown(1) && selectedObject != null)
        {
            SelectableCommand command = new SelectableCommand(KeyCode.Mouse1, ray);
            selectedObject.OnCommand(command);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && selectedObject != null)
        {
            SelectableCommand command = new SelectableCommand(KeyCode.LeftShift, ray);
            selectedObject.OnCommand(command);
        }
    }

    public void AddSelectable(ISelectable selectable){
        selectableSet.Add(selectable);
    }

    public void RemoveSelectable(ISelectable selectable){
        if (selectableSet.Contains(selectable)){
            selectableSet.Remove(selectable);
        }
    }
}
