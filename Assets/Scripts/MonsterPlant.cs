using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterPlant : Enemy
{
    [Header("MonsterPlant Parameters")]
    [SerializeField] float biteAttackRange = 1.5f;
    [SerializeField] float distanceAttackRange = 6.5f;
    [SerializeField] float IdleBattleStanceRange = 9f;

    Animator myAnimator;

    private void Start()
    {
        myAnimator = GetComponent<Animator>();
        this.IsAlive = true;
    }

    void LateUpdate()
    {
        if (GameManager.isGameActive)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            
            if (distanceToPlayer == 0) { IsAttackingActive = false; }

            if (IsAttackingActive && IsAlive)
            {
                if (distanceToPlayer < biteAttackRange) { BiteAttack(); }
                else if (distanceToPlayer < distanceAttackRange) { RangeAttack(); }
                else if(distanceToPlayer < IdleBattleStanceRange) { IdleBattleStance();}
            }
        }
        else 
        {
            if (IsAlive) 
            { 
                myAnimator.SetTrigger("Victory");
            }
            
        }
    }

    private void RangeAttack() 
    {
        myAnimator.SetTrigger("DistanceAttack");
    }
    private void BiteAttack() 
    {
        myAnimator.SetTrigger("Bite");
    }
    private void IdleBattleStance() 
    {
        myAnimator.SetTrigger("Battle");
    }

    protected override void Die() 
    {
        IsAlive = false;
        myAnimator.SetTrigger("Hit");
        print("Dead Enemy");
    }
}
