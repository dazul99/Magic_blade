using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossCollisionDetection : MonoBehaviour
{
    private Boss parent;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Attack") && parent.GetStunned()) parent.GotKilled(); ;
    }
}
