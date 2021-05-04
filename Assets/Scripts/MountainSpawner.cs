using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainSpawner : MonoBehaviour
{
    [SerializeField] GameObject[] mountainsPrefabs;
    [SerializeField] Transform SpawnPoint;

    void Start()
    {
        StartCoroutine(SpawnMountain());
    }

    private IEnumerator SpawnMountain() 
    {
        yield return new WaitForSeconds(((17 / Ground.Speed) - 0.1f) * 2);
        if (GameManager.isGameActive && GameManager.IsGameStarted)
        {
            Vector3 Pos = new Vector3(SpawnPoint.position.x, 4, Random.Range(SpawnPoint.position.z - 5, SpawnPoint.position.z + 5));
            GameObject mountain = Instantiate(mountainsPrefabs[Random.Range(0, mountainsPrefabs.Length)], Pos, Quaternion.identity);
            mountain.transform.SetParent(transform);
            StartCoroutine(SpawnMountain());
        }
    }
}
