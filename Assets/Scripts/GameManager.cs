using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject crosshair;
    private Vector3 mousePos;
    private PlayerController playerController;

    [SerializeField] private PlatformThrough[] platforms;

    [SerializeField] private GameObject workStation;

    private int numberOfEnemies;

    [SerializeField] private int timeToChangeLevel = 1;
    [SerializeField] private int timeToLoadLevel = 1;


    [SerializeField] private Collider2D endOfLevel;

    private AudioManager audioManager;
    private UIManager uiManager;
    private CameraMovement cam;

    //Variable only used on the boss scene, it's an invisible wall so the player doesn't exit the arena
    [SerializeField] private GameObject invWallBossRoom;

    void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
        audioManager = FindObjectOfType<AudioManager>();
        playerController = FindObjectOfType<PlayerController>();
        numberOfEnemies = FindObjectsOfType<AnimalEnemy>().Length;
        StartCoroutine(LoadLevel());

        //if it's not the first level, load those three playerprefs and send it to the player
        if (SceneManager.GetActiveScene().buildIndex != 1)
        {
            playerController.SetUSCharges(PlayerPrefs.GetInt("Ultimate_Skill_Charges"));
            playerController.SetUniqueSkill(PlayerPrefs.GetInt("Ultimate_Skill"));
            playerController.SetSecondaryAttack(PlayerPrefs.GetInt("Secondary_Attack"));
        }
        else //if it's the first level set it to zero
        {
            playerController.SetUniqueSkill(0);
            playerController.SetSecondaryAttack(0);
        }
        cam = FindObjectOfType<CameraMovement>();

        if(invWallBossRoom != null) invWallBossRoom.SetActive(false);

    }

    void Update()
    {

        //move the cursor on the game according to the movement of the mouse
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = Camera.main.nearClipPlane;
        crosshair.transform.position = mousePos;
        
    }

    //Sets the platform so the player can go through them or not
    public void SetPlatformsTrigger(bool x)
    {
        for(int i = 0; i < platforms.Length; i++)
        {
            platforms[i].SetThrough(x);
        }
    }

    public Vector2 CrossPos()
    {
        return new Vector2 (crosshair.transform.position.x, crosshair.transform.position.y);
    }

    public Vector3 PlayerPos()
    {
        return playerController.transform.position;
    }

    //When an enemy dies reduces the number of enemies left, if it's zero it opens the end of the level
    public void EnemyDied()
    {
        playerController.HasKilled();
        if(workStation.activeSelf) workStation.SetActive(false);
        numberOfEnemies--;
        if (numberOfEnemies == 0) endOfLevel.isTrigger = true;
    }

    public void GotToEnd()
    {
        if (AllEnemiesAreDead())
        {
            StartCoroutine(ChangeLevel());
        }
        return;
    }

    private IEnumerator ChangeLevel() 
    {
        playerController.LockMovement();
        audioManager.SaveTime();
        yield return new WaitForSeconds(timeToChangeLevel);
        SaveGame();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }


    private void SaveGame()
    {
        PlayerPrefs.SetInt("Level", SceneManager.GetActiveScene().buildIndex + 1);
        PlayerPrefs.SetInt("Ultimate_Skill_Charges", playerController.GetUSCharges());
        PlayerPrefs.SetInt("Ultimate_Skill", playerController.CurrentUS());
        PlayerPrefs.SetInt("Secondary_Attack", playerController.CurrentScndryA());
    }

    private IEnumerator LoadLevel()
    {
        playerController.LockMovement();
        
        yield return new WaitForSeconds(timeToLoadLevel);
        playerController.UnlockMovement();
    }

    private bool AllEnemiesAreDead()
    {
        if (numberOfEnemies == 0) return true;
        else return false;
    }

    public void Explosion()
    {
        audioManager.PlayExplosion();
    }

    public void Parry()
    {
        audioManager.PlayParry();
    }

    public void BossStarted(Vector2 pos)
    {
        cam.LockIn(pos);
        invWallBossRoom.SetActive(true);
    }

    public void BossDied()
    {
        uiManager.DemoFinished();
        Time.timeScale = 0f;
    }


}
