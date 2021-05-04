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

        //BiteSkill.GetComponentInChildren<Image>().fillAmount = 1;
        //FlamethrowerSkill.GetComponentInChildren<Image>().fillAmount = 1;
        //FireballSkill.GetComponentInChildren<Image>().fillAmount = 1;
        //SpringSkill.GetComponentInChildren<Image>().fillAmount = 1;
    }

    void Update()
    {
        Score.text = GameManager.Score.ToString();
        Coins.text = 0.ToString();
        ActivateSkillsButtons();
    }

    /// <summary>
    /// For The Skills Buttons
    /// </summary>
    /// 

    private void ActivateSkillsButtons()
    {
        if (SkillsManager.isBiteSkillAvailable)
        {
            BiteSkill.enabled = true;
            BiteSkill.GetComponentInChildren<Image>().color = Color.green;
        }
        else 
        { 
            BiteSkill.enabled = false;
            BiteSkill.GetComponentInChildren<Image>().color = Color.red;
        }

        if (SkillsManager.isFlamethrowerSkillAvailable) 
        { 
            FlamethrowerSkill.enabled = true;
            FlamethrowerSkill.GetComponentInChildren<Image>().color = Color.green;
        }
        else 
        { 
            FlamethrowerSkill.enabled = false;
            FlamethrowerSkill.GetComponentInChildren<Image>().color = Color.red;
        }

        if (SkillsManager.isFireBallSkillAvailable) 
        { 
            FireballSkill.enabled = true;
            FireballSkill.GetComponentInChildren<Image>().color = Color.green;
        }
        else 
        { 
            FireballSkill.enabled = false;
            FireballSkill.GetComponentInChildren<Image>().color = Color.red;
        }

        if (SkillsManager.isSprintSkillAvailable) 
        { 
            SpringSkill.enabled = true;
            SpringSkill.GetComponentInChildren<Image>().color = Color.green;
        }
        else 
        { 
            SpringSkill.enabled = false;
            SpringSkill.GetComponentInChildren<Image>().color = Color.red;
        }
    }

    //public void BiteSkillImageFill(float timer) 
    //{
    //    if (timer < 1) { BiteSkill.GetComponentInChildren<Image>().fillAmount = timer; }
    //}
    //public void FireballSkillImageFill(float timer)
    //{
    //    if (timer < 1) { FireballSkill.GetComponentInChildren<Image>().fillAmount = timer; }
    //}
    //public void FlamethrowerSkillImageFill(float timer)
    //{
    //    if (timer < 1) { FlamethrowerSkill.GetComponentInChildren<Image>().fillAmount = timer; }
    //}
    //public void SprintSkillImageFill(float timer)
    //{
    //    if (timer < 1) { SpringSkill.GetComponentInChildren<Image>().fillAmount = timer; }
    //}

    /// <summary>
    /// Buttons
    /// </summary>

    public void ActivateBiteSkill()
    {
        //if (!SkillsManager.UI_ACtivateBiteSkill) 
        //{
        //    BiteSkill.GetComponentInChildren<Image>().fillAmount = 0;
        //}
        SkillsManager.UI_ACtivateBiteSkill = true;
    }

    public void ActivateFlamethrowerSkill()
    {
        //if (!SkillsManager.UI_ACtivateFlamethrowerSkill)
        //{
        //    FlamethrowerSkill.GetComponentInChildren<Image>().fillAmount = 0;
        //}
        SkillsManager.UI_ACtivateFlamethrowerSkill = true;
    }

    public void ActivateFireBallSkill()
    {
        //if (!SkillsManager.UI_ACtivateFireBallSkill)
        //{
        //    FireballSkill.GetComponentInChildren<Image>().fillAmount = 0;
        //}
        SkillsManager.UI_ACtivateFireBallSkill = true;
    }

    public void ActivateSprintSkill()
    {
        //if (!SkillsManager.UI_ACtivateSprintSkill)
        //{
        //    SpringSkill.GetComponentInChildren<Image>().fillAmount = 0;
        //}
        SkillsManager.UI_ACtivateSprintSkill = true;
    }

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
