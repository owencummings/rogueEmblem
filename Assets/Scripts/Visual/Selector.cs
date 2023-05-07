using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
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
    private int walkableMask;

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

        walkableMask = LayerMask.GetMask("Walkable");

    }

    void Update(){
        if (PauseManager.paused) { return ; }


        Ray ray = gameCam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        int gridMask = (1 << (gridLayer-1));

        // Test building feature here for now
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, walkableMask))
            {
                Vector3 newBlockPosition = hit.transform.position + hit.normal;
                //Vector3Int roundedPosition = Vector3Int.R
                Instantiate(blockPrefab, GridManager.Instance.GetClosestGridPoint(newBlockPosition), Quaternion.identity, GameManager.Instance.transform);
                NavMeshManager.Instance.BakeNavMesh();
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
