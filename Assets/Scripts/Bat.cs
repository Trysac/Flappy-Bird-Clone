using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bat : Enemy
{
    [Header("Bat General Parameters")]
    [SerializeField] float flySpeed = 2f;

    Animator myAnimator;

    void Start()
    {
        myAnimator = GetComponent<Animator>();
        this.IsAlive = true;
    }


    void Update()
    {
        if (GameManager.isGameActive && GameManager.IsGameStarted) 
        {
            Fly();
        }
        else 
        {
            if (IsAlive) 
            { 
                myAnimator.SetTrigger("Victory");
            }
            
        }
    }

    private void Fly() 
    {
        transform.Translate(-Vector3.forward * flySpeed * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Player")) 
        {
            myAnimator.SetTrigger("Attack");
        }

        if (collision.gameObject.tag.Equals("PlayerAttack"))
        {
            myAnimator.SetTrigger("Hit");
            Die();
        }
    }

    protected override void Die() 
    {
        //Dead
    }

}
