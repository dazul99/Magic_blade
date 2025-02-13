using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crystal : MonoBehaviour
{
    private Boss parent;

    private void Start()
    {
        parent = GetComponentInParent<Boss>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Attack") || collision.gameObject.CompareTag("Shot") || collision.gameObject.CompareTag("Explosion")) Break();
    }

    public void Break()
    {
        parent.CrystalBreak();
        Destroy(gameObject);
    }
}
