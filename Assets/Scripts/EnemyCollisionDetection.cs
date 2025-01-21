using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCollisionDetection : MonoBehaviour
{
    private AnimalEnemy parent;

    void Start()
    {
        parent = GetComponentInParent<AnimalEnemy>();    
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (parent.isDead()) return;
        if (collision.gameObject.CompareTag("InvisibleWall")) parent.CollidedWithInvisibleWall(collision.transform.position);

        else if (collision.gameObject.CompareTag("Attack")) parent.GotHit(collision);
        else if (collision.gameObject.CompareTag("Explosion")) parent.Exploded(collision);
        else if (collision.gameObject.CompareTag("Shot")) parent.GotShot();
        else if (collision.gameObject.CompareTag("Room")) parent.UpdateRoom(collision.gameObject.GetComponent<Room>());
    }



}
