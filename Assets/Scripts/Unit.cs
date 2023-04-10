using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnitAttributes;

public abstract class Unit : MonoBehaviour, IActiveOnTurn, IOnGrid
{
    // Interface props
    private Vector2 _location = new Vector2();
    public Vector2 Location {
        get => _location;
        set => _location = value;
    }


    public virtual void OnTurn(){
        // Do an idle bounce
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
