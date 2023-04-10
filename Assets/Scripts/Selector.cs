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
    }

    void Update(){
        Ray ray = gameCam.ScreenPointToRay(Input.mousePosition);
        int gridMask = (1 << (gridLayer-1));
        if (Input.GetKeyDown(KeyCode.Space)){
            // Display selectables
            foreach (ISelectable selectable in selectableSet){
            }
        }

        if (Input.GetMouseButtonDown(0)){
            RaycastHit hit;
            // Deselect current selection
            if (selectedObject != null){
                selectedObject.OnDeselect();
                selectedObject.IsSelected = false;
            }
            selectedObject = null;
            
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~gridMask)){ // Make sure water exists on grid layer
                // Iterate through selectables
                float currDistance;
                float closestDistance = float.MaxValue;
                if (selectableSet.Count > 0){
                    ISelectable closestSelectable = null;
                    foreach (ISelectable selectable in selectableSet){
                        currDistance = Mathf.Pow(hit.transform.position.x - selectable.Position.x, 2)
                                        + Mathf.Pow(hit.transform.position.z - selectable.Position.z, 2);
                        if (currDistance < closestDistance){
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
