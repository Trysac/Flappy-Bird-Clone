using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillsManager : MonoBehaviour
{
    [Header("Skills Effects Prefabs")]
    [SerializeField] GameObject FlamethrowerEffectPrefab;
    [SerializeField] GameObject FireBallEffectPrefab;

    [Header("Skills Colldowns")]
    [SerializeField] float BiteCooldown = 1;
    [SerializeField] float FlamethrowerCooldown = 5;
    [SerializeField] float FireBallCooldown = 3;

    private float BiteSkillTimer;
    private float FlamethrowerSkillTimer;
    private float FireBallSkillTimer;

    private bool isBiteSkillAvailable;
    private bool isFlamethrowerSkillAvailable;
    private bool isFireBallSkillAvailable;

    Animator Myanimator;

    void Start()
    {
        isBiteSkillAvailable = true;
        isFlamethrowerSkillAvailable = true;
        isFireBallSkillAvailable = true;

        BiteSkillTimer = 0;
        FlamethrowerSkillTimer = 0;
        FireBallSkillTimer = 0;

        Myanimator = GetComponent<Animator>();
    }

    void Update()
    {
        if (PlayerController.isAlive && GameManager.isGameActive && GameManager.IsGameStarted) 
        {
            ReactiveSkills();
            ManageSkillsImputs();
        }
    }

    private void ReactiveSkills() 
    {
        if (!isBiteSkillAvailable) 
        {
            BiteSkillTimer += Time.deltaTime;
            if (BiteSkillTimer >= BiteCooldown) 
            {
                BiteSkillTimer = 0;
                isBiteSkillAvailable = true; 
            }
        }

        if (!isFlamethrowerSkillAvailable)
        {
            FlamethrowerSkillTimer += Time.deltaTime;
            if (FlamethrowerSkillTimer >= FlamethrowerCooldown) 
            {
                FlamethrowerSkillTimer = 0;
                isFlamethrowerSkillAvailable = true; 
            }
        }

        if (!isFireBallSkillAvailable)
        {
            FireBallSkillTimer += Time.deltaTime;
            if (FireBallSkillTimer >= FireBallCooldown) 
            {
                FireBallSkillTimer = 0;
                isFireBallSkillAvailable = true; 
            }
        }
    }

    private void ManageSkillsImputs() 
    {
        if (Input.GetKeyDown(KeyCode.Q) && isBiteSkillAvailable)
        {
            BiteSkill();
            isBiteSkillAvailable = false;
        }
        else if (Input.GetKeyDown(KeyCode.W) && isFlamethrowerSkillAvailable)
        {
            FlamethrowerSkill();
            isFlamethrowerSkillAvailable = false;
        }
        else if (Input.GetKeyDown(KeyCode.E) && isFireBallSkillAvailable) 
        {
            FireBallSkill();
            isFireBallSkillAvailable = false;
        }
    }

    private void BiteSkill() 
    {
        print("BiteSkill");
    }
    private void FlamethrowerSkill() 
    {
        print("FlamethrowerSkill");
    }
    private void FireBallSkill() 
    {
        print("FireBallSkill");
    }

}
