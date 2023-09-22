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
    private int maxUnits = 20;
    public int currUnits = 3;
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

    public int InstanceID
    {
        get => gameObject.GetInstanceID();
    }    

    public Vector3 Position
    {
        get => transform.position;
    }

    public float squadRange = 3f;

    void Awake()
    {
        (this as ISelectable).SubscribeToSelector();

        unitGoArr = new GameObject[maxUnits];
        unitArr = new ICommandable[maxUnits];

        currUnits = 3;
        Vector3[] circleDestinations = Vector3UtilsClass.getDestinationCircle(transform.position, currUnits, 0.3f * currUnits/3f);
        for (int i = 0; i < currUnits; i++)
        {
            GameObject go = Instantiate(unitPrefab, circleDestinations[i], Quaternion.identity, null);
            unitGoArr[i] = go;
            unitArr[i] = go.GetComponent<ICommandable>();
            if (go.TryGetComponent<Unit>(out Unit unit)){
                unit.parentSquad = this;
            }
        }
        rallyFlag = Instantiate(rallyFlagPrefab, transform.position, Quaternion.identity);
    }

    void Start()
    {
        // Set random color
        squadColor = UnitAttributes.BirdPalettes.paletteMap[unitGoArr[0].GetComponent<Unit>().unitTypeEnum].DarkColor;
        Material decalMat = decalProjector.material;
        decalProjector.material = new Material(decalProjector.material);
        decalProjector.material.SetColor("_Color", squadColor);
    }

    void Update(){
        if (PauseManager.paused){ return; }
        // Change position based on aggregate of units
        Vector3 aggregatePosition = Vector3.zero;
        foreach (GameObject unit in unitGoArr)
        {
            if (unit == null) { continue; }
            aggregatePosition = aggregatePosition + unit.transform.position;
        }

        if (currUnits == 0){ Object.Destroy(this); }

        aggregatePosition = aggregatePosition/currUnits;

        for (int i=0; i<unitGoArr.Length; i +=1)
        {
            if (unitGoArr[i] == null) { continue; }
            if (Vector3.Distance(aggregatePosition, unitGoArr[i].transform.position) > squadRange && currUnits > 1)
            { 
                unitGoArr[i].GetComponent<Unit>().parentSquad = null;
                unitGoArr[i] = null;
                unitArr[i] = null;
                currUnits -= 1;
            } 
        }

        if (currUnits > 0)
        {
            transform.position = Vector3UtilsClass.perFrameLerp(transform.position, aggregatePosition, 0.999f);
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
        targetOpacity = 1f; // Actually, I quite like it always on
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
        int terrainMask = LayerMask.GetMask("Walkable");
        int d_i = 0;
        int queryMask = ~LayerMask.GetMask("Squad");

        // Set route to new location
        // TODO: Allow clicked object to determine the command sent to units?
        if (command.KeyPressed == KeyCode.Mouse1){
            
            if (Physics.Raycast(command.CommandRay, out hit, Mathf.Infinity, queryMask)){
                Debug.Log(hit.transform.gameObject.name);
                if (terrainMask == (terrainMask | 1 << hit.transform.gameObject.layer))
                {
                    rallyLocation =  hit.transform.position + new Vector3(0, hit.transform.localScale.y/2.0f, 0);
                    rallyFlag.transform.position = rallyLocation;
                    Vector3[] destinations = Vector3UtilsClass.getDestinationCircle(rallyLocation, currUnits, 0.3f * currUnits/3f);
                    d_i = 0;
                    for (int i = 0; i < unitArr.Length; i++){
                        if (unitArr[i] == null){ continue; }
                        UnitCommand rallyCommand = new UnitCommand(UnitCommandEnum.Rally, destinations[d_i], null);
                        unitArr[i].OnCommand(rallyCommand);
                        d_i += 1;
                    }
                }
                else if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Attackable")){
                    UnitCommand attackCommand = new UnitCommand(UnitCommandEnum.Attack, Vector3.zero, hit.transform.gameObject);
                    for (int i = 0; i < unitArr.Length; i++){
                        if (unitArr[i] == null){ continue; }
                        unitArr[i].OnCommand(attackCommand);
                    }
                }
                else if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Carryable")){
                    if (hit.transform.gameObject.TryGetComponent<ICarryable>(out ICarryable carryable))
                    {
                        // Get unit destinations
                        d_i = 0;
                        for (int i = 0; i < unitArr.Length; i++){
                            if (unitArr[i] == null){ continue; }
                            if (d_i >= carryable.CarriersNeeded){ break; }
                            UnitCommand carryCommand = new UnitCommand(UnitCommandEnum.Carry, carryable.CarryPivots[d_i], hit.transform.gameObject);
                            unitArr[i].OnCommand(carryCommand);
                            d_i++;
                        } 
                    }
                }
            }
        }

        if (command.KeyPressed == KeyCode.LeftShift)
        {
            for (int i = 0; i < unitArr.Length; i++){
                if (unitArr[i] == null){ continue; }
                UnitCommand cancelCommand = new UnitCommand(UnitCommandEnum.Cancel, Vector3.zero, null);
                unitArr[i].OnCommand(cancelCommand);
            } 
        }
    
        if (command.KeyPressed == KeyCode.Space)
        {
            for (int i = 0; i < unitArr.Length; i++){
                if (unitArr[i] == null){ continue; }
                UnitCommand jumpCommand = new UnitCommand(UnitCommandEnum.Jump, Vector3.zero, null);
                unitArr[i].OnCommand(jumpCommand);
            } 
        }
    }

    void OnDestroy(){
        (this as ISelectable).UnsubscribeFromSelector();
        (this as ISelectable).DestroySelectable();
        Object.Destroy(this.gameObject);
        Object.Destroy(rallyFlag);
        Object.Destroy(this);
        
    }

    public void AddUnit(GameObject go, ICommandable commandable){
        if (currUnits < maxUnits){
            for (int i = 0; i < unitArr.Length; i++){
                if (unitGoArr[i] == null){
                    unitArr[i] = commandable;
                    unitGoArr[i] = go;
                    currUnits += 1;
                    // Have commandableUnit follow the same command,
                    // or even better re-trigger a command to all units of squad?
                    break;
                }
            }
        }
    }

}
