using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    private Rigidbody2D rigid;
    private CapsuleCollider2D coll;
    private GameManager gameManager;
    private SpriteRenderer spriteRenderer;

    [SerializeField] private PhysicsMaterial2D material;


    [SerializeField] private GameObject attackObject;
    [SerializeField] private GameObject attackHitObject;
    private Collider2D attackColl;
    private bool canAttack = true;
    private bool attacking = false;
    [SerializeField] private float attackCD = 1f;
    private float attackTime = 0.3f;
    [SerializeField] private float attackMove = 1f;

    private enum secondaryATKs
    {
        icicleShot,
        definitiveShield,
        fireball
    }

    [SerializeField] private secondaryATKs currentSA;

    private bool canUseScndry = true;

    private float defShieldCD = 1f;
    [SerializeField] private float shieldDuration = 1f;

    private float IceShotCD = 6f;
    [SerializeField] private GameObject icicleShotObject;



    [SerializeField] private GameObject fire;
    private float fireCD = 12f;

    [SerializeField] private float speed = 10f;
    [SerializeField] private float jump = 10f;
    [SerializeField] private float dash = 10f;
    [SerializeField] private float ddelta;

    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private float height =(float) 1.03;
    [SerializeField] private float width = 0.5f;
    private bool canDash = true;
    private bool dashing = false;
    private float dashCD = 0.5f;
    private float dashTime = 0.2f;
    private float horiz;
    private float acc=0;

    private float maxJump = 0.2f;

    private RaycastHit2D groundHits;
    private RaycastHit2D wallHits;
    private bool touchingGround = false;
    private bool goingthrough = false;

    private bool crouching = false;
    private Vector2 crouchOffset = new Vector2(0, -0.5f);
    private Vector2 crouchSize = new Vector2(1f, 1f);
    private Vector2 normalSize = new Vector2(1f, 2f);

    private bool wallLeft = false;
    private bool wallRight = false;
    private bool jumpedLeft = false;
    private bool jumpedRight = false;
    private float wallJumpCD = 0.5f;

    private bool ladderTop;
    private bool ladderBottom;
    private bool climbing;
    [SerializeField] private float ladderSpeed;
    private Ladder currentLadder;

    private Room currentRoom;


    private bool dead;
    private bool shielding = false;
    [SerializeField] private float stunTime = 3f;

    private UIManager uiManager;

    [SerializeField] private int maxUsesSecondaryATK;
    private int currentUsesSecondaryATK;

    private enum uniqueSkill
    {
        bluePlanet,
        turqoiseSplash
    }

    [SerializeField] private uniqueSkill currentUS;



    [SerializeField] private float timeSlowed = 0.2f;
    [SerializeField] private float distanceOfSpecialDash;
    [SerializeField] private GameObject specialDashGO;
    private bool hasUsedSpecialDash = false;

    private RaycastHit2D[] dashHits;
    private bool specialDashing;

    private void Start()
    {
        specialDashGO.transform.localScale = new Vector2 (distanceOfSpecialDash, distanceOfSpecialDash);
        specialDashGO.SetActive(false);
        currentUsesSecondaryATK = maxUsesSecondaryATK;
        rigid = GetComponent<Rigidbody2D>();
        coll = GetComponent<CapsuleCollider2D>();
        gameManager = FindObjectOfType<GameManager>();
        attackColl = attackHitObject.GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        uiManager = FindObjectOfType<UIManager>();

        uiManager.SetSecondary(maxUsesSecondaryATK);

    }
    // Update is called once per frame
    void Update()
    {
        if (dead) return;

        CheckGround();

        CheckWalls();

        

        if (dashing || attacking || climbing || shielding)
        {
            return;
        }
        if (specialDashing)
        {
            if (Input.GetKeyUp(KeyCode.LeftShift) && !hasUsedSpecialDash)
            {
                ExitDashMode();
            }
            return;
        }
        InputsUpdate();



        if (!attacking)
        {
            CheckGravity();
        }
        
        
    }

    private void InputsUpdate()
    {
        if (Input.GetKeyDown(KeyCode.S) && touchingGround)
        {
            if (ladderTop)
            {
                StartCoroutine(GoDownLadder());
                return;
            }

            PlatformThrough floor = groundHits.collider.gameObject.GetComponent<PlatformThrough>();
            if (floor != null)
            {
                floor.GoThrough();
            }

        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            if (ladderBottom)
            {
                StartCoroutine(GoUpLadder());
                return;
            }
            if (touchingGround)
            {
                StartCoroutine(Jump());
            }
            else if (wallLeft || wallRight)
            {
                WallJump();

            }
        }

        Movement();

        if (Input.GetKeyDown(KeyCode.LeftControl) && touchingGround)
        {
            GEDDAN();
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl) && touchingGround)
        {
            if (crouching)
            {
                GetUp();
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && !hasUsedSpecialDash)
        {
            EnterDashMode();
        }

        if (Input.GetMouseButtonDown(0) && canAttack)
        {
            StartCoroutine(Attack1());
        }

        if (Input.GetMouseButtonDown(1) && canUseScndry && currentUsesSecondaryATK > 0)
        {
            Attack2(currentSA);
        }
    }

    private void EnterDashMode()
    {
        Time.timeScale = 0.2f;
        specialDashGO.SetActive(true);
        specialDashing = true;
    }

    private void ExitDashMode()
    {
        specialDashing = false;
        //hasUsedSpecialDash = true;
        Time.timeScale = 1f;
        specialDashGO.SetActive(false);
        //rayo hacia el ratón
        Vector2 dir = Direction();
        dashHits = Physics2D.RaycastAll(transform.position, dir, distanceOfSpecialDash);
        //matar enemigos que toquen el rayo
        Vector2 finalPos = new Vector3(-1000, -1000, -1000);

        foreach(RaycastHit2D rayh in dashHits)
        {
            if (rayh.collider.gameObject.CompareTag("Enemy"))
            {
                rayh.collider.gameObject.GetComponent<AnimalEnemy>().Kill();
            }
            else if (rayh.collider.gameObject.CompareTag("Wall"))
            {
                finalPos = rayh.point;
                if (dir.y > 0)
                {
                    finalPos.y -= height;
                }
                else
                {
                    finalPos.y += height;
                }
                break;
            }
        }

        if (finalPos == new Vector2(-1000, -1000)) finalPos = transform.position + (new Vector3(dir.x,dir.y,0) * distanceOfSpecialDash);

        //mover el pj hasta el final del rayo o hasta la pared con la que choque

        transform.Translate(finalPos);

    }


    private void Attack2(secondaryATKs sA)
    {
        IEnumerator aux = null;
        currentUsesSecondaryATK--;
        if(sA == secondaryATKs.icicleShot)
        {
            aux = IcicleShot();
        }
        else if (sA == secondaryATKs.definitiveShield)
        {
            aux = DefinitiveShield();
        }
        else if(sA == secondaryATKs.fireball)
        {
            aux = Fireball();
        }
        else
        {
            return;
        }
        StartCoroutine(aux);
    }

    private void CheckGravity()
    {
        if ((wallLeft || wallRight) && rigid.velocity.y < 0 && rigid.gravityScale != 0.5f)
        {
            rigid.gravityScale = 0.5f;
        }
        else if (rigid.velocity.y < 0 && rigid.gravityScale != 2) rigid.gravityScale = 2;
        else if (rigid.gravityScale != 3) rigid.gravityScale = 3;
    }

    private void WallJump()
    {
        if (wallLeft)
        {

            Vector2 aux = Vector2.right * 1.5f + Vector2.up;
            aux.Normalize();
            rigid.velocity = Vector2.zero;
            rigid.AddForce(aux * 700);
            jumpedLeft = true;
        }
        else if (wallRight)
        {
            Vector2 aux = Vector2.left *1.5f + Vector2.up;
            aux.Normalize();
            rigid.velocity = Vector2.zero;
            rigid.AddForce(aux * 700);
            jumpedRight = true;
        }

        StartCoroutine(WallJumpCD());
    }

    private IEnumerator WallJumpCD()
    {
        float aux = 0;
        while (!(!jumpedRight && !jumpedLeft) && aux < wallJumpCD)
        {
            yield return new WaitForSeconds(0.05f);
            aux += 0.05f;
        }
        if (jumpedRight) jumpedRight = false;
        if (jumpedLeft) jumpedLeft = false;
    }

    private void GEDDAN()
    {
        Debug.Log("sneaky");
        coll.offset = crouchOffset;
        coll.size = crouchSize;
        speed /= 2;
        crouching = true;
    }

    private void GetUp()
    {
        Debug.Log("sneakyn't");
        coll.offset = Vector2.zero;
        coll.size = normalSize;
        speed *= 2;
        crouching = false;
    }

    private void CheckGround()
    {
        if(rigid.velocity.y > 0 && !goingthrough)
        {
            gameManager.SetPlatformsTrigger(true);
            goingthrough = true;
        }
        else if(rigid.velocity.y <= 0 && goingthrough)
        {
            gameManager.SetPlatformsTrigger(false);
            goingthrough = false;
        }

        float dist = 0.05f + ddelta;
        if (crouching) dist *= 2;
        Debug.DrawRay(transform.position - new Vector3((float)0.25, 1, 0), Vector3.down * ddelta);
        Debug.DrawRay(transform.position + new Vector3((float)0.25, -1, 0), Vector3.down * ddelta);
        groundHits = Physics2D.Raycast(transform.position - new Vector3((float)0.25, 1, 0), Vector2.down, dist, groundLayer);
        touchingGround = groundHits.collider != null;
        if (touchingGround && groundHits.collider.gameObject.CompareTag("ThroughFloor") && (Mathf.Abs(rigid.velocity.y) > 0.01f)) touchingGround = false;
        if (!touchingGround)
        {
            groundHits = Physics2D.Raycast(transform.position + new Vector3((float)0.25, -1, 0), Vector2.down, dist, groundLayer);
            touchingGround = groundHits.collider != null;
            if (touchingGround && groundHits.collider.gameObject.CompareTag("ThroughFloor") && (Mathf.Abs(rigid.velocity.y) > 0.01f)) touchingGround = false;
        }

        if (touchingGround)
        {
            if(jumpedLeft) jumpedLeft = false;
            if(jumpedRight) jumpedRight = false;
        }
    }

    private void CheckWalls()
    {
        float dist =  0.07f + ddelta;

        Debug.DrawRay(transform.position - new Vector3((float)0.5f, 0, 0), Vector3.left * ddelta);
        Debug.DrawRay(transform.position - new Vector3((float)-0.5f, 0, 0), Vector3.right * ddelta);
        Debug.DrawRay(transform.position - new Vector3((float)-0.5f, 0.95f, 0), Vector3.right * ddelta);
        wallHits = Physics2D.Raycast(transform.position - new Vector3((float)0.5f, 0, 0), Vector2.left, dist, groundLayer);
        wallLeft = wallHits.collider != null && wallHits.collider.gameObject.CompareTag("Wall");
        if (!wallLeft)
        {
            wallHits = Physics2D.Raycast(transform.position - new Vector3((float)0.5f, -0.95f, 0), Vector2.left, dist, groundLayer);
            wallLeft = wallHits.collider != null && wallHits.collider.gameObject.CompareTag("Wall");
        }

        if (!wallLeft)
        {
            
            wallHits = Physics2D.Raycast(transform.position - new Vector3((float)-0.5f, 0, 0), Vector2.right, dist, groundLayer);
            wallRight = wallHits.collider != null && wallHits.collider.gameObject.CompareTag("Wall");
            if (!wallRight)
            {
                 wallHits = Physics2D.Raycast(transform.position - new Vector3((float)-0.5f, 0.95f, 0), Vector2.right, dist, groundLayer);
                 wallRight = wallHits.collider != null && wallHits.collider.gameObject.CompareTag("Wall");

            }
        }

        if (wallLeft && jumpedRight)
        {
            horiz = -1;
            acc = 0.5f;
            jumpedRight = false;
            if (!spriteRenderer.flipX)
            {
                spriteRenderer.flipX = true;
            }
        }
        if (wallRight && jumpedLeft)
        {
            horiz = 1;
            jumpedLeft = false;
            acc = 0;
            if (spriteRenderer.flipX)
            {
                spriteRenderer.flipX = false;
            }
        }
    }

    private void Movement()
    {
        if (jumpedLeft || jumpedRight)
        {
            acc = 0;
            return;
        }
        Vector2 mov = new Vector2 (0,rigid.velocity.y);

        if (Input.GetKey(KeyCode.A))
        {
            
            horiz = -1;
            if (Input.GetKey(KeyCode.S) && touchingGround)
            {
                if(canDash) StartCoroutine(Dash());
                return;
            }
            if (!spriteRenderer.flipX)
            {
                spriteRenderer.flipX = true;
                acc = 0;
            }
            if (acc <= 1) acc += 0.03f;

        }
        else if (Input.GetKey(KeyCode.D))
        {
            
            horiz = 1;
            if (Input.GetKey(KeyCode.S) && touchingGround)
            {
                if (canDash) StartCoroutine(Dash());
                return;
            }
            if (spriteRenderer.flipX)
            {
                spriteRenderer.flipX = false;
                acc = 0;
            }
            if (acc <= 1) acc += 0.03f;

        }
        else
        {
            if (acc > 0)
            {
                acc -= 0.05f;
            }
            else acc = 0;
        }

        mov.x = horiz * speed * acc;
        rigid.velocity = mov;
    }


    private IEnumerator Dash()
    {
        Debug.Log("DASH");
        canDash = false;
        dashing = true;
        float aux = rigid.gravityScale;
        rigid.gravityScale = 0;

        rigid.velocity = new Vector2(horiz * dash, 0f);
        yield return new WaitForSeconds(dashTime);
        rigid.velocity = new Vector2(0f, 0f);
        rigid.gravityScale = aux;
        dashing = false;
        acc = 0;
        yield return new WaitForSeconds(dashCD);
        canDash = true;
    }
    private IEnumerator Jump()
    {
        rigid.velocity = new Vector2(rigid.velocity.x, jump);
        touchingGround = false;
        float aux = 0;
        while (Input.GetKey(KeyCode.W) && aux < maxJump)
        {
            yield return new WaitForSeconds(0.05f);
            aux += 0.05f;
            if (attacking) break;
            rigid.velocity = new Vector2(rigid.velocity.x, jump);
        }
    }

    private IEnumerator GoUpLadder()
    {
        climbing = true;
        coll.isTrigger = true;
        float aux = rigid.gravityScale;
        rigid.gravityScale = 0;
        acc = 0;
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
    }
    private IEnumerator GoDownLadder()
    {
        climbing = true;
        coll.isTrigger = true;
        float aux = rigid.gravityScale;
        rigid.gravityScale = 0;
        acc = 0;
        rigid.velocity = Vector2.zero;

        while (!ladderBottom)
        {
            yield return new WaitForSeconds(0.1f);
            rigid.velocity = new Vector2(0, -ladderSpeed);
        }
        coll.isTrigger = false;
        rigid.gravityScale = aux;
        climbing = false;
    }

    private IEnumerator Attack1()
    {
        
        float aux = acc;
        canAttack = false;
        attacking = true;
        float grav = rigid.gravityScale;
        rigid.gravityScale = 0;
        Vector2 dir = Direction();
        attackHitObject.transform.right =dir;
        attackColl.enabled = true;
        attackObject.SetActive(true);
        rigid.velocity = new Vector2(rigid.velocity.x/2,rigid.velocity.y/2);  
        rigid.AddForce(dir * attackMove);

        
        yield return new WaitForSeconds(attackTime);
        attackHitObject.transform.right = Vector2.right;
        attackColl.enabled = false;
        attackObject.SetActive(false);
        rigid.gravityScale = grav;
        attacking = false;
        if (dir.x / horiz > 0) acc = aux;
        else acc = 0.3f * aux;
        uiManager.UsedNormalAttack(attackCD);

        yield return new WaitForSeconds(attackCD);
        canAttack = true;
    }

    private IEnumerator IcicleShot()
    {
        Vector2 dir = Direction();
        Vector3 aux = new Vector3 (0,0,0);
        aux =  dir;
        canUseScndry = false;
        Instantiate(icicleShotObject, transform.position + aux, Quaternion.LookRotation(transform.forward, dir));
        uiManager.UsedSecondAttack(IceShotCD);
        yield return new WaitForSeconds(IceShotCD);
        canUseScndry = true;
    }

    private IEnumerator DefinitiveShield()
    {
        shielding = true;
        canUseScndry = false;
        rigid.velocity = Vector3.zero;
        acc = 0f;
        yield return new WaitForSeconds(shieldDuration);
        shielding = false;
        uiManager.UsedSecondAttack(defShieldCD);
        yield return new WaitForSeconds(defShieldCD);
        canUseScndry = true;
    }

    private IEnumerator Fireball()
    {
        Vector2 dir = Direction();
        Vector3 aux = new Vector3(0, 0, 0);
        aux = dir;
        canUseScndry = false;

        Instantiate(fire, transform.position + aux, Quaternion.LookRotation(transform.forward, dir));
        uiManager.UsedSecondAttack(fireCD);

        yield return new WaitForSeconds(fireCD); //fireCD
        canUseScndry = true;

    }


    public void ResetGoThrough(bool x)
    {
        goingthrough = x;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Room"))
        {
            currentRoom = collision.gameObject.GetComponent<Room>();
        }
        else if (collision.gameObject.CompareTag("Ladder"))
        {
            Ladder ladderaux = collision.gameObject.GetComponent<Ladder>();
            if (ladderaux != null)
            {
                currentLadder = ladderaux;
            }
            else if (collision.gameObject.GetComponent<LadderEnd>().GetTop())
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
        else if (collision.gameObject.CompareTag("EnemyAttack"))
        {
            if (attacking)
            {
                Vector2 enemypos = collision.gameObject.transform.position;
                Vector2 dir = new Vector2(transform.position.x - enemypos.x, transform.position.y - enemypos.y);
                dir.Normalize();
                
                rigid.AddForce(dir * 400);
                collision.gameObject.GetComponentInParent<AnimalEnemy>().Knockback(-dir);
            }
            else if (shielding)
            {
                collision.gameObject.GetComponentInParent<AnimalEnemy>().Stun(stunTime);
            }
            else if (!dead)
            {
                Die();
            }
        }
        else if (collision.gameObject.CompareTag("EnemyShot") && attacking)
        {
            Vector2 dir = Direction();
            collision.gameObject.GetComponentInParent<Projectile>().Return(dir);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Explosion"))
        {
            if (!shielding && !dead)
            {
                Die();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Ladder") )
        {
            if (currentLadder != null && collision.GetComponent<Ladder>() == currentLadder && !climbing)
            {
                currentLadder = null;
            }

            ladderBottom = ladderTop = false;

        }
    }

    private void Die()
    {
        rigid.velocity = Vector2.zero;
        dead = true;
    }

    public bool MeleeAttacking()
    {
        return attacking;
    }

    public Room GetRoom()
    {
        return currentRoom;
    }
    
    public bool IsClimbing()
    {
        return climbing;
    }

    public Ladder GetCurrentLadder()
    {
        return currentLadder;
    }

    private Vector2 Direction()
    {
        Vector2 cPos = gameManager.CrossPos();
        Vector2 pos = new Vector2(transform.position.x, transform.position.y);
        return (cPos - pos).normalized;
    }

}
