using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crystal : MonoBehaviour
{
    private Boss parent;
    private AudioManager manager;

    private void Start()
    {
        manager = FindObjectOfType<AudioManager>();
        parent = GetComponentInParent<Boss>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Attack") || collision.gameObject.CompareTag("Shot") || collision.gameObject.CompareTag("Explosion")) Break();
    }

    public void Break()
    {
        manager.PlayCrystalBreak();
        parent.CrystalBreak();
        Destroy(gameObject);
    }
}
