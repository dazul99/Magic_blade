using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

    //Tgls = toggles, scndry = secondary, us = ultimate skill

    //Variables for the workstation, used to get all toggles from the workstation panel and to know which ones are set
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

    [SerializeField] private Image[] scndryImages;

    [SerializeField] private GameObject interactButton;

    [SerializeField] private GameObject demoPanel;
    
    [SerializeField] private GameObject controlsPanel;

    [SerializeField] private GameObject gamePanel;

    private void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        workStationPanel.SetActive(false);
        foreach(Toggle toggle in scndryTgls) { toggle.isOn = false; }
        foreach(Toggle toggle in uSTgls) {  toggle.isOn = false; }

        timeLeft.text = timeToEndStage.ToString();
        interactButton.SetActive(false);
        if(demoPanel != null) demoPanel.SetActive(false);
        controlsPanel.SetActive(false);
        gamePanel.SetActive(true);
    }


    public void GameStarted()
    {
        bluePlanetPanel.SetActive(false);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        StartCoroutine(Countdown());
    }

    public void ShowControls()
    {
        controlsPanel.SetActive(true);
        pausePanel.SetActive(false);
        gamePanel.SetActive(false);
    }

    public void HideControls()
    {
        controlsPanel.SetActive(false);
        pausePanel.SetActive(true);
        gamePanel.SetActive(true);
    }

    private void ShowGamePanel()
    {
        gamePanel.SetActive(true);
    }
    private void HideGamePanel()
    {
        gamePanel.SetActive(false);
    }

    public void ShowInteractable()
    {
        interactButton.SetActive(true);
    }

    public void HideInteractable()
    {
        interactButton.SetActive(false);
    }

    //This is the timer so the player doesn't spend too much time in one level.
    private IEnumerator Countdown()
    {
        while (timeToEndStage > 0)
        {
            yield return new WaitForSeconds(0.1f);
            timeToEndStage -= 0.1f;

            //this is because if we don't round the number it starts displaying 0.9999999 numbers
            timeToEndStage = Mathf.Round(timeToEndStage * 100) / 100;
            timeLeft.text = timeToEndStage.ToString();
        }
        playerController.DiePublic();
    }

    public void UsedNormalAttack(float cd)
    {
        currentCDNormalAttack = cd;
        
        normalAttackTime.text = currentCDNormalAttack.ToString();
        StartCoroutine(NormalCooldown());
    }

    //The cooldown manager for the normal attack, it's in the UIManager because like this it synchronizes with the time showed on the screen
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

    //same as the normal attack cooldown but this one is for the secondary attack
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

    //manager for the US (Ultimate Skills) on the campfire (workstation GO)
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

    //manager for the SA (Secondary attacks) on the campfire (workstation GO)
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

    //When we hide the worksStation panel we update the secondary attack and ultimate skill for the player
    public void HideWorkStation()
    {
        workStationPanel.SetActive(false);
        playerController.SetSecondaryAttack(scndryATglsIndex);
        secondAttackImage.sprite = scndryImages[scndryATglsIndex].sprite;
        playerController.SetUniqueSkill(uSTglsIndex);
        playerController.ExitStation();
    }

    public void UpdateSecondAttack(int x, int y)
    {
        currentUsesSecondAttack = x;
        maxUsesSecondAttack = x;
        secondAttackUses.text = currentUsesSecondAttack + "/" + maxUsesSecondAttack;
        secondAttackImage.sprite = scndryImages[y].sprite;
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

    public void GoToAudioPanel()
    {
        pausePanel.SetActive(false);
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

    public void DemoFinished()
    {
        demoPanel.SetActive(true);
    }

}
