using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioUIManager : MonoBehaviour
{
    [SerializeField] private GameObject audioPanel;

    [SerializeField] private Slider masterVolume;
    [SerializeField] private Slider musicVolume;
    [SerializeField] private Slider miscVolume;

    private AudioManager audioManager;

    private MainMenu mainMenu;
    private UIManager uiManager;

    void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();

        masterVolume.value = audioManager.GetMaster();
        musicVolume.value = audioManager.GetMusic();
        miscVolume.value = audioManager.GetMisc();

        mainMenu = FindObjectOfType<MainMenu>();
        uiManager = FindObjectOfType<UIManager>();
        audioPanel.SetActive(false);

        if (mainMenu != null) audioManager.PlayMenuMusic();
        else
        {
            audioManager.PlayMainMusic();
        }
        audioManager.EndedStart();
    }

    public void ChangeMaster()
    {
        audioManager.SetMaster(masterVolume.value);
    }

    public void ChangeMusic()
    {
        audioManager.SetMusic(musicVolume.value);
    }

    public void ChangeMisc()
    {
        audioManager.SetMisc(miscVolume.value);
    }

    public void ExitAudioPanel()
    {
        audioPanel.SetActive(false);
        audioManager.ExitOptions();
        if(mainMenu != null )
        {
            mainMenu.ShowPanel();
        }
        else
        {
            uiManager.ShowPausePanel();
        }
    }

    public void EnterAudioPanel()
    {
        audioPanel.SetActive(true);
        if (mainMenu != null)
        {
            mainMenu.HidePanel();
        }
        else
        {
            uiManager.GoToAudioPanel();
        }
    }
}
