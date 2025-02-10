using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip menuClip;
    [SerializeField] private AudioClip musicClip;

    [SerializeField] private AudioSource miscSource1;
    [SerializeField] private AudioSource miscSource2;

    [SerializeField] private AudioClip swordAttack;
    [SerializeField] private AudioClip swordClash;
    [SerializeField] private AudioClip castFireball;
    [SerializeField] private AudioClip castIcicleShot;
    [SerializeField] private AudioClip laser;
    [SerializeField] private AudioClip deathIcicleShot;
    [SerializeField] private AudioClip parry;
    [SerializeField] private AudioClip openDoor;
    [SerializeField] private AudioClip deathEnemySword;
    [SerializeField] private AudioClip explosion;
    [SerializeField] private AudioClip deathEnemyLaser;
    [SerializeField] private AudioClip deathPlayer;
    [SerializeField] private AudioClip step1;
    [SerializeField] private AudioClip step2;
    [SerializeField] private AudioClip step3;
    [SerializeField] private AudioClip workspace;
    [SerializeField] private AudioClip jump;
    [SerializeField] private AudioClip dash;


    private float masterVolume;
    private float musicVolume;
    private float miscVolume;

    private bool started = false;

    private void Awake()
    {
        musicSource.loop = true;

        if(!PlayerPrefs.HasKey("Master Volume"))
        {
            PlayerPrefs.SetFloat("Master Volume", 0.5f);
            PlayerPrefs.SetFloat("Music Volume", 0.5f);
            PlayerPrefs.SetFloat("Misc Volume", 0.5f);
            masterVolume = musicVolume = miscVolume = 0.5f;
        }
        else
        {
            masterVolume = PlayerPrefs.GetFloat("Master Volume");
            musicVolume = PlayerPrefs.GetFloat("Music Volume");
            miscVolume = PlayerPrefs.GetFloat("Misc Volume");

        }
        musicSource.loop = true;
        UpdateAudio();
    }

    public void SaveTime()
    {
        PlayerPrefs.SetFloat("MainMusicTime", musicSource.time);
    }

    public void EndedStart()
    {
        started = true;
    }

    public void ExitOptions()
    {
        PlayerPrefs.SetFloat("Master Volume", masterVolume);
        PlayerPrefs.SetFloat("Music Volume", musicVolume);
        PlayerPrefs.SetFloat("Misc Volume", miscVolume);
    }

    private void UpdateAudio()
    {
        musicSource.volume = masterVolume * musicVolume;
        miscSource1.volume = miscSource2.volume = masterVolume * miscVolume;
    }

    public float GetMaster()
    {
        return masterVolume;
    }

    public void SetMaster(float x)
    {
        masterVolume = x;
        UpdateAudio();
    }

    public float GetMusic()
    {
        return musicVolume;
    }

    public void SetMusic(float x)
    {
        musicVolume = x;
        UpdateAudio();
    }

    public float GetMisc()
    {
        return miscVolume;
    }

    public void SetMisc(float x)
    {
        miscVolume = x;
        UpdateAudio();
        if(started) PlaySwordAttack();
    }


    public void PlayMenuMusic()
    {
        musicSource.clip = menuClip;
        musicSource.Play();
    }

    public void PlayMainMusic()
    {
        musicSource.clip = musicClip;
        if (PlayerPrefs.HasKey("MainMusicTime"))
        {
            musicSource.time = PlayerPrefs.GetFloat("MainMusicTime");
        }
        musicSource.Play();
    }

    public void PlaySwordAttack()
    {
        if (!miscSource1.isPlaying)
        {
            miscSource1.clip = swordAttack;
            miscSource1.Play();
        }
        else{
            miscSource2.clip = swordAttack;
            miscSource2.Play();
        }
        
    }

    public void PlaySwordClash()
    {
        if (!miscSource1.isPlaying)
        {
            miscSource1.clip = swordClash;
            miscSource1.Play();
        }
        else
        {
            miscSource2.clip = swordClash;
            miscSource2.Play();
        }
    }

    public void PlayCastFireball()
    {
        if (!miscSource1.isPlaying)
        {
            miscSource1.clip = castFireball;
            miscSource1.Play();
        }
        else
        {
            miscSource2.clip = castFireball;
            miscSource2.Play();
        }
    }

    public void PlayCastIcicleShot()
    {
        if (!miscSource1.isPlaying)
        {
            miscSource1.clip = castIcicleShot;
            miscSource1.Play();
        }
        else
        {
            miscSource2.clip = castIcicleShot;
            miscSource2.Play();
        }
    }

    public void PlayLaser()
    {
        if (!miscSource1.isPlaying)
        {
            miscSource1.clip = laser;
            miscSource1.Play();
        }
        else
        {
            miscSource2.clip = laser;
            miscSource2.Play();
        }
    }

    public void PlayDeathIcicleShot()
    {
        if (!miscSource1.isPlaying)
        {
            miscSource1.clip = deathIcicleShot;
            miscSource1.Play();
        }
        else
        {
            miscSource2.clip = deathIcicleShot;
            miscSource2.Play();
        }
    }

    public void PlayParry()
    {
        if (!miscSource1.isPlaying)
        {
            miscSource1.clip = parry;
            miscSource1.Play();
        }
        else
        {
            miscSource2.clip = parry;
            miscSource2.Play();
        }
    }

    public void PlayOpenDoor()
    {
        if (!miscSource1.isPlaying)
        {
            miscSource1.clip = openDoor;
            miscSource1.Play();
        }
        else
        {
            miscSource2.clip = openDoor;
            miscSource2.Play();
        }
    }

    public void PlayDeathEnemySword()
    {
        if (!miscSource1.isPlaying)
        {
            miscSource1.clip = deathEnemySword;
            miscSource1.Play();
        }
        else
        {
            miscSource2.clip = deathEnemySword;
            miscSource2.Play();
        }
    }

    public void PlayExplosion()
    {
        if (!miscSource1.isPlaying)
        {
            miscSource1.clip = explosion;
            miscSource1.Play();
        }
        else
        {
            miscSource2.clip = explosion;
            miscSource2.Play();
        }
    }

    public void PlayDeathEnemyLaser()
    {
        if (!miscSource1.isPlaying)
        {
            miscSource1.clip = deathEnemyLaser;
            miscSource1.Play();
        }
        else
        {
            miscSource2.clip = deathEnemyLaser;
            miscSource2.Play();
        }
    }

    public void PlayDeathPlayer()
    {
        if (!miscSource1.isPlaying)
        {
            miscSource1.clip = deathPlayer;
            miscSource1.Play();
        }
        else
        {
            miscSource2.clip = deathPlayer;
            miscSource2.Play();
        }
    }

    public void PlayStep(int x)
    {
        if (x == 1) PlayStep1();
        else if (x == 2) PlayStep2();
        else PlayStep3();
    }

    private void PlayStep1()
    {
        if (!miscSource1.isPlaying)
        {
            miscSource1.clip = step1;
            miscSource1.Play();
        }
        else
        {
            miscSource2.clip = step1;
            miscSource2.Play();
        }
    }

    private void PlayStep2()
    {
        if (!miscSource1.isPlaying)
        {
            miscSource1.clip = step2;
            miscSource1.Play();
        }
        else
        {
            miscSource2.clip = step2;
            miscSource2.Play();
        }
    }

    private void PlayStep3()
    {
        if (!miscSource1.isPlaying)
        {
            miscSource1.clip = step3;
            miscSource1.Play();
        }
        else
        {
            miscSource2.clip = step3;
            miscSource2.Play();
        }
    }

    public void PlayWorkspace()
    {
        if (!miscSource1.isPlaying)
        {
            miscSource1.clip = workspace;
            miscSource1.Play();
        }
        else
        {
            miscSource2.clip = workspace;
            miscSource2.Play();
        }
    }

    public void PlayJump()
    {
        if (!miscSource1.isPlaying)
        {
            miscSource1.clip = jump;
            miscSource1.Play();
        }
        else
        {
            miscSource2.clip = jump;
            miscSource2.Play();
        }
    }

    public void PlayDash()
    {
        if (!miscSource1.isPlaying)
        {
            miscSource1.clip = dash;
            miscSource1.Play();
        }
        else
        {
            miscSource2.clip = dash;
            miscSource2.Play();
        }
    }


}
