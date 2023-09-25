using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GridSpace;

public class Ingestor : MonoBehaviour
{   
    private Rigidbody _rb;
    private Collider[] _aggroHit;
    private int carryableMask;

    void Awake(){
        _rb = GetComponent<Rigidbody>();
        RigidbodyUtils.StandardizeRigidbody(_rb);
        _rb.isKinematic = true;
        carryableMask = LayerMask.GetMask("Carryable");
    }

    void FixedUpdate(){
            _aggroHit = Physics.OverlapBox(transform.position, new Vector3(2, 2, 2), Quaternion.identity, carryableMask);
            if (_aggroHit.Length > 0){
                foreach(Collider coll in _aggroHit){
                    if (coll.gameObject.TryGetComponent<Food>(out Food food)){
                        if (!food.toIngest){
                            food.toIngest = true;
                            SpawnInUnits(food.value);
                        }
                    }
                }
            }
    }

    void SpawnInUnits(int unitsToSpawn){
        for(int i = 0; i < unitsToSpawn; i++)
        {
            Instantiate(GridManager.Instance.EntityManager.EntityLookup["Melee"] as GameObject, 
                        this.transform.position + Vector3.up + UnityEngine.Random.insideUnitSphere,
                        Quaternion.identity);
        }
    }
}
