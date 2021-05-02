using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static bool isGameActive { get; set; }

    void Start()
    {
        isGameActive = true;
    }

    void Update()
    {
        if (FindObjectOfType<PlayerController>() != null) 
        { 
            isGameActive = PlayerController.isAlive;
        }

        if (isGameActive.Equals(false))
        {
            FindObjectOfType<UIManager>().EnableGameOverPanel();
        }
    }
}
