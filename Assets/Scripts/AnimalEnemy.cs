using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86;

public class AnimalEnemy : MonoBehaviour
{
    private GameManager gameManager;

    private Rigidbody2D rigid;
    [SerializeField] private GameObject collGO;
    private Collider2D coll;
    private bool dead = false;

    [SerializeField] private bool lookingRight = true;
    private Vector3 front;

    [SerializeField] private float rangeOfDetection = 6f;
    [SerializeField] private float rangeOfVision = 10f;

    [SerializeField] private LayerMask playerLayer;
    private RaycastHit2D[] detecting;
    private RaycastHit2D[] chasing;

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

    //Variable used to enter just once in suspicious state without entering any of the other states afterwards
    private bool notEnterAgain = false;

    //LP = last position, this variable is used for both the distance to the last position where the player was seen and the distance to the enemy's original position
    private float distanceToLP;

    private float stunnedTime;
    private bool stunned;

    [SerializeField] private bool canDeflect = false;
    [SerializeField] private LayerMask projectileLayer;

    [SerializeField] private bool flying = false;


    [SerializeField] private bool ranged = false;
    [SerializeField] private GameObject projectile;


    private float movementOfRanged = 0.5f;

    private AudioManager audioManager;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Animator attackAnimator;

    

    private void Awake()
    {
        if (!ranged) transform.rotation = Quaternion.Euler(0, 0, 0);
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>(); 
        rigid = GetComponent<Rigidbody2D>();
        originalPos = transform.position;
        if (lookingRight) spriteRenderer.flipX = true;
        else spriteRenderer.flipX = false;
    }


    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        coll = collGO.GetComponent<Collider2D>();
        if(attackHitObject != null) attackColl = attackHitObject.GetComponent<Collider2D>();
        player = FindObjectOfType<PlayerController>();
        if(ranged) StartCoroutine(MovementRanged());
        else attackAnimator = GetComponentInChildren<Animator>();
        audioManager = FindObjectOfType<AudioManager>();
        
    }

    private void Update()
    {
        //If the enemy is not a flying one nor a ranged one, they detect the player normally
        //they use three rays towards one direction

        if (!flying && !ranged) Detect();

        //If they ARE a flying or a ranged enemy they detect in a radius

        else DetectRadius();

        //If the enemy is stunned or is attacking we exit the update, it's necessary to use two ifs because if it's attacking we don't want to set the velocity to 0
        if (stunned)
        {
            rigid.velocity = Vector2.zero;
            return;
        }
        if (attacking) return;

        detecting = null;

        //Flip the sprite if the enemy turns
        if (rigid.velocity.x > 0 && lookingRight)
        {
            lookingRight = false;
            spriteRenderer.flipX = false;
        }
        else if (rigid.velocity.x < 0 && !lookingRight)
        {
            lookingRight = true;
            spriteRenderer.flipX = false;

        }

        
        animator.SetFloat("Speed", Mathf.Abs(rigid.velocity.x) + Mathf.Abs(rigid.velocity.y));


        if (!dead)
        {
            //Check in which state is the enemy at the moment, there's three possible states

            //Idle state: Go back to original position
            if (idleState)
            {
                IdleState();
                return;
            }

            //Chasing State: Chase the player and attack if possible
            else if (chasingState)
            {
                if (notEnterAgain) notEnterAgain = false;

                //Check if it can still see the player

                direction = player.transform.position - transform.position;
                direction.Normalize();

                CalculateRaycastOrigin();
                Debug.DrawRay(transform.position + offset, direction * rangeOfVision);
                chasing = Physics2D.RaycastAll(transform.position + offset, direction, rangeOfVision);

                ChasingCheck();

                if (!chasingState) return;
                Debug.DrawRay(transform.position, new Vector2(rangeOfAttack, 0));

                //If it's a ranged enemy attacks the player without moving
                if (ranged)
                {
                    attacking = true;   
                    StartCoroutine(RangedAttack());
                    return;
                }

                //If not close enough, the enemy gets closer

                if (Physics2D.OverlapCircle(transform.position, rangeOfAttack, playerLayer) == null) 
                {
                    if (!flying)
                    {
                        if (player.transform.position.x < transform.position.x) directionToMove = -1;
                        else directionToMove = 1;
                        rigid.velocity = new Vector2(directionToMove * speed, rigid.velocity.y);
                    }
                    else
                    {
                        rigid.velocity = new Vector2(player.transform.position.x - transform.position.x, player.transform.position.y - transform.position.y).normalized * speed;
                    }
                    
                }
                else //If close enough, attack
                {
                    if(!flying) rigid.velocity = new Vector2(0, rigid.velocity.y);
                    else rigid.velocity = new Vector2(0,0);
                    attacking = true;
                    StartCoroutine(MeleeAttack());
                    
                }

                

            }


            //Suspicious State: Enemy looks for player on the spot
            //Enter here just once because it starts a coroutine
            else if (suspiciousState && !notEnterAgain)
            {
                SuspiciousState();
                return;
            }
        }
        if (wall) wall = false;
    }

    private void SuspiciousState()
    {
        //If it's a ranged enemy it just returns to Idle state
        if (ranged)
        {
            suspiciousState = false;
            idleState = true;
            StartCoroutine(MovementRanged());
            return;
        }
        if (lastPosition != null) //Check if we have a last position
        {
            distanceToLP = Mathf.Abs(lastPosition.x - transform.position.x);
            if (distanceToLP < 1f) //check if it got to the last position or not
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

    private void IdleState()
    {
        if (ranged)
        {
            return;
        }
        distanceToLP = Mathf.Abs(originalPos.x - transform.position.x);
        if (distanceToLP > 1f) //Check if it's close to its original position
        {
            float dir = Mathf.Sign(originalPos.x - transform.position.x);
            rigid.velocity = new Vector2(speed * dir, rigid.velocity.y);
        }
        else
        {
            rigid.velocity = new Vector2(0, rigid.velocity.y);
        }
        if (notEnterAgain) notEnterAgain = false;
    }
   
    //Function used so the enemy looks for the player on the spot
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
                aux = 0f;
                lookingRight = !lookingRight;
                if (lookingRight) spriteRenderer.flipX = false;
                else spriteRenderer.flipX = true;

            }
        }
        notEnterAgain = false;
        yield return null;
    }

    private IEnumerator MeleeAttack()
    {
        animator.SetBool("Attacking", true);
        Vector2 cPos;
        Vector2 pos;
        cPos = new(player.transform.position.x, player.transform.position.y);
        pos = new(transform.position.x, transform.position.y);
        audioManager.PlaySwordAttack();

        yield return new WaitForSeconds(attackDelay);

        attackHitObject.transform.right = cPos - pos;
        Vector2 dir = cPos - pos;
        dir.Normalize();
        if (dead) yield break;
        actuallyAttacking = true;
        rigid.AddForce(dir * attackMov);
        attackColl.enabled = true;
        attackObject.SetActive(true);
        attackAnimator.SetBool("Attacked", true);

        yield return new WaitForSeconds(attackTime);

        animator.SetBool("Attacking", false);
        attackHitObject.transform.right = Vector2.right;
        actuallyAttacking = false;
        attackColl.enabled = false;
        attackObject.SetActive(false);
        
        yield return new WaitForSeconds(attackCD);
        
        attacking = false;
    }

    private IEnumerator RangedAttack()
    {
        yield return new WaitForSeconds(attackDelay);

        Vector2 cPos;
        Vector2 pos;
        cPos = new(player.transform.position.x, player.transform.position.y);
        pos = new(transform.position.x, transform.position.y);
        audioManager.PlayCastIcicleShot();
        

        Vector2 dir = cPos - pos;
        dir.Normalize();

        if (dead) yield break;

        actuallyAttacking = true;

        Instantiate(projectile, transform.position + new Vector3(dir.x, dir.y, 0), Quaternion.LookRotation(transform.forward, dir));

        actuallyAttacking = false;

        yield return new WaitForSeconds(attackCD);
        attacking = false;
    }

    //Function to check where the detection raycasts come from for the enemies that don't detect the player with a radius
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

    private void DetectRadius()
    {
        Collider2D flyingDetecting = Physics2D.OverlapCircle(transform.position, rangeOfDetection, playerLayer);
        if( flyingDetecting != null && ItHits(flyingDetecting))
        {
            idleState = false;
            suspiciousState = false;
            chasingState = true;
        }
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

    //Check if the Enemy still can see the player, it checks all the Raycast array and stops when it detects a wall or the player
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

    //Same as before but used for the detection, not for the chasing state
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

    public bool isDead()
    {
        return dead;
    }

    public void CollidedWithInvisibleWall(Vector3 wallPos)
    {
        rigid.velocity = new Vector2(0, rigid.velocity.y);
        Vector2 aux = wallPos - transform.position;
        rigid.AddForce(aux * 100);
    }

    public void GotHit(Collider2D collision)
    {
        if (player.MeleeAttacking())
        {
            if (actuallyAttacking) return;
            if (ItHits(collision)) StartCoroutine(Die());
            else return;
        }
        else
        {
            audioManager.PlayDeathEnemySword();
            StartCoroutine(Die());
        }
    }

    public void Exploded(Collider2D collision)
    {
        if (ItHits(collision) && !canDeflect) StartCoroutine(Die());
    }
    
    public void GotShot()
    {
        if (!canDeflect)
        {
            audioManager.PlayDeathIcicleShot();
            StartCoroutine(Die());
        }
    }

    //Funtion made to check if an attack or an explosion actually hits the enemy
    //attacks and explosions hitboxes are large and can get through walls, this checks if there is a wall between the explosion/attack and the enemy
    private bool ItHits(Collider2D collision)
    {
        Vector2 aux1 = coll.ClosestPoint(collision.gameObject.transform.position);
        Vector2 dir = new(-aux1.x + collision.gameObject.transform.position.x, -aux1.y + collision.gameObject.transform.position.y);
        float dist = Mathf.Sqrt(dir.x * dir.x + dir.y * dir.y);

        attackHits = Physics2D.Raycast(aux1, dir.normalized, dist, groundLayer);
        return attackHits.collider == null;
    }

    //Funtion to make ranged enemies dance a little
    private IEnumerator MovementRanged()
    {
        while (!dead && idleState)
        {

            transform.Translate(movementOfRanged, 0, 0);


            yield return new WaitForSeconds(0.3f);

            movementOfRanged *= -1;
        }
    }

    private IEnumerator Die()
    {
        rigid.velocity = Vector2.zero;
        rigid.inertia = 0f;
        dead = true;
        gameObject.layer = 0;
        gameManager.EnemyDied();
        coll.enabled = false;
        rigid.gravityScale = 1f;
        rigid.includeLayers = groundLayer;
        spriteRenderer.flipY = true;
        animator.SetBool("Dead", true);
        yield return new WaitForSeconds(4f);
        Destroy(gameObject);
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
        StartCoroutine(Die());
    }

}
