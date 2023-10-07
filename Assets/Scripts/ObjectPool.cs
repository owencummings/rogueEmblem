using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EntitySpawningSpace;

namespace ObjectPooling
{
    public class ObjectPool : ScriptableObject
    {
        public static ObjectPool Instance;
        public ObjectPool GetInstance()
        {
            if (Instance == null)
            {
                Instance = new ObjectPool();
            }
            return Instance;
        }

        public static Dictionary<string, List<GameObject>> pooledMap;
        public static int amountToPool = 10;

        public static void PoolObjects(Dictionary<string, int> poolables)
        {
            List<GameObject> pooledObjects;
            foreach(string objectName in poolables.Keys){
                pooledObjects = new List<GameObject>();
                GameObject tmp;
                for(int i = 0; i < poolables[objectName]; i++)
                {
                    tmp = Instantiate(EntitySpawning.EntityLookup[objectName]);
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
                if(pooledMap[objectName][i].activeInHierarchy)
                {
                    return pooledMap[objectName][i];
                }
            }
            return null;
        }
    }
}
