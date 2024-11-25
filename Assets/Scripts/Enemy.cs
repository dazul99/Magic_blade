using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
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

    [SerializeField]private float suspiciousTime;
    [SerializeField] private float timeLookingOneWay;

    private bool wall;
    private Vector2 direction;

    private PlayerController player;
    private Vector2 lastPosition;
    private Room lastRoom;

    private RaycastHit2D attackHits;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private float rangeOfAttack;
    [SerializeField] private int typeOfEnemy;

    private Queue<Room> listOfRooms;
    private Room currentRoom;
    private Connection targetCon;
    private Vector2 connectionPos;
    private float directionToMove;
    private bool traveling;

    [SerializeField] private float speed;
    [SerializeField] private float ladderSpeed;

    private Map map;

    private bool climbing;
    private bool ladderTop;
    private bool ladderBottom;

    
    [SerializeField] private GameObject attackObject;
    [SerializeField] private GameObject attackHitObject;
    private Collider2D attackColl;
    [SerializeField] private float attackMov;
    private bool attacking = false;
    [SerializeField] private float attackCD = 1f;
    [SerializeField] private float attackDelay = 0.75f;
    private float attackTime = 0.3f;

    private Ladder playerLadder;
    [SerializeField] private float rangeToStartClimbing = 1f;

    private bool playerInLadder = false;
    private bool notEnterAgain = false;

    private float distanceToLP;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        coll = GetComponent<Collider2D>();
        rigid = GetComponent<Rigidbody2D>();
        map = FindObjectOfType<Map>();
        attackColl = attackHitObject.GetComponent<Collider2D>();
        player = FindObjectOfType<PlayerController>();
    }

    private void Update()
    {
        Detect();
        detecting = null;
        if (rigid.velocity.x > 0 && lookingRight) lookingRight = false;
        else if(rigid.velocity.x < 0 && !lookingRight) lookingRight = true;

        if (climbing || attacking) return;

        if (!dead)
        {
            if (traveling)
            {

                //check if there's floor or it's hitting a wall/door
                rigid.velocity = new Vector2(directionToMove * speed, 0);
                if (targetCon.IsLadder() && ((directionToMove > 0 && connectionPos.x <= transform.position.x) || (directionToMove < 0 && connectionPos.x >= transform.position.x)))
                {
                    //use ladder
                    if (ladderTop) StartCoroutine(GoDownLadder());
                    else StartCoroutine(GoUpLadder());
                }
            }
            else if (idleState)
            {
                return;
            }
            else if (chasingState)
            {
                
                //Comprobar si aún se le ve

                direction = player.transform.position - transform.position;
                direction.Normalize();

                CalculateRaycastOrigin();
                Debug.DrawRay(transform.position + offset, direction * rangeOfVision );
                chasing = Physics2D.RaycastAll(transform.position + offset, direction, rangeOfVision);

                ChasingCheck();

                if (!chasingState) return;
                Debug.DrawRay(transform.position, new Vector2(rangeOfAttack, 0));
                //acercarse si no está lo suficientemente cerca
                if (Physics2D.OverlapCircle(transform.position, rangeOfAttack, playerLayer) == null) //si no está lo suficientemente cerca para atacar
                {
                    if(currentRoom != player.GetRoom())
                    {
                        Debug.Log("pain");
                        GoToRoom(player.GetRoom().GetID());
                    }
                    else
                    {
                        if (player.transform.position.x < transform.position.x) directionToMove = -1;
                        else directionToMove = 1;
                        rigid.velocity = new Vector2(directionToMove * speed, 0);
                    }
                }
                else //si está suficientemente cerca
                {
                    rigid.velocity = Vector2.zero;
                    StartCoroutine(Attack1());
                }
                
            }
            else if (suspiciousState && !notEnterAgain)
            {
                //Debug.Log(playerInLadder);
                //Debug.Log(playerLadder == null);
                if(lastPosition != null) //mirar si tenemos una última posición
                {
                    distanceToLP = Mathf.Abs(lastPosition.x - transform.position.x);
                    if(playerLadder == null && playerInLadder) playerLadder = player.GetCurrentLadder();

                    if (playerLadder != null)
                    {
                        FollowPlayerOnLadder(playerLadder);
                    }
                    else if (distanceToLP < 1f) //mirar si está lo suficientemente cerca de esa posición
                    {
                        StartCoroutine(SearchInPlace());
                        notEnterAgain = true;
                    }
                    else if(currentRoom != lastRoom)
                    {
                        GoToRoom(lastRoom.GetID());
                        notEnterAgain = true;
                    }
                    else
                    {
                        float dir = (lastPosition.x - transform.position.x) / Mathf.Abs(lastPosition.x - transform.position.x);
                        rigid.velocity = new Vector2(speed * dir, 0);
                    }
                }
            }
        }
        if (wall) wall = false;
    }

    private void FollowPlayerOnLadder(Ladder ladder)
    {
        float distanceToLadder = Vector2.Distance(transform.position, ladder.transform.position);

        if (distanceToLadder < rangeToStartClimbing)  
        {
            rigid.velocity = Vector2.zero;  
            if (player.transform.position.y > transform.position.y)
            {
                Debug.Log(player.transform.position.y);
                Debug.Log(transform.position.y);
                StartCoroutine(GoUpLadder());
            }
            else if (player.transform.position.y < transform.position.y)
            {
                Debug.Log("what");
                Debug.Log(player.transform.position.y);
                Debug.Log(transform.position.y);
                StartCoroutine(GoDownLadder());
            }
        }
        else
        {
            float moveDirection = Mathf.Sign(ladder.transform.position.x - transform.position.x);
            rigid.velocity = new Vector2(moveDirection * speed, rigid.velocity.y);
        }
    }

    private IEnumerator SearchInPlace()
    {
        rigid.velocity = new Vector2(0,rigid.velocity.y);
        float timePassed = 0f;
        float aux = 0f;
        Debug.Log("AAAAAAAAAAAAAAAAAA");
        while (suspiciousState)
        {
            if (timePassed >= suspiciousTime) break; 
            else 
            {
                while(suspiciousState && aux < timeLookingOneWay)
                {
                    Debug.Log(aux);
                    Detect();
                    timePassed += 0.1f;
                    aux += 0.1f;
                    yield return new WaitForSeconds(0.1f);
                }
                aux = 0f;
                lookingRight = !lookingRight;
            }
        }
        notEnterAgain = false;
        yield return null;
    }

    private IEnumerator Attack1()
    {
        Vector2 cPos = new (player.transform.position.x, player.transform.position.y);
        attacking = true;
        Vector2 pos = new (transform.position.x, transform.position.y);
        Vector2 dir = cPos - pos;
        dir.Normalize();
        attackHitObject.transform.right = cPos - pos;
        yield return new WaitForSeconds(attackDelay);

        rigid.AddForce(dir* attackMov);
        attackColl.enabled = true;
        attackObject.SetActive(true);

        yield return new WaitForSeconds(attackTime);
        attackHitObject.transform.right = Vector2.right;
        attackColl.enabled = false;
        attackObject.SetActive(false);
        attacking = false;
        yield return new WaitForSeconds(attackCD);
    }

    private IEnumerator GoUpLadder()
    {
        climbing = true;
        coll.isTrigger = true;
        float aux = rigid.gravityScale;
        rigid.gravityScale = 0;
        rigid.velocity = Vector2.zero;
        while (!ladderTop)
        {
            yield return new WaitForSeconds(0.1f);
            rigid.velocity = new Vector2(0, ladderSpeed);
        }
        yield return new WaitForSeconds(0.2f);
        coll.isTrigger = false;
        rigid.gravityScale = aux;
        climbing = false;
        playerLadder = null;
    }
    private IEnumerator GoDownLadder()
    {
        climbing = true;
        coll.isTrigger = true;
        float aux = rigid.gravityScale;
        rigid.gravityScale = 0;
        rigid.velocity = Vector2.zero;
        while (!ladderBottom)
        {
            yield return new WaitForSeconds(0.1f);
            rigid.velocity = new Vector2(0, -ladderSpeed);
        }
        coll.isTrigger = false;
        rigid.gravityScale = aux;
        climbing = false;
        playerLadder = null;
    }

    private void GoToRoom(int id)
    {
        listOfRooms = map.GetPath(currentRoom,map.GetRoomWithID(id));
        
        if (listOfRooms == null) return;
        else
        {
            traveling = true;
            listOfRooms.Dequeue();
            WalkToNextRoom();
        }
    }

    private void WalkToNextRoom()
    {
        if (listOfRooms.Count == 0)
        {
            if(notEnterAgain) notEnterAgain = false;
            traveling = false;
            return;
        }

        Room nextRoom = listOfRooms.Dequeue();
        targetCon = currentRoom.FindConnection(nextRoom);
        if (targetCon.IsLadder()) connectionPos = targetCon.GetPos();
        else connectionPos = nextRoom.GetPos();
        directionToMove = connectionPos.x - transform.position.x;
        directionToMove /= Mathf.Abs(directionToMove);
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
            front = new Vector2(aux*3, -1);
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
                        lastRoom = player.GetRoom();
                        if (player.IsClimbing()) playerInLadder = true;
                        chasingState = false;
                        suspiciousState = true;
                        return;
                    }

                }
                if (chasing[i].collider != null && chasing[i].collider.gameObject.CompareTag("Player")) return;
            }

            if (Physics2D.OverlapCircle(transform.position, 1, playerLayer) == null)
            {
                if (player.IsClimbing()) playerInLadder = true;
                lastPosition = player.transform.position;
                lastRoom = player.GetRoom();
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
        else if (collision.gameObject.CompareTag("Room")&& collision.gameObject.GetComponent<Room>() != currentRoom)
        {
            currentRoom = collision.gameObject.GetComponent<Room>();
            if (traveling)
            {
                WalkToNextRoom();
            }
        }
        else if (collision.gameObject.CompareTag("Ladder") && collision.gameObject.GetComponent<LadderEnd>() != null)
        {
            if (collision.gameObject.GetComponent<LadderEnd>().GetTop())
            {
                ladderTop = true;
                ladderBottom = false;
            }
            else
            {
                ladderTop = false;
                ladderBottom = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Ladder"))
        {
            ladderTop = ladderBottom = false;
        }
    }


    private bool ItHits(Collider2D collision)
    {
        Vector2 aux1 = coll.ClosestPoint(collision.gameObject.transform.position);
        Vector2 dir = new (-aux1.x + collision.gameObject.transform.position.x, -aux1.y + collision.gameObject.transform.position.y);
        float dist = Mathf.Sqrt(dir.x * dir.x + dir.y * dir.y);

        attackHits = Physics2D.Raycast(aux1, dir.normalized, dist, groundLayer);
        return attackHits.collider == null;
    }

    private void Die()
    {
        rigid.velocity = Vector2.zero;
        dead = true;
        Debug.Log("AAAAAAGH");
    }
}
