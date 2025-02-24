using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Unity.Burst.Intrinsics.X86;

public enum SecondaryATKs
{
    IcicleShot,
    DefinitiveShield,
    Fireball
}

public enum UniqueSkill
{
    BluePlanet,
    TurqoiseSplash
}

public class PlayerController : MonoBehaviour
{

    private Rigidbody2D rigid;
    [SerializeField] private GameObject collGO;
    private CapsuleCollider2D coll;
    private GameManager gameManager;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private PhysicsMaterial2D material;

    //Variables for the standard attack
    [SerializeField] private GameObject attackObject;
    [SerializeField] private GameObject attackHitObject;
    private Collider2D attackColl;
    private bool canAttack = true;
    private bool attacking = false;
    [SerializeField] private float attackCD = 1f;
    private float attackTime = 0.3f;
    [SerializeField] private float attackMove = 1f;

    //SA = Secondary Attack
    [SerializeField] private SecondaryATKs currentSA;

    private bool canUseScndry = true;
    private int[] maxUses;

    //CD = cooldown
    private float defShieldCD = 1f;
    [SerializeField] private float shieldDuration = 1f;


    private float IceShotCD = 1f;
    [SerializeField] private GameObject icicleShotObject;

    [SerializeField] private GameObject fire;
    private float fireCD = 12f;

    [SerializeField] private float speed = 10f;
    [SerializeField] private float jump = 10f;
    [SerializeField] private float ddelta;

    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private float height = (float)1.03;
    [SerializeField] private float width = 0.5f;
    private float horiz;

    //acc = acceleration
    private float acc = 0;

    private float maxJump = 0.2f;

    private RaycastHit2D groundHits;
    private RaycastHit2D wallHits;
    private bool touchingGround = false;
    private bool goingthrough = false;

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

    [SerializeField] private UIManager uiManager;

    [SerializeField] private int maxUsesSecondaryATK;
    private int currentUsesSecondaryATK;



    [SerializeField] private float distanceOfSpecialDash;
    [SerializeField] private GameObject specialDashGO;
    private bool hasUsedSpecialDash = false;
    [SerializeField] private GameObject specialDashDangerZone;
    [SerializeField] private GameObject dangerZoneCenter;

    private RaycastHit2D[] dashHits;
    private bool specialDashing;

    [SerializeField] private UniqueSkill currentUS;
    [SerializeField] private int maxUSCharges;
    [SerializeField] private int currentUSCharges;

    [SerializeField] private float durationBP;
    [SerializeField] private float timeSlowed = 0.2f;

    private LineRenderer turqoiseSplashRenderer;
    private RaycastHit2D turqoiseSplashHit;
    [SerializeField] private float durationTS;
    private bool shootingRay = false;
    private Vector2 turqoiseSplashDir;
    private RaycastHit2D[] enemiesToKill;
    [SerializeField] private LayerMask enemyLayer;
    private float distanceOfTS;

    private bool onWorkStation;
    private bool working;
    private Door door;
    private bool paused = false;
    private bool locked = false;

    private AudioManager audioManager;
    [SerializeField] private float wallRaycastLeft;
    [SerializeField] private float wallRaycastRight;
    [SerializeField] private float groundRaycastLeft;
    [SerializeField] private float groundRaycastRight;

    [SerializeField] private Animator animator;
    [SerializeField] private Animator slashAnimator;

    private void Awake()
    {
        maxUses = new int[3];
        maxUses[0] = 7; 
        maxUses[1] = 50;
        maxUses[2] = 3; 
        turqoiseSplashRenderer = GetComponent<LineRenderer>();
        turqoiseSplashRenderer.enabled = false;
        rigid = GetComponent<Rigidbody2D>();
        coll = collGO.GetComponent<CapsuleCollider2D>();


    }

    private void Start()
    {
        specialDashGO.transform.localScale = new Vector2(distanceOfSpecialDash * 2, distanceOfSpecialDash * 2);
        specialDashGO.SetActive(false);
        specialDashDangerZone.SetActive(false);
        dangerZoneCenter.transform.localScale = new Vector3(distanceOfSpecialDash, 0.25f, 0.25f);
        gameManager = FindObjectOfType<GameManager>();
        attackColl = attackHitObject.GetComponent<Collider2D>();
        audioManager = FindObjectOfType<AudioManager>();
        uiManager.GameStarted();
        StartCoroutine(WalkingSounds());
    }
    // Update is called once per frame
    void Update()
    {


        if (dead) return;

        animator.SetFloat("YSpeed", rigid.velocity.y);
        animator.SetFloat("XSpeed", Mathf.Abs(rigid.velocity.x));


        CheckGround();

        CheckWalls();



        if (attacking || climbing || shielding || working || paused)
        {
            return;
        }

        if (specialDashing)
        {
            Vector2 dir = Direction();
            dangerZoneCenter.transform.right = dir;
            if (Input.GetKeyUp(KeyCode.LeftShift) && !hasUsedSpecialDash)
            {
                ExitDashMode();
                specialDashDangerZone.SetActive(false);
                uiManager.UsedSpecialDash();
            }
            return;
        }

        if (shootingRay)
        {
            rigid.velocity = new Vector2(0f, 0f);
            rigid.gravityScale = 0;
            enemiesToKill = Physics2D.RaycastAll(transform.position, turqoiseSplashDir, distanceOfTS, enemyLayer);
            if (enemiesToKill.Length > 0) KillEnemies();
            enemiesToKill = Physics2D.RaycastAll(transform.position - new Vector3(0, 0.5f, 0), turqoiseSplashDir, distanceOfTS, enemyLayer);
            if (enemiesToKill.Length > 0) KillEnemies();
            enemiesToKill = Physics2D.RaycastAll(transform.position + new Vector3(0, 0.5f, 0), turqoiseSplashDir, distanceOfTS, enemyLayer);
            if (enemiesToKill.Length > 0) KillEnemies();
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

        if (locked) return;

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

        if (Input.GetKeyDown(KeyCode.Space) && currentUSCharges >= maxUSCharges)
        {
            UseUniqueSkill();
            currentUSCharges = 0;
            uiManager.ChangeUniqueSkill(0);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (onWorkStation)
            {
                ChangingSkills();
            }
            else if (door != null)
            {

                door.Open();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
        }

        //BORRAR DESPUÉS
        if (Input.GetKey(KeyCode.P) && Input.GetKeyDown(KeyCode.L))
        {
            HasKilled();
        }

        if (Input.GetKey(KeyCode.P) && Input.GetKeyDown(KeyCode.M))
        {
            SceneManager.LoadScene(1);
        }

    }

    private void PauseGame()
    {
        Time.timeScale = 0f;
        paused = true;
        uiManager.ShowPausePanel();
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        paused = false;
    }

    private void ChangingSkills()
    {
        audioManager.PlayWorkspace();
        uiManager.ShowWorkStation();
        Time.timeScale = 0f;
        working = true;
    }

    public void ExitStation()
    {
        Time.timeScale = 1f;
        working = false;
    }

    private void KillEnemies()
    {
        audioManager.PlayDeathEnemyLaser();
        foreach (RaycastHit2D enemy in enemiesToKill)
        {
            enemy.collider.gameObject.GetComponentInParent<AnimalEnemy>().Kill();
        }
    }

    private void UseUniqueSkill()
    {
        if (currentUS == global::UniqueSkill.BluePlanet) StartCoroutine(BluePlanet());
        else if (currentUS == global::UniqueSkill.TurqoiseSplash) StartCoroutine(TurqoiseSplash());
        else return;
    }

    private IEnumerator TurqoiseSplash()
    {
        turqoiseSplashDir = Direction();
        turqoiseSplashHit = Physics2D.Raycast(transform.position, turqoiseSplashDir, 200, groundLayer);
        turqoiseSplashRenderer.SetPosition(0, transform.position);
        turqoiseSplashRenderer.SetPosition(1, turqoiseSplashHit.point);
        distanceOfTS = Mathf.Sqrt((Mathf.Abs(turqoiseSplashHit.point.x - transform.position.x) * Mathf.Abs(turqoiseSplashHit.point.x - transform.position.x)) + (Mathf.Abs(turqoiseSplashHit.point.y - transform.position.y) * Mathf.Abs(turqoiseSplashHit.point.y - transform.position.y)));
        shootingRay = true;
        audioManager.PlayLaser();
        turqoiseSplashRenderer.enabled = true;
        yield return new WaitForSeconds(durationTS);
        shootingRay = false;
        rigid.gravityScale = 3;
        turqoiseSplashRenderer.enabled = false;
    }

    private IEnumerator BluePlanet()
    {
        Time.timeScale = timeSlowed;
        uiManager.UsingBluePlanet();
        yield return new WaitForSeconds(durationBP * timeSlowed);
        uiManager.EndedBluePlanet();
        Time.timeScale = 1f;
    }

    private void EnterDashMode()
    {
        Time.timeScale = 0.2f;
        specialDashGO.SetActive(true);
        specialDashing = true;
        specialDashDangerZone.SetActive(true);
    }

    private void ExitDashMode()
    {
        audioManager.PlayDash();
        hasUsedSpecialDash = true;
        Time.timeScale = 1f;
        specialDashGO.SetActive(false);
        Vector2 dir = Direction();
        dashHits = Physics2D.RaycastAll(transform.position, dir, distanceOfSpecialDash * 4);
        Vector2 finalPos = new Vector3(-1000, -1000, -1000);

        foreach (RaycastHit2D rayh in dashHits)
        {
            if (rayh.collider.gameObject.CompareTag("Enemy"))
            {
                rayh.collider.gameObject.GetComponentInParent<AnimalEnemy>().Kill();

            }
            else if (rayh.collider.gameObject.CompareTag("Crystal"))
            {
                rayh.collider.gameObject.GetComponentInParent<Crystal>().Break();

            }
            else if (rayh.collider.gameObject.CompareTag("Wall") || rayh.collider.gameObject.CompareTag("Door"))
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

        if (finalPos == new Vector2(-1000, -1000)) finalPos = transform.position + (new Vector3(dir.x, dir.y, 0) * distanceOfSpecialDash * 4);
        Vector2 aux = new Vector2(transform.position.x, transform.position.y);
        dashHits = Physics2D.RaycastAll(transform.position - new Vector3(0, 1, 0), dir, distanceOfSpecialDash * 4);
        foreach (RaycastHit2D rayh in dashHits)
        {
            if (rayh.collider.gameObject.CompareTag("Enemy"))
            {
                rayh.collider.gameObject.GetComponentInParent<AnimalEnemy>().Kill();
            }
        }

        dashHits = Physics2D.RaycastAll(transform.position + new Vector3(0, 1, 0), dir, distanceOfSpecialDash);
        foreach (RaycastHit2D rayh in dashHits)
        {
            if (rayh.collider.gameObject.CompareTag("Enemy"))
            {
                rayh.collider.gameObject.GetComponentInParent<AnimalEnemy>().Kill();
            }
        }

        specialDashing = false;
        //mover el pj hasta el final del rayo o hasta la pared con la que choque

        transform.Translate(finalPos - aux);

    }

    private IEnumerator WalkingSounds()
    {
        int aux = 1;
        while (!dead)
        {
            if (aux == 4) aux = 1;
            if (acc != 0)
            {
                audioManager.PlayStep(aux);
                aux++;
            }
            yield return new WaitForSeconds(0.4f);
        }
    }

    private void Attack2(SecondaryATKs sA)
    {
        currentUsesSecondaryATK--;
        if (sA == SecondaryATKs.IcicleShot)
        {
            IcicleShot();
        }
        else if (sA == SecondaryATKs.DefinitiveShield)
        {
            StartCoroutine(DefinitiveShield());
        }
        else if (sA == SecondaryATKs.Fireball)
        {
            Fireball();
        }
        else
        {
            return;
        }
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
        Vector2 aux = Vector2.zero;

        if (wallLeft)
        {

            aux = Vector2.right + Vector2.up;
            jumpedLeft = true;
            FlipSprite(true);
        }
        else if (wallRight)
        {
            aux = Vector2.left + Vector2.up;
            jumpedRight = true;
            FlipSprite(false);
        }
        audioManager.PlayJump();
        aux.Normalize();
        rigid.velocity = Vector2.zero;
        rigid.AddForce(aux * 700);
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

    private void CheckGround()
    {
        if (rigid.velocity.y > 0 && !goingthrough)
        {
            gameManager.SetPlatformsTrigger(true);
            goingthrough = true;
        }
        else if (rigid.velocity.y <= 0 && goingthrough)
        {
            gameManager.SetPlatformsTrigger(false);
            goingthrough = false;
        }

        float dist = 0.05f + ddelta;
        Debug.DrawRay(transform.position - new Vector3(groundRaycastLeft, 0.99f, 0), Vector3.down * ddelta);
        Debug.DrawRay(transform.position + new Vector3(groundRaycastRight, -0.99f, 0), Vector3.down * ddelta);
        groundHits = Physics2D.Raycast(transform.position - new Vector3(groundRaycastLeft, 1, 0), Vector2.down, dist, groundLayer);
        touchingGround = groundHits.collider != null;
        if (touchingGround && groundHits.collider.gameObject.CompareTag("ThroughFloor") && (Mathf.Abs(rigid.velocity.y) > 0.01f)) touchingGround = false;
        if (!touchingGround)
        {
            groundHits = Physics2D.Raycast(transform.position + new Vector3(groundRaycastRight, -1, 0), Vector2.down, dist, groundLayer);
            touchingGround = groundHits.collider != null;
            if (touchingGround && groundHits.collider.gameObject.CompareTag("ThroughFloor") && (Mathf.Abs(rigid.velocity.y) > 0.01f)) touchingGround = false;
        }

        if (touchingGround)
        {
            if (jumpedLeft) jumpedLeft = false;
            if (jumpedRight) jumpedRight = false;
        }
    }

    private void CheckWalls()
    {
        float dist = 0.07f + ddelta;

        Debug.DrawRay(transform.position - new Vector3(wallRaycastLeft, 0, 0), Vector3.left * ddelta);
        Debug.DrawRay(transform.position - new Vector3(wallRaycastRight, 0, 0), Vector3.right * ddelta);
        Debug.DrawRay(transform.position - new Vector3(wallRaycastRight, 0.95f, 0), Vector3.right * ddelta);
        wallHits = Physics2D.Raycast(transform.position - new Vector3(wallRaycastLeft, 0, 0), Vector2.left, dist, groundLayer);
        wallLeft = wallHits.collider != null && wallHits.collider.gameObject.CompareTag("Wall");
        if (!wallLeft)
        {
            wallHits = Physics2D.Raycast(transform.position - new Vector3(wallRaycastLeft, -0.95f, 0), Vector2.left, dist, groundLayer);
            wallLeft = wallHits.collider != null && wallHits.collider.gameObject.CompareTag("Wall");
        }

        if (!wallLeft)
        {

            wallHits = Physics2D.Raycast(transform.position - new Vector3(wallRaycastRight, 0, 0), Vector2.right, dist, groundLayer);
            wallRight = wallHits.collider != null && wallHits.collider.gameObject.CompareTag("Wall");
            if (!wallRight)
            {
                wallHits = Physics2D.Raycast(transform.position - new Vector3(wallRaycastRight, 0.95f, 0), Vector2.right, dist, groundLayer);
                wallRight = wallHits.collider != null && wallHits.collider.gameObject.CompareTag("Wall");

            }
        }

        if (wallLeft && jumpedRight)
        {
            horiz = -1;
            //acc = 0.5f;
            jumpedRight = false;
            
            if (!spriteRenderer.flipX)
            {
                FlipSprite(true);
            }
        }
        if (wallRight && jumpedLeft)
        {
            horiz = 1;
            jumpedLeft = false;
            
            //acc = 0;
            if (spriteRenderer.flipX)
            {
                FlipSprite(false);
            }
        }
    }

    private void Movement()
    {
        Vector2 mov = new Vector2(0, rigid.velocity.y);
        if (locked)
        {
            horiz = 1;
            acc = 1;
            mov.x = horiz * speed * acc;
            rigid.velocity = mov;
            return;
        }
        if (jumpedLeft || jumpedRight)
        {
            //acc = 0;
            return;
        }


        if (Input.GetKey(KeyCode.A))
        {

            horiz = -1;
            if (!spriteRenderer.flipX)
            {
                FlipSprite(false);
                acc = 0;
            }
            if (acc <= 1) acc += 0.03f;

        }
        else if (Input.GetKey(KeyCode.D))
        {

            horiz = 1;
            if (spriteRenderer.flipX)
            {
                FlipSprite(true);
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


    private IEnumerator Jump()
    {
        rigid.velocity = new Vector2(rigid.velocity.x, jump);
        touchingGround = false;
        audioManager.PlayJump();
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
        animator.SetBool("Attacked", true);

        audioManager.PlaySwordAttack();
        float aux = acc;
        canAttack = false;
        attacking = true;
        float grav = rigid.gravityScale;
        rigid.gravityScale = 0;
        Vector2 dir = Direction();
        attackHitObject.transform.right = dir;
        attackColl.enabled = true;
        attackObject.SetActive(true);
        slashAnimator.SetBool("Attacked", true);
        rigid.velocity = new Vector2(rigid.velocity.x / 2, rigid.velocity.y / 2);
        rigid.AddForce(dir * attackMove);


        yield return new WaitForSeconds(attackTime);
        attackHitObject.transform.right = Vector2.right;
        attackColl.enabled = false;
        slashAnimator.SetBool("Attacked", false);
        attackObject.SetActive(false);
        rigid.gravityScale = grav;
        attacking = false;
        if (dir.x / horiz > 0) acc = aux;
        else acc = 0.3f * aux;
        uiManager.UsedNormalAttack(attackCD);

    }

    private void IcicleShot()
    {
        Vector2 dir = Direction();
        Vector3 aux = new Vector3(0, 0, 0);
        aux = dir;
        canUseScndry = false;
        Instantiate(icicleShotObject, transform.position + aux, Quaternion.LookRotation(transform.forward, dir));
        uiManager.UsedSecondAttack(IceShotCD);
        audioManager.PlayCastIcicleShot();
    }

    private IEnumerator DefinitiveShield()
    {
        shielding = true;
        canUseScndry = false;
        rigid.velocity = Vector3.zero;
        acc = 0f;
        spriteRenderer.color = Color.blue;
        yield return new WaitForSeconds(shieldDuration);
        spriteRenderer.color = Color.white;

        shielding = false;
        uiManager.UsedSecondAttack(defShieldCD);
    }

    private void Fireball()
    {
        Vector2 dir = Direction();
        Vector3 aux = new Vector3(0, 0, 0);
        aux = dir;
        canUseScndry = false;
        audioManager.PlayCastFireball();
        Instantiate(fire, transform.position + aux, Quaternion.LookRotation(transform.forward, dir));
        uiManager.UsedSecondAttack(fireCD);


    }

    public void SecondaryCDDone()
    {
        canUseScndry = true;
    }

    public void NormalCDDone()
    {
        canAttack = true;
    }

    public void ResetGoThrough(bool x)
    {
        goingthrough = x;
    }

    public void SetDoor(Door d)
    {
        if (d != null) uiManager.ShowInteractable();
        else uiManager.HideInteractable();
        door = d;
    }

    public void SetWS(bool ws)
    {
        if (ws) uiManager.ShowInteractable();
        else uiManager.HideInteractable();
        onWorkStation = ws;
    }

    public void SetRoom(Room r)
    {
        currentRoom = r;
    }

    public void SetLadder(Ladder l)
    {
        if (l != null)
        {
            currentLadder = l;
        }


    }

    public void SetLadderEnd(LadderEnd ladderEnd)
    {
        if (ladderEnd.GetTop())
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

    public void ExitLadder(Ladder l)
    {
        if (currentLadder != null && l == currentLadder && !climbing)
        {
            currentLadder = null;
        }

        ladderBottom = ladderTop = false;
    }

    public void GotHit(AnimalEnemy aE)
    {
        if (attacking)
        {
            Vector2 enemypos = aE.gameObject.transform.position;
            Vector2 dir = new Vector2(transform.position.x - enemypos.x, transform.position.y - enemypos.y);
            dir.Normalize();

            rigid.AddForce(dir * 400);
            audioManager.PlaySwordClash();
            aE.Knockback(-dir);
        }
        else if (shielding)
        {
            audioManager.PlayParry();
            aE.Stun(stunTime);
        }
        else if (!dead)
        {
            Die();

        }
    }

    public void GotShot(Projectile p)
    {
        if (attacking)
        {
            Vector2 dir = Direction();
            p.Return(dir);
            return;
        }
        Die();
    }

    public void DiePublic()
    {
        if (!shielding && !dead)
        {
            Die();
        }
    }

    private void Die()
    {
        animator.SetBool("Died", true);
        audioManager.PlayDeathPlayer();
        rigid.velocity = Vector2.zero;
        dead = true;
        StartCoroutine(GameOver());
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

    public void HasKilled()
    {
        if (currentUSCharges < maxUSCharges)
        {
            currentUSCharges++;
            uiManager.ChangeUniqueSkill(currentUSCharges);
        }

    }

    public int GetUSCharges()
    {
        return currentUSCharges;
    }

    public void SetUSCharges(int x)
    {
        currentUSCharges = x;
        uiManager.ChangeUniqueSkill(currentUSCharges);
    }

    public int CurrentUS()
    {
        return (int)currentUS;
    }

    public int CurrentScndryA()
    {
        return (int)currentSA;
    }

    public void SetUniqueSkill(int x)
    {
        currentUS = (UniqueSkill)x;
    }

    public void SetSecondaryAttack(int x)
    {
        currentSA = (SecondaryATKs)x;
        maxUsesSecondaryATK = maxUses[x];
        currentUsesSecondaryATK = maxUsesSecondaryATK;
        uiManager.UpdateSecondAttack(maxUses[x], x);
    }

    public IEnumerator GameOver()
    {
        yield return new WaitForSeconds(0.5f);
        Time.timeScale = 0f;
        uiManager.GameOver();
    }


    public void LockMovement()
    {
        locked = true;
    }

    public void UnlockMovement()
    {
        locked = false;
        acc = 0;
    }

    public void EnteredEndOfLevel()
    {
        gameManager.GotToEnd();
    }

    private void FlipSprite(bool right)
    {
        if (right && spriteRenderer.flipX)
        {
            spriteRenderer.flipX = false;
            coll.transform.localScale = Vector3.one;
            wallRaycastLeft -= 0.26f;
            wallRaycastRight -= 0.26f;
            groundRaycastLeft -= 0.45f;
            groundRaycastRight += 0.45f;
            return;
        }
        if(!right && !spriteRenderer.flipX)
        {
            spriteRenderer.flipX = true;
            coll.transform.localScale = new Vector3(-1, 1, 1);
            wallRaycastLeft += 0.26f;
            wallRaycastRight += 0.26f;
            groundRaycastLeft += 0.45f;
            groundRaycastRight -= 0.45f;
            return;
        }
        return;
    }
}
