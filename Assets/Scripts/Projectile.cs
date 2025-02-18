using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed;
    Rigidbody2D rb;
    [SerializeField] private bool explodes = false;

    [SerializeField] private GameObject explosion;

    [SerializeField] private float explosionTime = 0.9f;

    private GameManager gameManager;

    [SerializeField] private Animator explosionAnimator;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        rb = GetComponent<Rigidbody2D>();
        rb.AddForce(transform.up * speed);
        if (explodes)
        {
            explosion.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Ladder") || collision.gameObject.CompareTag("ThroughFloor") || collision.gameObject.CompareTag("Room") || collision.gameObject.CompareTag("MainCamera") || collision.gameObject.CompareTag("InvisibleWall") || collision.gameObject.CompareTag("WorkStation"))
        {
            return;
        }
        if (collision.gameObject.CompareTag("Door") && collision.isTrigger) return;
        if (gameObject.CompareTag("Shot"))
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                return;
            }
            else if (collision.gameObject.CompareTag("EnemyAttack"))
            {
                Debug.Log("a");
                transform.rotation = Quaternion.LookRotation(Vector3.forward, gameManager.PlayerPos());
                ReturnPriv();
                gameObject.tag = "EnemyShot";
                return;
            }
        }
        if(gameObject.CompareTag("EnemyShot"))
        {
            if (collision.gameObject.CompareTag("Attack"))
            {
                Vector3 auxPos = new Vector3(gameManager.CrossPos().x, gameManager.CrossPos().y,0);
                transform.rotation = Quaternion.LookRotation(Vector3.forward, (auxPos - transform.position).normalized);
                ReturnPriv();
                gameObject.tag = "Shot";
                return;
            }
            else if (collision.gameObject.CompareTag("Enemy")) return;
            
        }
        if (explodes)
        {
            StartCoroutine(Ekusupurosion());
        }
        else
        {
            Destroy(gameObject);
        }
        
    }

    private IEnumerator Ekusupurosion()
    {
        explosionAnimator.SetBool("Exploded", true);
        gameManager.Explosion();
        rb.velocity = Vector3.zero;
        explosion.SetActive(true);
        yield return new WaitForSeconds(explosionTime);
        Destroy(explosion);
        Destroy(gameObject);
    }

    private void ReturnPriv()
    {
        gameManager.Parry();
        rb.velocity = Vector2.zero;
        rb.AddForce(transform.up * speed);
    }

    public void Return(Vector2 dir)
    {
        gameManager.Parry();
        transform.rotation = Quaternion.LookRotation(new Vector3(0, 0, 1), new Vector3(dir.x, dir.y, 0));
        rb.velocity = Vector2.zero;
        rb.AddForce(transform.up * speed);
    }
}
