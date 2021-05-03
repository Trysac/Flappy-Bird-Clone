using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Range(0.01f,0.50f)]
    [Tooltip("From 0.05 to 0.25")]
    [SerializeField] float IncreseInScaleTime = 0.1f;

    [Tooltip("don�t get too crazy. From 1 to 5")]
    [SerializeField] int pointToAdd = 1;

    [Range(20f, 120f)]
    [Tooltip("Depend on how long do you want to make every game loop last for the player")]
    [SerializeField] float timeBetweenTimeScaleIncreses = 40f;

    public static bool isGameActive { get; set; }
    public static int Score { get; set; }

    void Start()
    {
        Score = 0;
        isGameActive = true;
        StartCoroutine(AddScoreForFlying());
        StartCoroutine(IncreseDificulty());
    }

    void Update()
    {
        if (FindObjectOfType<PlayerController>() != null) 
        { 
            isGameActive = PlayerController.isAlive;
        }

        if (isGameActive.Equals(false))
        {
            StopCoroutine(AddScoreForFlying());
            StopCoroutine(IncreseDificulty());
            FindObjectOfType<UIManager>().EnableGameOverPanel();
        }
    }

    private IEnumerator AddScoreForFlying() 
    {
        yield return new WaitForSeconds(1);
        Score += pointToAdd;
        StartCoroutine(AddScoreForFlying());
    }

    private IEnumerator IncreseDificulty()
    {
        yield return new WaitForSeconds(timeBetweenTimeScaleIncreses);
        if ((Time.timeScale < 1.39)) 
        {
            pointToAdd += 2;
            Time.timeScale += IncreseInScaleTime;
        }

        StartCoroutine(IncreseDificulty());
    }


    //It's call from other classes, like the "Watch", "Coins", etc.
    public static void DecreseDificulty(float decrese) 
    {
        Time.timeScale -= decrese;
    }

    public static void AddScore(int pointsToAdd)
    {
        Score += pointsToAdd;
    }
}