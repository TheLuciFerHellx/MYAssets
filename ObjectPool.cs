using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance;

    private Dictionary<string, List<GameObject>> pooledObjects = new Dictionary<string, List<GameObject>>();

    private void Awake()
    {
        Instance = this;
    }

    public void AddToPool(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.position = new Vector3(22, 1, 38);
        
        string key = obj.name.Replace("(Clone)", "").Trim();
        if (!pooledObjects.ContainsKey(key))
        {
            pooledObjects[key] = new List<GameObject>();
        }
        pooledObjects[key].Add(obj);
    }

    public CarMover GetCarFromPool(CarMover prefab)
    {
        string key = prefab.name;
        if (pooledObjects.ContainsKey(key))
        {
            foreach (GameObject obj in pooledObjects[key])
            {
                if (!obj.activeInHierarchy)
                {
                    return obj.GetComponent<CarMover>();
                }
            }
        }
        return null;
    }

    public Passenger GetPassengerFromPool(Passenger passenger)
    {
        string key = passenger.name;
        if (pooledObjects.ContainsKey(key))
        {
            foreach (GameObject obj in pooledObjects[key])
            {
                if (!obj.activeInHierarchy)
                {
                    return obj.GetComponent<Passenger>();
                }
            }
        }
        return null;
    }
}
