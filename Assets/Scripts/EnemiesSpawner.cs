using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesSpawner : MonoBehaviour
{
    [SerializeField] Transform enemiesParent;
    [SerializeField] GameObject[] enemiesPrefabs;
    [SerializeField] float spawnRate = 2f;
    [SerializeField] float SpawnDelay = 4f;
    

    private void Start()
    {
        StartCoroutine(InstatiateEnemies());
    }

    private IEnumerator InstatiateEnemies() 
    {
        yield return new WaitForSeconds(SpawnDelay);
        if (GameManager.isGameActive && GameManager.IsGameStarted) 
        {
            SpawnDelay = 0;
            Vector3 pos = new Vector3(Random.Range(-1.5f , 1.5f), Random.Range(0.5f, 4.2f),17);
            int index = Random.Range(0, enemiesPrefabs.Length);
            GameObject enemy = Instantiate(enemiesPrefabs[index], pos, enemiesPrefabs[index].transform.rotation);
            enemy.transform.SetParent(enemiesParent);
            yield return new WaitForSeconds(spawnRate);
            StartCoroutine(InstatiateEnemies());
        }     
    }
}
