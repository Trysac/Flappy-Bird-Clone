using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Range(0.01f,0.50f)]
    [Tooltip("From 0.05 to 0.25")]
    [SerializeField] float IncreseInScaleTime = 0.1f;

    [Tooltip("don´t get too crazy. From 1 to 5")]
    [SerializeField] int pointToAdd = 1;

    [Range(20f, 120f)]
    [Tooltip("Depend on how long do you want to make every game loop last for the player")]
    [SerializeField] float timeBetweenTimeScaleIncreses = 40f;

    [Range(1f, 1.4f)]
    [Tooltip("More that 1.40 it feels to exaggerated, and it´s almost imposible to play")]
    [SerializeField] float limitTimeScaleValue = 1.4f;

    [SerializeField] int CountDownTime = 3;

    public static bool isGameActive { get; set; }
    public static int Score { get; set; }
    public static bool IsGameStarted { get; set; }

    void Start()
    {
        Score = 0;
        isGameActive = true;
        IsGameStarted = false;
        StartCoroutine(StartGame());
    }

    public IEnumerator StartGame() 
    {
        yield return new WaitForSeconds(CountDownTime);
        IsGameStarted = true;
        PlayerController.GravityState = true;
        FindObjectOfType<GroundSpawner>().SpawnInitialPlatforms();
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
        if ((Time.timeScale < limitTimeScaleValue)) 
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
