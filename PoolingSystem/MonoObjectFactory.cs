using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace Scripts.Core.Pooling
{
    /// <summary>
    /// Base class for object factories. For basic functionality you need only to create child class which will derive from this. 
    /// </summary>
    /// <typeparam name="T">Object class that is supposed to be put in the pool. Typically bullet, enemy, etc. Needs to implement IPoolable interface.</typeparam>
    public class MonoObjectFactory<T> : MonoBehaviour where T : MonoBehaviour, IPoolable
    {
        [Serializable]
        public struct PooledObjectDefinition<T>
        {
            public T PrefabToSpawn;
            [Tooltip("Specify parent transform in case it is different from default one")]
            public Transform CustomParent;
        }

        public int DefaultElements;
        public int MaxElements;
        public List<PooledObjectDefinition<T>> ObjectPrefabs;
        public Transform DefaultParent;

        /// <summary>
        /// Dictionary containing Unity Object pools. Key is the enum representing type of the pooled object.
        /// </summary>
        protected Dictionary<Enum, ObjectPool<T>> _poolsDict;

        protected bool _isInitialized = false;
        public bool IsInitialized
        {
            get { return _isInitialized; }
            protected set { _isInitialized = value; }
        }

        private void OnDestroy()
        {
            Clear();
        }

        /* User defined methods */
        /// <summary>
        /// Use Init() to fill in pools with object instances. This method must be called explicitly through derived class instance
        /// </summary>
        public virtual void Init()
        {
            if (!System.Object.ReferenceEquals(_poolsDict, null))
            {
                Debug.LogError("Trying to create new pool dict, but it is already created!");
                return;
            }

            _poolsDict = new Dictionary<Enum, ObjectPool<T>>();
            foreach (PooledObjectDefinition<T> prefabDef in ObjectPrefabs)
            {
                Transform parent = prefabDef.CustomParent != null ? prefabDef.CustomParent : DefaultParent;
                _poolsDict.Add(prefabDef.PrefabToSpawn.Type, new ObjectPool<T>(() => CreatePooledItem(prefabDef.PrefabToSpawn, parent),
                                OnPoolGet, OnPoolRelease, OnDestroyPoolObject, true, DefaultElements, MaxElements));
            }

            if (_poolsDict.Count > 0)
                _isInitialized = true;
        }

        /* Methods used for unity object pool */
        /// <summary>
        /// Called automatically when new object is instantiatted
        /// </summary>
        /// <param name="objectToCreate"></param>
        /// <param name="objectParent"></param>
        /// <returns></returns>
        protected virtual T CreatePooledItem(T objectToCreate, Transform objectParent)
        {
            if (!_isInitialized)
            {
                Debug.LogError(gameObject.name + " Creating item in factory without initialization! Please call init on this factory.");
                return null;
            }

            if (System.Object.ReferenceEquals(objectParent, null))
                Debug.LogWarning(objectToCreate.gameObject.name + " has no transform parent! This should not be happening");

            T newObj = Instantiate(objectToCreate, objectParent);
            if (System.Object.ReferenceEquals(newObj, null))
                Debug.LogError("Cannot instantiate the object before adding to pool!");

            newObj.onPoolReturning += ActionBindableReturn;
            return newObj;
        }

        /// <summary>
        /// Auxilliar method for binding to action on IPoolable interface
        /// </summary>
        /// <param name="objToReturn"></param>
        private void ActionBindableReturn(IPoolable objToReturn)
        {
            Return((T)objToReturn);
        }

        /// <summary>
        /// Main method used for getting an object from the pools dictionary
        /// </summary>
        /// <param name="typeOfObject">Enum type of the object</param>
        /// <returns>Expected object when everything goes well, null when error happens</returns>
        public virtual T Get(Enum typeOfObject)
        {
            try
            {
                return _poolsDict[typeOfObject].Get();
            }
            catch (Exception ex)
            {
                Debug.LogError(gameObject.name + ", method Get(), fatal exception " + ex.GetType().ToString() + ", key " + typeOfObject.ToString() + ". Exception message: \n" + ex.Message);

                return null;
            }
        }

        /// <summary>
        /// Method used to return given object to the pol
        /// </summary>
        /// <param name="objectToReturn">Object to return</param>
        protected virtual void Return(T objectToReturn)
        {
            try
            {
                _poolsDict[objectToReturn.Type].Release(objectToReturn);
            }
            catch (Exception ex)
            {
                Debug.LogError(gameObject.name + "method Return(), fatal exception " + ex.GetType().ToString() + ", object " + objectToReturn.gameObject.name + ". Exception message: \n" + ex.Message);
            }
        }


        /// <summary>
        /// Clears all dictionaries
        /// </summary>
        public virtual void Clear()
        {
            foreach (KeyValuePair<Enum, ObjectPool<T>> pool in _poolsDict)
            {
                if (_poolsDict != null && !System.Object.ReferenceEquals(_poolsDict[pool.Key], null))
                {
                    _poolsDict[pool.Key].Clear();
                }
            }
        }

        /// <summary>
        /// Called when an item is taken from the pool using Get
        /// </summary>
        /// <param name="objFromPool">Item taken</param>
        protected virtual void OnPoolGet(T objFromPool)
        {
            objFromPool.gameObject.SetActive(true);
            objFromPool.OnPoolGet();
        }

        /// <summary>
        /// Called when an item is returned to the pool using Release
        /// </summary>
        /// <param name="objFromPool">Item returned</param>
        protected virtual void OnPoolRelease(T objFromPool)
        {
            objFromPool.OnPoolRelease();
            objFromPool.gameObject.SetActive(false);
        }

        /// <summary>
        /// If the pool capacity is reached then any items returned will be destroyed.
        /// this is also being called when calling Clear()
        /// </summary>
        /// <param name="objFromPool">Item being destroyed</param>
        protected virtual void OnDestroyPoolObject(T objFromPool)
        {
            if (!System.Object.ReferenceEquals(objFromPool, null) && !System.Object.ReferenceEquals(objFromPool.gameObject, null))
            {
                //Debug.LogWarning(gameObject.name + ": Capacity reached for " + objFromPool.gameObject.name);
                Destroy(objFromPool.gameObject);
            }
        }

        /// <summary>
        /// Debug method
        /// </summary>
        public void DebugPoolsCount()
        {
            foreach (KeyValuePair<Enum, ObjectPool<T>> pool in _poolsDict)
            {
                Debug.Log(gameObject.name + " Pools debug: ");
                if (_poolsDict[pool.Key] != null)
                {
                    Debug.Log("POOL " + pool.Key.ToString());
                    Debug.Log("Count all: " + _poolsDict[pool.Key].CountAll);
                    Debug.Log("Count out of pool: " + _poolsDict[pool.Key].CountActive);
                    Debug.Log("Count in the pool: " + _poolsDict[pool.Key].CountInactive);
                }
            }
        }
    }
}
