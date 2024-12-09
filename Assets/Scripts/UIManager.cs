using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Image normalAttackImage;

    [SerializeField] private Image secondAttackImage;

    [SerializeField] private TextMeshProUGUI secondAttackUses;

    [SerializeField] private Image dashAttackImage;

    [SerializeField] private TextMeshProUGUI normalAttackTime;
    [SerializeField] private TextMeshProUGUI secondAttackTime;

    private int maxUsesSecondAttack;
    private int currentUsesSecondAttack;

    private float currentCDNormalAttack;
    private bool normalCDDone = true;

    private float currentCDSecondAttack;
    private bool secondCDDone = true;

    [SerializeField] private GameObject bluePlanetPanel;


    [SerializeField] private Slider uniqueSkillSlider;

    private PlayerController playerController;

    [SerializeField] private GameObject workStationPanel;

    [SerializeField] private Toggle[] scndryTgls;
    private int scndryATglsIndex;


    [SerializeField] private Toggle[] uSTgls;
    private int uSTglsIndex;

    private bool changing;

    private Color usedSkillColor = Color.gray;
    private Color availableSkillColor = Color.white;

    [SerializeField] private float timeToEndStage;
    [SerializeField] private TextMeshProUGUI timeLeft;

    [SerializeField] private GameObject pausePanel;

    [SerializeField] private GameObject gameOverPanel;

    private void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        workStationPanel.SetActive(false);
        foreach(Toggle toggle in scndryTgls) { toggle.isOn = false; }
        foreach(Toggle toggle in uSTgls) {  toggle.isOn = false; }

        timeLeft.text = timeToEndStage.ToString();

    }
    
    public void GameStarted()
    {
        bluePlanetPanel.SetActive(false);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        StartCoroutine(Countdown());
    }

    private IEnumerator Countdown()
    {
        while (timeToEndStage > 0)
        {
            yield return new WaitForSeconds(0.1f);
            timeToEndStage -= 0.1f;
            timeToEndStage = Mathf.Round(timeToEndStage * 100) / 100;
            timeLeft.text = timeToEndStage.ToString();
        }
        //YOU LOSE
    }

    public void UsedNormalAttack(float cd)
    {
        currentCDNormalAttack = cd;
        
        normalAttackTime.text = currentCDNormalAttack.ToString();
        StartCoroutine(NormalCooldown());
    }

    private IEnumerator NormalCooldown()
    {
        normalCDDone = false;
        normalAttackImage.color = usedSkillColor;
        while(!normalCDDone)
        {
            currentCDNormalAttack -= 0.05f;
            yield return new WaitForSeconds(0.05f);
            if (currentCDNormalAttack <= 0f)
            {
                normalCDDone = true;
                currentCDNormalAttack = 0f;
            }
            normalAttackTime.text = "" + (Mathf.Round(currentCDNormalAttack*10) / 10); 
            
        }
        normalAttackImage.color = availableSkillColor;
        playerController.NormalCDDone();
        normalAttackTime.text = "";
    }

    public void UsedSecondAttack(float cd)
    {
        currentCDSecondAttack = cd;
        
        if (currentUsesSecondAttack >= 0)
        {
            currentUsesSecondAttack--;
            secondAttackUses.text = currentUsesSecondAttack + "/" + maxUsesSecondAttack;
        }

        secondAttackTime.text = currentCDSecondAttack.ToString();
        StartCoroutine(SecondCooldown());
    }

    private IEnumerator SecondCooldown()
    {
        secondCDDone = false;
        secondAttackImage.color = usedSkillColor;
        while (!secondCDDone)
        {
            currentCDSecondAttack -= 0.05f;
            yield return new WaitForSeconds(0.05f);
            if (currentCDSecondAttack <= 0f)
            {
                secondCDDone = true;
                currentCDSecondAttack = 0f;
            }
            secondAttackTime.text = "" + (Mathf.Round(currentCDSecondAttack * 10) / 10);

        }
        secondAttackImage.color = availableSkillColor;
        playerController.SecondaryCDDone();
        secondAttackTime.text = "";
    }

    public void UsedSpecialDash()
    {
        //cambiar sprite de dashattackuse y dashattackimage 
        dashAttackImage.color = usedSkillColor;
    }

    public void UsingBluePlanet()
    {
        bluePlanetPanel.SetActive(true);
    }

    public void EndedBluePlanet()
    {
        bluePlanetPanel.SetActive(false);
    }

    public void ChangeUniqueSkill(int x)
    {
        uniqueSkillSlider.value = x;
    }

    public void ChangedUS(int x)
    {
        if (changing) return;
        if(x == uSTglsIndex)
        {
            changing = true;
            uSTgls[uSTglsIndex].isOn = true;
            changing = false;
            return;
        }
        changing = true;
        uSTgls[uSTglsIndex].isOn = false;
        uSTglsIndex = x;
        changing = false;
    }
    public void ChangedSA(int x)
    {
        if (changing) return;
        if (x == scndryATglsIndex)
        {
            changing = true;
            scndryTgls[scndryATglsIndex].isOn = true;
            changing = false;
            return;
        }
        changing = true;
        scndryTgls[scndryATglsIndex].isOn = false;
        scndryATglsIndex = x;
        changing = false;
    }

    public void ShowWorkStation()
    {
        workStationPanel.SetActive(true);
        scndryATglsIndex = playerController.CurrentScndryA();
        uSTglsIndex = playerController.CurrentUS();
        scndryTgls[scndryATglsIndex].isOn = true;
        uSTgls[uSTglsIndex].isOn = true;
    }

    public void HideWorkStation()
    {
        workStationPanel.SetActive(false);
        playerController.SetSecondaryAttack(scndryATglsIndex);
        playerController.SetUniqueSkill(uSTglsIndex);
        playerController.ExitStation();
    }

    public void UpdateSecondAttack(int x)
    {
        currentUsesSecondAttack = x;
        maxUsesSecondAttack = x;
        secondAttackUses.text = currentUsesSecondAttack + "/" + maxUsesSecondAttack;
    }

    public void ShowPausePanel()
    {
        pausePanel.SetActive(true);
    }

    public void HidePausePanel()
    {
        pausePanel.SetActive(false);
        playerController.ResumeGame();
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void GameOver()
    {
        gameOverPanel.SetActive(true);
    }

}
