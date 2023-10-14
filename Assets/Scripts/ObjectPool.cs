using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EntitySpawningSpace;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }
    public Dictionary<string, int> poolables;
    public Dictionary<string, List<GameObject>> pooledMap;
    public int amountToPool = 10;

    void Awake()
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
    }

    public void PopulatePoolables(){
        poolables = new Dictionary<string, int>();
        poolables["Sword"] = 50;
    }

    public void PoolObjects()
    {
        PopulatePoolables();
        pooledMap = new Dictionary<string, List<GameObject>>();
        List<GameObject> pooledObjects;
        foreach(string objectName in poolables.Keys){
            pooledObjects = new List<GameObject>();
            GameObject tmp;
            for(int i = 0; i < poolables[objectName]; i++)
            {
                tmp = Instantiate(EntitySpawning.EntityLookup[objectName], transform);
                tmp.SetActive(false);
                pooledObjects.Add(tmp);
            }
            pooledMap.Add(objectName, pooledObjects);
        }
    }

    public GameObject GetPooledObject(string objectName)
    {
        for(int i = 0; i < pooledMap[objectName].Count; i++)
        {
            if(!pooledMap[objectName][i].activeInHierarchy)
            {
                Debug.Log(i);
                pooledMap[objectName][i].SetActive(true);
                if (pooledMap[objectName][i].TryGetComponent<IResetable>(out IResetable resetable)){
                    resetable.Reset();
                }
                return pooledMap[objectName][i];
            }
        }
        Debug.Log("Increase pool size for " + objectName);
        return pooledMap[objectName][0];
    }

    public void Release(GameObject go){
        go.SetActive(false);
        go.transform.position = transform.position;
    }
}

interface IResetable {
    public void Reset(){}
}
