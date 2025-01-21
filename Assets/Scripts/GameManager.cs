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

    void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        numberOfEnemies = FindObjectsOfType<AnimalEnemy>().Length;
        Debug.Log(numberOfEnemies);
    }

    void Update()
    {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = Camera.main.nearClipPlane;
        crosshair.transform.position = mousePos;
        
    }

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

    public void EnemyDied()
    {
        playerController.HasKilled();
        if(workStation.activeSelf) workStation.SetActive(false);
        numberOfEnemies--;
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
        yield return new WaitForSeconds(timeToChangeLevel);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private bool AllEnemiesAreDead()
    {
        if (numberOfEnemies == 0) return true;
        else return false;
    }
}
