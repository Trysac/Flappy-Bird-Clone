using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundSpawner : MonoBehaviour
{
    [SerializeField] Transform platformsParent;

    [SerializeField] int NumberInitialPlatforms = 1;
    [SerializeField] GameObject[] platforms;

    [SerializeField] float platformsSpeed = 1f;

    // InicialPlatformsPosInZ = 16.91
    float inicialPlatformsPosInZ = 16.91f;
    float SpawnRate;

    const int PlatformsSize = 17;

    void Start()
    {
        SetSpawnRate();
    }

    void Update()
    {
        if (GameManager.IsGameStarted) 
        { 
            if (GameManager.isGameActive.Equals(true)) 
            {
                SetSpawnRate();
            }
            else
            {
                StopCoroutine(SpawnPlatform());
                Ground.Speed = 0;
            }
        }   
    }

    public void SpawnInitialPlatforms()
    {
        Vector3 Pos = new Vector3(0, 0, inicialPlatformsPosInZ);
        for (int i = 0; i < NumberInitialPlatforms; i++)
        {
            GameObject platform = Instantiate(platforms[Random.Range(0, platforms.Length)], Pos, Quaternion.identity);
            platform.transform.SetParent(platformsParent);
            inicialPlatformsPosInZ += 16.91f;
            Pos = new Vector3(0, 0, inicialPlatformsPosInZ);
        }
        StartCoroutine(SpawnPlatform());
    }

    private IEnumerator SpawnPlatform()
    {
        yield return new WaitForSeconds(SpawnRate);
        Vector3 Pos = new Vector3(0, 0, inicialPlatformsPosInZ - 16.91f);
        GameObject platform = Instantiate(platforms[Random.Range(0, platforms.Length)], Pos, Quaternion.identity);
        platform.transform.SetParent(platformsParent);
        StartCoroutine(SpawnPlatform());
    }

    private void SetSpawnRate() 
    {
        Ground.Speed = platformsSpeed;
        SpawnRate = (PlatformsSize / Ground.Speed) - 0.1f;
    }
}
