using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    private int currentLevel;

    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject creditsPanel;

    private void Start()
    {
        currentLevel = PlayerPrefs.GetInt("Level");
        if (currentLevel == 0) continueButton.interactable = false;
        panel.SetActive(true);
        if(PlayerPrefs.HasKey("MainMenuTime")) PlayerPrefs.DeleteKey("MainMenuTime"); 
        controlsPanel.SetActive(false);
        creditsPanel.SetActive(false);
    }

    public void ShowControls()
    {
        controlsPanel.SetActive(true);
    }

    public void HideControls()
    {
        controlsPanel.SetActive(false);
    }

    public void GoToGameScene()
    {
        SceneManager.LoadScene(1);
    }

    public void LoadGame()
    {
        SceneManager.LoadScene(currentLevel);
    }

    public void Exit()
    {
        
        Application.Quit();
    }

    public void ShowPanel()
    {
        panel.SetActive(true);
    }

    public void HidePanel()
    {
        panel.SetActive(false);
    }

    public void ShowCredits()
    {
        creditsPanel.SetActive(true);
    }

    public void HideCredits()
    {
        creditsPanel.SetActive(false);
    }
}
