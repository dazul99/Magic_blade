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

    private void Start()
    {
        currentLevel = PlayerPrefs.GetInt("Level");
        if (currentLevel == 0) continueButton.interactable = false;
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
}
