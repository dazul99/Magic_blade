using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject crosshair;
    private Vector3 mousePos;
    private PlayerController playerController;

    [SerializeField] private PlatformThrough[] platforms;

    [SerializeField] private GameObject workStation;

    // Update is called once per frame
    void Update()
    {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = Camera.main.nearClipPlane;
        crosshair.transform.position = mousePos;
        playerController = FindObjectOfType<PlayerController>();
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
    }

}
