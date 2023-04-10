using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Selectable;

public class Squad : MonoBehaviour, ISelectable
{
    // Start is called before the first frame update
    private int maxUnits = 5;
    public GameObject[]  unitArr;
    public GameObject unitPrefab;
    public Vector3 selectableLocation;
    public Vector3 rallyLocation;

    public GameObject selectableMeshPrefab;
    public GameObject rallyFlagPrefab;
    public GameObject selectableMesh;
    public GameObject rallyFlag;

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

        // Default for now, initialize 5 units
        unitArr = new GameObject[maxUnits];
        Vector3[] circleDestinations = getDestinationCircle(transform.position);
        for (int i = 0; i < maxUnits; i++)
        {
            GameObject go = Instantiate(unitPrefab, circleDestinations[i], Quaternion.identity);
            unitArr[i] = go;
        }
        selectableMesh = Instantiate(selectableMeshPrefab, transform.position + 0.01f * Vector3.up, Quaternion.identity);
        rallyFlag = Instantiate(rallyFlagPrefab, transform.position, Quaternion.identity);
    }


    // Put this somewhere else
    Vector3[] getDestinationCircle(Vector3 center, float radius = 0.3f, int pointCount = 5)
    {
        Vector3[] circleDestinations = new Vector3[pointCount];
        for (int i =0; i < pointCount; i++){
            circleDestinations[i] = new Vector3(center.x + radius * Mathf.Cos(Mathf.PI * 2 * i/pointCount),
                                                center.y,
                                                center.z + radius * Mathf.Sin(Mathf.PI * 2 * i/pointCount));
        }
        return circleDestinations;
    }


    void Update(){}

    public void OnShow(){

    }

    public void OnHide(){

    }

    public void OnSelect(){
        selectableMesh.GetComponent<MeshRenderer>().material.SetInt("_Selected", 1);
        rallyFlag.transform.Find("Cube").gameObject.GetComponent<MeshRenderer>().material.SetInt("_Selected", 1);
            /*
            selectedObject = hit.transform.parent.gameObject;
            selectedMesh = hit.transform.gameObject.GetComponent<MeshRenderer>();
            selectedMesh.material.SetInt("_Selected", 1);
            */
    }

    public void OnDeselect(){
        selectableMesh.GetComponent<MeshRenderer>().material.SetInt("_Selected", 0);
        rallyFlag.transform.Find("Cube").gameObject.GetComponent<MeshRenderer>().material.SetInt("_Selected", 0);
    }

    void OnDestroy(){
        (this as ISelectable).UnsubscribeFromSelector();
    }
}
