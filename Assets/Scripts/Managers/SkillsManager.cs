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

    public static bool isBiteSkillAvailable;
    public static bool isFlamethrowerSkillAvailable;
    public static bool isFireBallSkillAvailable;

    /// <summary>
    /// Variables To Activated Skills With the UI Buttons
    /// </summary>

    public static bool UI_ACtivateBiteSkill;
    public static bool UI_ACtivateFireBallSkill;
    public static bool UI_ACtivateFlamethrowerSkill;


    Animator Myanimator;

    void Start()
    {
        isBiteSkillAvailable = true;
        isFlamethrowerSkillAvailable = true;
        isFireBallSkillAvailable = true;

        //UI_ACtivateBiteSkill = false;
        //UI_ACtivateFireBallSkill = false;
        //UI_ACtivateFlamethrowerSkill = false;

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
        if ((Input.GetKeyDown(KeyCode.Q) /*|| UI_ACtivateBiteSkill*/) && isBiteSkillAvailable)
        {
            isBiteSkillAvailable = false;
            UI_ACtivateBiteSkill = false;
            BiteSkill();
        }
        else if ((Input.GetKeyDown(KeyCode.W) /*|| UI_ACtivateFlamethrowerSkill*/) && isFlamethrowerSkillAvailable)
        {
            isFlamethrowerSkillAvailable = false;
            UI_ACtivateFlamethrowerSkill = false;
            FlamethrowerSkill();
        }
        else if ((Input.GetKeyDown(KeyCode.E) /*|| UI_ACtivateFireBallSkill*/) && isFireBallSkillAvailable) 
        {           
            isFireBallSkillAvailable = false;
            UI_ACtivateFlamethrowerSkill = false;
            FireBallSkill();
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
