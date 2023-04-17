using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Selectable;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;
using UnitCommands;

public class Squad : MonoBehaviour, ISelectable
{
    // Start is called before the first frame update
    private int maxUnits = 3;
    public Unit[]  unitArr;
    public GameObject [] unitGoArr;

    public GameObject unitPrefab;
    public Vector3 selectableLocation;
    public Vector3 rallyLocation;

    public GameObject rallyFlagPrefab;
    public GameObject rallyFlag;

    public DecalProjector decalProjector;

    private Color squadColor;
    float currOpacity = 0f;

    public bool isDisplaying;

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => _isSelected = value;
    }

    public Vector3 Position
    {
        get => transform.position;
    }

    void Awake()
    {
        (this as ISelectable).SubscribeToSelector();

        unitGoArr = new GameObject[maxUnits];
        unitArr = new Unit[maxUnits];

        Vector3[] circleDestinations = getDestinationCircle(transform.position);
        for (int i = 0; i < maxUnits; i++)
        {
            GameObject go = Instantiate(unitPrefab, circleDestinations[i], Quaternion.identity);
            unitGoArr[i] = go;
            unitArr[i] = go.GetComponent<Unit>();
        }
        rallyFlag = Instantiate(rallyFlagPrefab, transform.position, Quaternion.identity);


        // Set random color
        squadColor = Random.ColorHSV(0f, 1f, 0.5f, 0.75f, 0.8f, 0.9f);
        Material decalMat = decalProjector.material;
        decalProjector.material = new Material(decalProjector.material);
        decalProjector.material.SetColor("_Color", squadColor);
    }


    // TODO: Put this somewhere else
    Vector3[] getDestinationCircle(Vector3 center, float radius = 0.3f, int pointCount = 3)
    {
        int shift = Random.Range(0, pointCount);
        Vector3[] circleDestinations = new Vector3[pointCount];
        for (int i =0; i < pointCount; i++){
            circleDestinations[i] = new Vector3(center.x + radius * Mathf.Cos(Mathf.PI * 2 * (i + shift)/pointCount),
                                                center.y,
                                                center.z + radius * Mathf.Sin(Mathf.PI * 2 * (i + shift)/pointCount));
        }
        return circleDestinations;
    }


    void Update(){
        // Change position based on aggregate of units
        Vector3 aggregatePosition = Vector3.zero;
        foreach (GameObject unit in unitGoArr){
            aggregatePosition = aggregatePosition + unit.transform.position;
        }
        selectableLocation = aggregatePosition/unitGoArr.Length;
        transform.position = selectableLocation;

        // Change opacity based on selection status
        // Really this should just change opacity of the projector
        float targetOpacity = 0f; 
        float opacitySpeed = 4f;
        if (isDisplaying){
            targetOpacity = 0.75f;
        }
        if (_isSelected){
            targetOpacity = 1f;
        }
        currOpacity = Mathf.MoveTowards(currOpacity, targetOpacity, opacitySpeed * Time.deltaTime);
        decalProjector.material.SetFloat("_Alpha", currOpacity);
    }

    public void OnShow(){
        isDisplaying = true;
    }

    public void OnHide(){
        isDisplaying = false;
    }

    public void OnSelect(){
        // TODO: cache flag object ref
        rallyFlag.transform.Find("Cube").gameObject.GetComponent<MeshRenderer>().material.SetInt("_Selected", 1);
    }

    public void OnDeselect(){
        rallyFlag.transform.Find("Cube").gameObject.GetComponent<MeshRenderer>().material.SetInt("_Selected", 0);
    }

    public void OnCommand(SelectableCommand command){
        RaycastHit hit;
        int gridLayer = 1;
        int gridMask = (1 << (gridLayer-1));

        // Set route to new location
        if (command.KeyPressed == KeyCode.Mouse1){
            // TODO: Allow for raycast to hit enemies and behave differently
            if (Physics.Raycast(command.CommandRay, out hit, Mathf.Infinity)){
                if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Walkable"))
                {
                    rallyLocation =  hit.transform.position + new Vector3(0, hit.transform.localScale.y/2.0f);
                    rallyFlag.transform.position = rallyLocation;
                    Vector3[] destinations = getDestinationCircle(rallyLocation);
                    for (int i = 0; i < unitArr.Length; i++){
                        UnitCommand rallyCommand = new UnitCommand(UnitCommandEnum.Rally, destinations[i], null);
                        unitArr[i].OnCommand(rallyCommand);
                    }
                }
                else if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Attackable")){
                    UnitCommand attackCommand = new UnitCommand(UnitCommandEnum.Attack, Vector3.zero, hit.transform.gameObject);
                    for (int i = 0; i < unitArr.Length; i++){
                        unitArr[i].OnCommand(attackCommand);
                    }
                }
            }
        } 
    }

    void OnDestroy(){
        (this as ISelectable).UnsubscribeFromSelector();
    }
}
