using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollisionDetection : MonoBehaviour
{
    private PlayerController playerController;

    private void Start()
    {
        playerController = GetComponentInParent<PlayerController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.gameObject.CompareTag("Door")) playerController.SetDoor(collision.gameObject.GetComponent<Door>());
        else if (collision.gameObject.CompareTag("WorkStation")) playerController.SetWS(true);
        else if (collision.gameObject.CompareTag("Room")) playerController.SetRoom(collision.gameObject.GetComponent<Room>());
        else if (collision.gameObject.CompareTag("Ladder"))
        {
            if (collision.gameObject.GetComponent<Ladder>() != null) playerController.SetLadder(collision.gameObject.GetComponent<Ladder>());
            else playerController.SetLadderEnd(collision.gameObject.GetComponent<LadderEnd>());
        }
        else if (collision.gameObject.CompareTag("EnemyAttack")) playerController.GotHit(collision.gameObject.GetComponentInParent<AnimalEnemy>());
        else if (collision.gameObject.CompareTag("EnemyShot")) playerController.GotShot(collision.gameObject.GetComponentInParent<Projectile>());
        else if (collision.gameObject.CompareTag("EndOfLevel")) playerController.EnteredEndOfLevel();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Explosion") && !gameObject.CompareTag("Attack"))
        {
            playerController.DiePublic();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (gameObject.CompareTag("Attack")) return;
        if (collision.gameObject.CompareTag("Ladder"))
        {
            playerController.ExitLadder(collision.gameObject.GetComponent<Ladder>());

        }
        else if (collision.gameObject.CompareTag("WorkStation")) playerController.SetWS(false);
        else if (collision.gameObject.CompareTag("Door")) playerController.SetDoor(null);
    }

}
