using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Selectable;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;
using UnitCommands;
using Vector3Utils;

public class Squad : MonoBehaviour, ISelectable
{
    // Start is called before the first frame update
    private int maxUnits = 3;
    public ICommandable[]  unitArr;
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
        unitArr = new ICommandable[maxUnits];

        Vector3[] circleDestinations = Vector3UtilsClass.getDestinationCircle(transform.position);
        for (int i = 0; i < maxUnits; i++)
        {
            GameObject go = Instantiate(unitPrefab, circleDestinations[i], Quaternion.identity);
            unitGoArr[i] = go;
            unitArr[i] = go.GetComponent<ICommandable>();
        }
        rallyFlag = Instantiate(rallyFlagPrefab, transform.position, Quaternion.identity);


        // Set random color
        squadColor = Random.ColorHSV(0f, 1f, 0.5f, 0.75f, 0.8f, 0.9f);
        Material decalMat = decalProjector.material;
        decalProjector.material = new Material(decalProjector.material);
        decalProjector.material.SetColor("_Color", squadColor);
    }

    void Update(){
        if (PauseManager.paused){ return; }

        // Change position based on aggregate of units
        Vector3 aggregatePosition = Vector3.zero;
        int unitCount = 0;
        foreach (GameObject unit in unitGoArr)
        {
            if (unit == null) { continue; }
            aggregatePosition = aggregatePosition + unit.transform.position;
            unitCount += 1;
        }

        if (unitCount > 0)
        {
            selectableLocation = aggregatePosition/unitCount;
            transform.position = selectableLocation;
        }


        // Change opacity based on selection status
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
        // TODO: cache this object ref
        rallyFlag.transform.Find("Cube").gameObject.GetComponent<MeshRenderer>().material.SetInt("_Selected", 1);
    }

    public void OnDeselect(){
        rallyFlag.transform.Find("Cube").gameObject.GetComponent<MeshRenderer>().material.SetInt("_Selected", 0);
    }

    public void OnCommand(SelectableCommand command){
        RaycastHit hit;
        int gridLayer = 1;
        int gridMask = (1 << (gridLayer-1));
        int terrainMask = LayerMask.GetMask("Walkable") + LayerMask.GetMask("NonWalkableTerrain");

        // Set route to new location
        // TODO: Allow clicked object to determine the command sent to units?
        if (command.KeyPressed == KeyCode.Mouse1){
            if (Physics.Raycast(command.CommandRay, out hit, Mathf.Infinity)){
                if (terrainMask == (terrainMask | 1 << hit.transform.gameObject.layer))
                {
                    rallyLocation =  hit.transform.position + new Vector3(0, hit.transform.localScale.y/2.0f);
                    rallyFlag.transform.position = rallyLocation;
                    Vector3[] destinations = Vector3UtilsClass.getDestinationCircle(rallyLocation, unitArr.Length);
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
                else if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Carryable")){
                    if (hit.transform.gameObject.TryGetComponent<ICarryable>(out ICarryable carryable))
                    {
                        // Get unit destinations
                        for (int i = 0; i < unitArr.Length; i++){
                            UnitCommand carryCommand = new UnitCommand(UnitCommandEnum.Carry, carryable.CarryPivots[i], hit.transform.gameObject);
                            unitArr[i].OnCommand(carryCommand);
                        } 
                    }
                }
            }
        }

        if (command.KeyPressed == KeyCode.LeftShift)
        {
            for (int i = 0; i < unitArr.Length; i++){
                UnitCommand cancelCommand = new UnitCommand(UnitCommandEnum.Cancel, Vector3.zero, null);
                unitArr[i].OnCommand(cancelCommand);
            } 
        }
    }

    void OnDestroy(){
        (this as ISelectable).UnsubscribeFromSelector();
    }
}
