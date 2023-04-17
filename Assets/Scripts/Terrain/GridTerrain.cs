using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTerrain : MonoBehaviour
{
    // Component used to hold data of an individual grid square

    public int defense;

    void Awake(){
        defense = Random.Range(0, 6);
    }

}
