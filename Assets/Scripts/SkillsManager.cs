using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillsManager : MonoBehaviour
{
    [Header("General Parameters")]
    [SerializeField] GameObject SkillOriginPoint;

    [Header("Skills Effects Prefabs")]
    [SerializeField] GameObject FlamethrowerEffectPrefab;
    [SerializeField] GameObject FireBallEffectPrefab;
    [SerializeField] GameObject BiteEffectPrefab;

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
        Myanimator.SetTrigger("Bite");
        StartCoroutine(InstatiateSkillWithoutAnimation(BiteEffectPrefab, 0f));
    }
    private void FlamethrowerSkill() 
    {
        Myanimator.SetTrigger("Flamethrower");
        StartCoroutine(InstatiateSkillWithAnimation(FlamethrowerEffectPrefab, 0.9f));
    }
    private void FireBallSkill() 
    {
        Myanimator.SetTrigger("FireBall");
        StartCoroutine(InstatiateSkillWithoutAnimation(FireBallEffectPrefab,1.28f, 0.8f));
    }

    private IEnumerator InstatiateSkillWithAnimation(GameObject skillPrefb, float waitToActive = 0.5f) 
    {
        PlayerController.GravityState = false;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        yield return new WaitForSeconds(waitToActive);
        GameObject skill = Instantiate(skillPrefb, SkillOriginPoint.transform.position, Quaternion.identity);
        skill.transform.SetParent(SkillOriginPoint.transform);
        Destroy(skill, skill.GetComponent<Animation>().clip.length + 0.1f);
        yield return new WaitForSeconds(skill.GetComponent<Animation>().clip.length);
        PlayerController.GravityState = true;
    }

    private IEnumerator InstatiateSkillWithoutAnimation(GameObject skillPrefb, float waitToActive = 0.5f, float delayToDestroitObject = 1f)
    {
        PlayerController.GravityState = false;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        yield return new WaitForSeconds(waitToActive);
        GameObject skill = Instantiate(skillPrefb, SkillOriginPoint.transform.position, Quaternion.identity);
        skill.transform.SetParent(SkillOriginPoint.transform);
        Destroy(skill, delayToDestroitObject + 0.1f);
        yield return new WaitForSeconds(delayToDestroitObject);
        PlayerController.GravityState = true;
    }



}
