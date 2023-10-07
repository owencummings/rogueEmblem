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

        public static Dictionary<string, GameObject> EntityLookup = new Dictionary<string, GameObject>(){};

        public static List<string> Rewards = new List<string>{"Food", "Carryable"};
        public static List<string> Enemies = new List<string>{"BigEnemy", "Wizard"};

        public static Dictionary<string, int> EntitiesToPool = new Dictionary<string, int>();

        public static GameObject RandomReward(){
            return EntityLookup[Rewards[UnityEngine.Random.Range(0,Rewards.Count)]];
        }

        public static GameObject RandomEnemy(){
            return EntityLookup[Enemies[UnityEngine.Random.Range(0,Enemies.Count)]];
        }

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
            isLoaded = true;
        }
    }


}
