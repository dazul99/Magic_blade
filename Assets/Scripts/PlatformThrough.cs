using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformThrough : MonoBehaviour
{
    private Collider2D coll;
    private PlayerController playerController;
    private bool isIn = false;

    private void Start()
    {
        coll = GetComponent<Collider2D>();
        playerController = FindObjectOfType<PlayerController>();
    }

    public void GoThrough()
    {
        coll.isTrigger = true;
    }

    public void SetThrough(bool x)
    {
        if (!x && isIn)
        {
            StartCoroutine(WillDo());
        }
        else coll.isTrigger = x;
    }

    private IEnumerator WillDo()
    {
        while (isIn)
        {
            yield return new WaitForSeconds(0.5f);
        }
        coll.isTrigger = false;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player")) isIn = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            coll.isTrigger = false;
            isIn = false;
        }
    }
}
