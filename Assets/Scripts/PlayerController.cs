using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float flyForce = 1f;
    
    const float MaximumFlightHeight = 4.50f;
    
    public static bool isAlive;

    Rigidbody myRigidbody;

    void Start()
    {
        isAlive = true;
        myRigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (isAlive && !UIManager.IsPause) 
        {
            Fly();

            if (transform.position.y > MaximumFlightHeight) 
            {
                transform.position = new Vector3(transform.position.x, MaximumFlightHeight, transform.position.z);
            }
        }
    }

    public void Fly() 
    {
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) && transform.position.y < MaximumFlightHeight) 
        {
            myRigidbody.AddForce(Vector3.up * flyForce, ForceMode.Impulse);
        }
        
    }
}
