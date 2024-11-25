using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed;
    Rigidbody2D rb;
    [SerializeField] private bool explodes = false;

    [SerializeField] private GameObject explosion;


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.AddForce(transform.up * speed);
        if (explodes)
        {
            explosion.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Ladder") || collision.gameObject.CompareTag("ThroughFloor") || collision.gameObject.CompareTag("Room"))
        {
            return;
        }
        if (gameObject.CompareTag("Shot"))
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                return;
            }
        }
        if(gameObject.CompareTag("EnemyAttack") && collision.gameObject.CompareTag("Attack"))
        {
            transform.rotation = collision.gameObject.transform.rotation;

            ReturnPriv();
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
        rb.velocity = Vector3.zero;
        explosion.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        Destroy(explosion);
        Destroy(gameObject);
    }

    private void ReturnPriv()
    {
        rb.velocity = Vector2.zero;
        rb.AddForce(transform.up * speed);
    }

    public void Return(Vector2 dir)
    {
        transform.rotation = Quaternion.LookRotation(new Vector3(0, 0, 1), new Vector3(dir.x, dir.y, 0));
        rb.velocity = Vector2.zero;
        rb.AddForce(transform.up * speed);
    }
}
