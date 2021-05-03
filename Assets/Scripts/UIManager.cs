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

    [Header("General Buttons")]
    [SerializeField] Button pauseButton;

    [Header("Skills Buttons")]
    [SerializeField] Button BiteSkill;
    [SerializeField] Button FireballSkill;
    [SerializeField] Button FlamethrowerSkill;
    [SerializeField] Button SpringSkill;

    public static bool IsPause { get; set; }

    void Start()
    {
        IsPause = false;
    }

    void Update()
    {
        Score.text = GameManager.Score.ToString();
        Coins.text = 0.ToString();
        //ActivateSkillsButtons();
    }

    /// <summary>
    /// For The Skills Buttons
    /// </summary>
    /// 

    //private void ActivateSkillsButtons()
    //{
    //    if (SkillsManager.isBiteSkillAvailable) { BiteSkill.enabled = true; }
    //    else { BiteSkill.enabled = false; }

    //    if (SkillsManager.isFlamethrowerSkillAvailable) { FlamethrowerSkill.enabled = true; }
    //    else { FlamethrowerSkill.enabled = false; }

    //    if (SkillsManager.isFireBallSkillAvailable) { FireballSkill.enabled = true; }
    //    else { FireballSkill.enabled = false;}
    //}

    //public void ActivateBiteSkill()
    //{
    //    SkillsManager.UI_ACtivateBiteSkill = true;
    //}

    //public void ActivateFlamethrowerSkill()
    //{
    //    SkillsManager.UI_ACtivateFlamethrowerSkill = true;
    //}

    //public void ActivateFireBallSkill() 
    //{
    //    SkillsManager.UI_ACtivateFireBallSkill = true;
    //}

    //public void ActivateSprintSkill() 
    //{
    //    print("Sprint");
    //}

    /// <summary>
    /// End Skills Buttons
    /// </summary>


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
