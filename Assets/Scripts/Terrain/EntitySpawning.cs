using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace EntitySpawningSpace {
    public enum Entity {
        Barrel,
        Fruit
    }

    public class EntitySpawning{
        public bool isLoaded = false;

        public Dictionary<string, GameObject> EntityLookup = new Dictionary<string, GameObject>(){
            //{ Entity.Barrel, new GameObject() },
            //{ Entity.Fruit, new GameObject() }
        };

        private AsyncOperationHandle<IList<GameObject>> handle;

        public IEnumerator LoadEntities(){
            var loadResourceLocationsHandle = Addressables.LoadResourceLocationsAsync("unit", typeof(GameObject));

            if (!loadResourceLocationsHandle.IsDone)
                yield return loadResourceLocationsHandle;

            //start each location loading
            List<AsyncOperationHandle> opList = new List<AsyncOperationHandle>();

            foreach (IResourceLocation location in loadResourceLocationsHandle.Result)
            {
                AsyncOperationHandle<GameObject> loadAssetHandle
                    = Addressables.LoadAssetAsync<GameObject>(location);
                loadAssetHandle.Completed +=
                    obj => { EntityLookup.Add(location.PrimaryKey, obj.Result); };
                opList.Add(loadAssetHandle);
            }

            //create a GroupOperation to wait on all the above loads at once. 
            var groupOp = Addressables.ResourceManager.CreateGenericGroupOperation(opList);

            if (!groupOp.IsDone)
                yield return groupOp;

            Addressables.Release(loadResourceLocationsHandle);

            //take a gander at our results.
            foreach (var item in EntityLookup)
            {
                Debug.Log(item.Key + " - " + item.Value.name);
            }

            isLoaded = true;
        }
    }


}
