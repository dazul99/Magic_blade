using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class AnimalEnemy : MonoBehaviour
{
    private GameManager gameManager;

    private Rigidbody2D rigid;

    private Collider2D coll;
    private bool dead = false;

    [SerializeField] private bool lookingRight = true;
    private Vector3 front;

    [SerializeField] private float rangeOfDetection = 6f;
    [SerializeField] private float rangeOfVision = 10f;

    [SerializeField] private LayerMask playerLayer;
    private RaycastHit2D[] detecting;
    private RaycastHit2D[] chasing;

    private bool canSeePlayer = false;
    private Vector3 offset = new Vector3(0.51f, 0.25f, 0);

    private bool idleState = true;
    private bool chasingState = false;
    private bool suspiciousState = false;

    [SerializeField] private float suspiciousTime;
    [SerializeField] private float timeLookingOneWay;

    private bool wall;
    private Vector2 direction;

    private PlayerController player;
    private Vector2 lastPosition;

    private RaycastHit2D attackHits;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private float rangeOfAttack;
    [SerializeField] private int typeOfEnemy;


    private Room currentRoom;
    private float directionToMove;

    [SerializeField] private float speed;

    private Vector2 originalPos;


    [SerializeField] private GameObject attackObject;
    [SerializeField] private GameObject attackHitObject;
    private Collider2D attackColl;
    [SerializeField] private float attackMov;
    private bool attacking = false;
    private bool actuallyAttacking = false;
    [SerializeField] private float attackCD = 1f;
    [SerializeField] private float attackDelay = 0.75f;
    private float attackTime = 0.3f;

    private bool notEnterAgain = false;

    private float distanceToLP;

    private float stunnedTime;
    private bool stunned;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        coll = GetComponent<Collider2D>();
        rigid = GetComponent<Rigidbody2D>();
        attackColl = attackHitObject.GetComponent<Collider2D>();
        player = FindObjectOfType<PlayerController>();
        originalPos = transform.position;
    }

    private void Update()
    {
        Detect();
        if (stunned)
        {
            rigid.velocity = Vector2.zero;
            return;
        }
        if (attacking) return;
        detecting = null;
        if (rigid.velocity.x > 0 && lookingRight) lookingRight = false;
        else if (rigid.velocity.x < 0 && !lookingRight) lookingRight = true;

        

        if (!dead)
        {

            if (idleState)
            {
                distanceToLP = Mathf.Abs(originalPos.x - transform.position.x);
                if (distanceToLP > 1f) //mirar si est� lejos de su posici�n inicial
                {
                    float dir = Mathf.Sign(originalPos.x - transform.position.x);
                    rigid.velocity = new Vector2(speed * dir, rigid.velocity.y);
                }
                else
                {
                    rigid.velocity = Vector2.zero;
                }
                if (notEnterAgain) notEnterAgain = false;
                return;
            }
            else if (chasingState)
            {
                if (notEnterAgain) notEnterAgain = false;

                //Comprobar si a�n se le ve

                direction = player.transform.position - transform.position;
                direction.Normalize();

                CalculateRaycastOrigin();
                Debug.DrawRay(transform.position + offset, direction * rangeOfVision);
                chasing = Physics2D.RaycastAll(transform.position + offset, direction, rangeOfVision);

                ChasingCheck();

                if (!chasingState) return;
                Debug.DrawRay(transform.position, new Vector2(rangeOfAttack, 0));
                //acercarse si no est� lo suficientemente cerca
                if (Physics2D.OverlapCircle(transform.position, rangeOfAttack, playerLayer) == null) //si no est� lo suficientemente cerca para atacar
                {
                    if (player.transform.position.x < transform.position.x) directionToMove = -1;
                    else directionToMove = 1;
                    rigid.velocity = new Vector2(directionToMove * speed, rigid.velocity.y);
                }
                else //si est� suficientemente cerca
                {
                    rigid.velocity = Vector2.zero;
                    StartCoroutine(Attack1());
                }

            }
            else if (suspiciousState && !notEnterAgain)
            {
                if (lastPosition != null) //mirar si tenemos una �ltima posici�n
                {
                    distanceToLP = Mathf.Abs(lastPosition.x - transform.position.x);
                    if (distanceToLP < 1f) //mirar si est� lo suficientemente cerca de esa posici�n
                    {
                        StartCoroutine(SearchInPlace());
                        notEnterAgain = true;
                    }
                    else
                    {
                        float dir = (lastPosition.x - transform.position.x) / Mathf.Abs(lastPosition.x - transform.position.x);
                        rigid.velocity = new Vector2(speed * dir, rigid.velocity.y);
                    }
                }
            }
        }
        if (wall) wall = false;
    }

   
    private IEnumerator SearchInPlace()
    {
        rigid.velocity = new Vector2(0, rigid.velocity.y);
        rigid.inertia = 0f;

        float timePassed = 0f;
        float aux = 0f;
        while (suspiciousState)
        {
            if (timePassed >= suspiciousTime)
            {
                idleState = true;
                suspiciousState = false;
            }
            else
            {
                while (suspiciousState && aux < timeLookingOneWay)
                {
                    rigid.velocity = new Vector2(0, rigid.velocity.y);
                    Detect();
                    timePassed += 0.1f;
                    aux += 0.1f;
                    yield return new WaitForSeconds(0.1f);
                }
                Debug.Log("Change");
                aux = 0f;
                lookingRight = !lookingRight;
            }
        }
        notEnterAgain = false;
        yield return null;
    }

    private IEnumerator Attack1()
    {
        Vector2 cPos = new(player.transform.position.x, player.transform.position.y);
        attacking = true;
        Vector2 pos = new(transform.position.x, transform.position.y);
        Vector2 dir = cPos - pos;
        dir.Normalize();
        attackHitObject.transform.right = cPos - pos;
        yield return new WaitForSeconds(attackDelay);
        if (dead) yield break;
        actuallyAttacking = true;
        rigid.AddForce(dir * attackMov);
        attackColl.enabled = true;
        attackObject.SetActive(true);

        yield return new WaitForSeconds(attackTime);
        attackHitObject.transform.right = Vector2.right;
        actuallyAttacking = false;
        attackColl.enabled = false;
        attackObject.SetActive(false);
        attacking = false;
        yield return new WaitForSeconds(attackCD);
    }



    private void CalculateRaycastOrigin()
    {
        if (lookingRight && front != Vector3.right)
        {
            front = Vector3.right;
            offset.x = front.x * 0.51f;
        }
        else if (!lookingRight && front != Vector3.left)
        {
            front = Vector3.left;
            offset.x = front.x * 0.51f;
        }
        front.Normalize();
    }


    private void Detect()
    {
        float aux;
        CalculateRaycastOrigin();

        aux = front.x;
        Debug.DrawRay(transform.position + offset, front * rangeOfDetection);
        detecting = Physics2D.RaycastAll(transform.position + offset, front, rangeOfDetection);

        DetectingCheck();

        if (chasingState) return;
        else
        {
            front = new Vector2(aux * 3, -1);
            front.Normalize();
            Debug.DrawRay(transform.position + offset, front * rangeOfDetection);
            detecting = Physics2D.RaycastAll(transform.position + offset, front, rangeOfDetection);
        }

        DetectingCheck();

        if (chasingState) return;
        else
        {
            front = new Vector2(aux * 3, 1);
            front.Normalize();
            Debug.DrawRay(transform.position + offset, front * rangeOfDetection);
            detecting = Physics2D.RaycastAll(transform.position + offset, front, rangeOfDetection);
        }


        DetectingCheck();

    }

    private void ChasingCheck()
    {
        if (chasing.Length > 0)
        {
            for (int i = 0; i < chasing.Length && !wall; i++)
            {

                if (chasing[i].collider != null && chasing[i].collider.gameObject.CompareTag("Wall"))
                {
                    if (Physics2D.OverlapCircle(transform.position, 1, playerLayer) == null)
                    {
                        wall = true;
                        lastPosition = player.transform.position;
                        chasingState = false;
                        suspiciousState = true;
                        return;
                    }

                }
                if (chasing[i].collider != null && chasing[i].collider.gameObject.CompareTag("Player")) return;
            }

            if (Physics2D.OverlapCircle(transform.position, 1, playerLayer) == null)
            {
                lastPosition = player.transform.position;
                chasingState = false;
                suspiciousState = true;
                return;
            }


        }
    }

    private void DetectingCheck()
    {
        if (detecting.Length > 0)
        {
            for (int i = 0; i < detecting.Length && !wall; i++)
            {
                if (detecting[i].collider != null && detecting[i].collider.gameObject.CompareTag("Wall"))
                {
                    wall = true;
                }
                else if (detecting[i].collider != null && detecting[i].collider.gameObject.CompareTag("Player") && !chasingState)
                {

                    idleState = false;
                    suspiciousState = false;
                    chasingState = true;

                }

            }

        }
        wall = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (dead) return;

        if (collision.gameObject.CompareTag("Attack"))
        {
            if (player.MeleeAttacking())
            {
                if (actuallyAttacking) return;
                if (ItHits(collision)) Die();
                else return;
            }
            else
            {
                Die();
            }
        }
        else if (collision.gameObject.CompareTag("Explosion"))
        {
            if (ItHits(collision)) Die();
            else return;
        }
        else if (collision.gameObject.CompareTag("Room") && collision.gameObject.GetComponent<Room>() != currentRoom)
        {
            currentRoom = collision.gameObject.GetComponent<Room>();
        }
    }


    private bool ItHits(Collider2D collision)
    {
        Vector2 aux1 = coll.ClosestPoint(collision.gameObject.transform.position);
        Vector2 dir = new(-aux1.x + collision.gameObject.transform.position.x, -aux1.y + collision.gameObject.transform.position.y);
        float dist = Mathf.Sqrt(dir.x * dir.x + dir.y * dir.y);

        attackHits = Physics2D.Raycast(aux1, dir.normalized, dist, groundLayer);
        return attackHits.collider == null;
    }

    private void Die()
    {
        rigid.velocity = Vector2.zero;
        rigid.inertia = 0f;
        dead = true;
        Debug.Log("AAAAAAGH");
    }

    public void Stun(float t)
    {
        stunnedTime = t;
        StartCoroutine(StunnedForSomeTime());
    }

    private IEnumerator StunnedForSomeTime()
    {
        float aux = 0f;
        stunned = true;
        while(aux<stunnedTime)
        {
            aux += 0.05f;
            yield return new WaitForSeconds(0.05f);
        }
        stunned = false;
    }

    public void Knockback(Vector2 aux)
    {
        rigid.AddForce(aux * 600);
    }

    public void Kill()
    {
        Die();
    }

}