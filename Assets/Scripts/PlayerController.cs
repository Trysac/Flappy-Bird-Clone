using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float flyForce = 1f;
    
    const float MaximumFlightHeight = 4.50f;
    
    public static bool isAlive;

    public static bool GravityState;

    Rigidbody myRigidbody;
    Animator Myanimator;

    void Start()
    {
        isAlive = true;

        Myanimator = GetComponent<Animator>();

        myRigidbody = GetComponent<Rigidbody>();
        myRigidbody.useGravity = GravityState;

        StartCoroutine(StartFlyBattleState());
    }

    void Update()
    {
        if (GameManager.IsGameStarted) 
        {
            myRigidbody.useGravity = GravityState;

            if (isAlive && !UIManager.IsPause) 
            {
                if (GravityState) 
                { 
                    Fly();
                }
                
                if (transform.position.y > MaximumFlightHeight) 
                {
                    transform.position = new Vector3(transform.position.x, MaximumFlightHeight, transform.position.z);
                }
            }
        }     
    }

    private IEnumerator StartFlyBattleState()
    {
        yield return new WaitForSeconds(GameManager.InitialCountDownTime);
        Myanimator.SetBool("Battle",true);
    }

    public void Fly() 
    {
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) && transform.position.y < MaximumFlightHeight) 
        {
            myRigidbody.AddForce(Vector3.up * flyForce, ForceMode.Impulse);
        }      
    }

}
