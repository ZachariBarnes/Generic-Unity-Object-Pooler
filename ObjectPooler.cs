using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ObjectPoolItem
{

	public GameObject objectToPool;
	public int amountToPool;
	public bool shouldExpand = true;

	public ObjectPoolItem(GameObject obj, int amt, bool exp = true)
	{
		objectToPool = obj;
		amountToPool = Mathf.Max(amt, 2);
		shouldExpand = exp;
	}
}

public class ObjectPooler : MonoBehaviour
{
	public static ObjectPooler SharedInstance;
	public List<ObjectPoolItem> itemsToPool;


	public List<List<GameObject>> pooledObjectsList;
	internal List<GameObject> pooledObjects;
	private List<int> positions;

	void Awake()
	{

		SharedInstance = this;

		pooledObjectsList = new List<List<GameObject>>();
		pooledObjects = new List<GameObject>();
		positions = new List<int>();


		for (int i = 0; i < itemsToPool.Count; i++)
		{
			ObjectPoolItemToPooledObject(i);
		}
	}

	public GameObject GetPooledObject(int index)
	{
		int curSize = pooledObjectsList[index].Count;
		for (int i = positions[index] + 1; i < positions[index] + pooledObjectsList[index].Count; i++)
		{
			if (pooledObjectsList[index][i % curSize] != null)
			{
				if (!pooledObjectsList[index][i % curSize].activeInHierarchy)
				{
					positions[index] = i % curSize;
					return pooledObjectsList[index][i % curSize];
				}
			}
		}
		if (itemsToPool[index].shouldExpand)
		{
			GameObject obj = (GameObject)Instantiate(itemsToPool[index].objectToPool,  transform);
			pooledObjectsList[index].Add(obj);
			itemsToPool[index].amountToPool++;
			obj.SetActive(false);
			return obj;

		}
		return null;
	}

	public List<GameObject> GetAllPooledObjects(int index)
	{
		return pooledObjectsList[index];
	}


	public int AddObject(GameObject GO, int amt = 3, bool exp = true)
	{
		ObjectPoolItem item = new ObjectPoolItem(GO, amt, exp);
		int currLen = itemsToPool.Count;
		itemsToPool.Add(item);
		ObjectPoolItemToPooledObject(currLen);
		return currLen;
	}

	public void CheckPoolIntegrity()
	{
		int corrections = 0;
		int numOfPools = itemsToPool.Count;
		for(int i=0; i<numOfPools; i++)
		{
			int[] itemsToRemove = new int[] { };
			pooledObjectsList[i].ForEach(pooledItem =>
			{
				if (pooledItem == null)
				{
					itemsToRemove.Concat(new int[] { pooledObjectsList[i].IndexOf(pooledItem) });
				}
			});
			for (int y = 0; y < itemsToRemove.Length; y++)
			{
				pooledObjectsList[i].RemoveAt(itemsToRemove[y]);
				GameObject replacement = Instantiate(itemsToPool[i].objectToPool, transform);
				replacement.SetActive(false);
				pooledObjectsList[i].Add(replacement);
				corrections++;
			}
		}
		if (corrections > 0)
		{
			Debug.LogWarning($"Orphaned Objects Detected in Pool. Corrections made: {corrections}. Please Determine origin and correct.");
		}
	}

	void ObjectPoolItemToPooledObject(int index)
	{
		ObjectPoolItem item = itemsToPool[index];

		pooledObjects = new List<GameObject>();
		for (int i = 0; i < item.amountToPool; i++)
		{
			GameObject obj = (GameObject)Instantiate(item.objectToPool);
			obj.SetActive(false);
			obj.transform.parent = this.transform;
			pooledObjects.Add(obj);
		}
		pooledObjectsList.Add(pooledObjects);
		positions.Add(0);

	}
}