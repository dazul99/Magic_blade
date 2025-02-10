using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private Color openedColor;
    [SerializeField] private Collider2D subcoll;

    private AudioManager audioManager;

    private void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
    }

    public void Open()
    {
        audioManager.PlayOpenDoor();
        transform.localScale = new Vector3(1.1f, transform.localScale.y, transform.localScale.z);
        transform.Translate(0.8f,0,0);
        GetComponent<SpriteRenderer>().color = openedColor;
        subcoll.enabled = false;
        gameObject.GetComponent<Collider2D>().enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Attack")) Open();
    }

}
