using System.Collections.Generic;
using UnityEngine;

public class EnemyPoolManager : MonoBehaviour {
    public static EnemyPoolManager Instance { get; private set; }
    
    [System.Serializable]
    public class Pool {
        public GameObject prefab;
        public int poolSize;
    }
    
    public List<Pool> pools;
    
    private Dictionary<GameObject, Queue<GameObject>> poolDictionary;
    
    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // This line makes it persistent!
        } else {
            Destroy(gameObject);
            return;
        }
    
        poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();
        foreach (Pool pool in pools) {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            for (int i = 0; i < pool.poolSize; i++) {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            poolDictionary.Add(pool.prefab, objectPool);
        }
    }

    
    public GameObject GetEnemy(GameObject prefab) {
        if (!poolDictionary.ContainsKey(prefab)) {
            Debug.LogWarning("Pool for prefab not found, instantiating new object");
            return Instantiate(prefab);
        }
        
        Queue<GameObject> objectPool = poolDictionary[prefab];
        if (objectPool.Count > 0) {
            GameObject obj = objectPool.Dequeue();
            obj.SetActive(true);
            return obj;
        } else {
            // Optionally expand the pool if needed
            return Instantiate(prefab);
        }
    }
    
    public void ReturnEnemy(GameObject enemy, GameObject prefab) {
        enemy.SetActive(false);
        if (poolDictionary.ContainsKey(prefab)) {
            poolDictionary[prefab].Enqueue(enemy);
        } else {
            Destroy(enemy);
        }
    }
}