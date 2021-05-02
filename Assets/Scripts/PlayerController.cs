using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float flyForce = 1f;

    bool isAlive;

    Rigidbody myRigidbody;

    void Start()
    {
        isAlive = true;
        myRigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (isAlive) 
        {
            Fly();
        }
    }

    public void Fly() 
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) 
        {
            myRigidbody.AddForce(Vector3.up * flyForce, ForceMode.Impulse);
        }
        
    }
}
