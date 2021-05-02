using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ground : MonoBehaviour
{
    public static float Speed { get; set; }

    void Start()
    {
        Speed = 1f;
    }

    void Update()
    {
        Move();
    }

    private void Move()
    {
        transform.Translate(-Vector3.forward * Speed * Time.deltaTime);
    }

}
