using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossCollisionDetection : MonoBehaviour
{
    private Boss parent;

    private void Start()
    {
        parent = GetComponentInParent<Boss>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Attack") && parent.GetPoise()) parent.GotKilled();
        else if (collision.gameObject.CompareTag("Shot") && !parent.GetStunned()) StartCoroutine(parent.GotShot());
    }
}
