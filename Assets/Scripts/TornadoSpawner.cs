using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TornadoSpawner : MonoBehaviour
{
    [SerializeField] GameObject[] TornadosPrefabs;
    [SerializeField] float DelayBetwwenTornados = 3f;
    [SerializeField] float delayToDestroyTornado = 10f;
    public static bool ActivateTornadoSpawner { get; set; }
    void Start()
    {
        ActivateTornadoSpawner = false;
        StartCoroutine(SpawnTornado());
    }

    private IEnumerator SpawnTornado() 
    {
        yield return new WaitForSeconds(DelayBetwwenTornados);
        if (GameManager.isGameActive && ActivateTornadoSpawner) 
        {
            Vector3 Pos = new Vector3(Random.Range(-100, -20), transform.position.y + 10,transform.position.z);
            GameObject tornado = Instantiate(TornadosPrefabs[Random.Range(0, TornadosPrefabs.Length)], Pos, Quaternion.identity);
            tornado.transform.SetParent(transform);
            Destroy(tornado, delayToDestroyTornado);
            StartCoroutine(SpawnTornado());
        }
    } 

}
