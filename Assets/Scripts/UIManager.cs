using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] TMP_Text Score;
    [SerializeField] TMP_Text Coins;
    [SerializeField] TMP_Text gameOverScore;
    [SerializeField] TMP_Text gameOverBestScore;
    //[SerializeField] TMP_Text gameOverCoins;

    [Header("Panels")]
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] GameObject pausePanel;

    [Header("Buttons")]
    [SerializeField] Button pauseButton;

    public static bool IsPause { get; set; }

    void Start()
    {
        IsPause = false;
    }

    void Update()
    {
        Score.text = 0.ToString();
        Coins.text = 0.ToString();
    }

    public void PauseGame()
    {
        if (IsPause.Equals(false)) 
        {
            pauseButton.enabled = false;
            pausePanel.SetActive(true);
            Time.timeScale = 0;
        }
        else 
        {
            pausePanel.SetActive(false);
            pauseButton.enabled = true;
            Time.timeScale = 1;
        }

        IsPause = !IsPause;
    }

    public void PlayAgain() 
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu() 
    {
        print("Main Menu");
        //SceneManager.LoadScene("MainMenu");
    }

    public void EnableGameOverPanel() 
    {
        Score.enabled = false;
        gameOverPanel.SetActive(true);
        gameOverScore.text = 0.ToString();
        gameOverBestScore.text = 0.ToString();
    }
}
