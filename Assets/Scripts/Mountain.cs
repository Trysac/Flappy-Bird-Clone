using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mountain : MonoBehaviour
{
    void Update()
    {
        if (GameManager.isGameActive && GameManager.IsGameStarted)
        {
            transform.Translate(-Vector3.right * (Ground.Speed / 2) * Time.deltaTime);
        }

        if (transform.position.x < -160) 
        {
            Destroy(gameObject);
        }
    }
}
