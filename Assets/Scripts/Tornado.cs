using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tornado : MonoBehaviour
{
    void Update()
    {
        if (GameManager.isGameActive && GameManager.IsGameStarted)
        {
            transform.Translate(-Vector3.forward * (Random.Range(Ground.Speed * 3, Ground.Speed * 15)) * Time.deltaTime);
        }

        if (transform.position.x < -160)
        {
            Destroy(gameObject);
        }
    }
}
