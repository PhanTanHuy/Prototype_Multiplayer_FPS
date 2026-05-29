using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;
    [SerializeField] private GameObject prefabEnemy;
    [SerializeField] private int poolSize = 20;
    [SerializeField] private float spawnInterval = 3f;
    private List<Transform> targets = new List<Transform>();
    private Queue<GameObject> enemyPool = new Queue<GameObject>();
    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        InitPool();
        StartCoroutine(IESpawnEnemy());
        GameObject[] targetArr = GameObject.FindGameObjectsWithTag("Player");
        foreach(GameObject target in targetArr)
        {
            targets.Add(target.transform);
            Debug.Log(target.name);
        }
    }
    public Transform GetTarget(Transform enemyWantToGetTarget)
    {
        if (targets.Count == 0) return null;

        Transform targetReturn = targets[0];
        float minSqrDistance = (enemyWantToGetTarget.position - targets[0].position).sqrMagnitude;

        for (int i = 1; i < targets.Count; i++)
        {
            float sqrDist = (enemyWantToGetTarget.position - targets[i].position).sqrMagnitude;
            if (sqrDist < minSqrDistance)
            {
                minSqrDistance = sqrDist;
                targetReturn = targets[i];
            }
        }
        return targetReturn;
    }

    void InitPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject enemy = Instantiate(prefabEnemy);
            enemy.SetActive(false);
            enemyPool.Enqueue(enemy);
        }
    }

    IEnumerator IESpawnEnemy()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (enemyPool.Count == 0)
                continue;

            GameObject enemy = enemyPool.Dequeue();
            enemy.transform.position = transform.position;
            enemy.transform.rotation = Quaternion.identity;
            enemy.GetComponent<LifeManager>().ReSpawn();
        }
    }

    public void ReturnEnemy(GameObject enemy)
    {
        enemy.SetActive(false);
        enemyPool.Enqueue(enemy);
    }
}
