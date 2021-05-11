using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
    [Header("Coins Prefabs")]
    [SerializeField] GameObject[] coinsPrefabs;
    [SerializeField] Transform[] pointsInPlarform;
    [SerializeField] Transform coinsParent;

    [Header("Coins Probabilities")]
    [Range(0, 0.7f)] [SerializeField] float bronceCoinSpawnProbability;
    [Range(0, 0.25f)] [SerializeField] float silverCoinSpawnProbability;
    [Range(0, 0.15f)] [SerializeField] float goldCoinSpawnProbability;
    [Range(0, 1)] [SerializeField] float NOCoinSpawnProbability;


    void Start()
    {
        SpawnCoins();
    }


    void Update()
    {
        AdjustProbability();
    }


    private void SpawnCoins() 
    {
        for (int i = 0; i < pointsInPlarform.Length; i++) 
        {
            float randomProbability = Random.Range(0f,1f);
            if(randomProbability <= NOCoinSpawnProbability) { continue; }
            else 
            {
                randomProbability = Random.Range(0f, 1f);
                if (randomProbability <= bronceCoinSpawnProbability) { SpawnCoinPrefab(i,0); }
                else if (randomProbability <= bronceCoinSpawnProbability + silverCoinSpawnProbability) { SpawnCoinPrefab(i, 1); }
                else if (randomProbability <= bronceCoinSpawnProbability + silverCoinSpawnProbability + goldCoinSpawnProbability) { SpawnCoinPrefab(i, 2); }
            }
        }
    }

    private void SpawnCoinPrefab(int loopIndex, int coinArrayIndex) 
    {
        GameObject coin = Instantiate(coinsPrefabs[coinArrayIndex], pointsInPlarform[loopIndex].position, Quaternion.identity);
        coin.transform.SetParent(coinsParent);
    }

    private void AdjustProbability()
    {
        NOCoinSpawnProbability = 1 - (bronceCoinSpawnProbability + silverCoinSpawnProbability + goldCoinSpawnProbability);
    }
}
