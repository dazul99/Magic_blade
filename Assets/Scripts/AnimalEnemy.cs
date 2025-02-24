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

    //variable para entrar solo una vez en el suspicious state
    private bool notEnterAgain = false;

    //LP = last position
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

    //Inicialización de variables relacionadas con los componentes del objeto

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

    //Inicialización de variables relacionadas con otros objetos
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
        //si no vuelan ni son a rango detectan al player de forma normal, en una dirección

        if (!flying && !ranged) Detect();

        //si vuelan o son a rango detectan en un radio

        else DetectRadius();

        //si el enemigo está stun hay que hacer return, y lo mismo si ataca, para que no se mueva ni ataque durante un ataque
        if (stunned)
        {
            rigid.velocity = Vector2.zero;
            return;
        }
        if (attacking) return;

        detecting = null;

        //girar el sprite si se da la vuelta el enemigo
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

        //si tiene velocidad empezará la animación de correr (si tiene)
        animator.SetFloat("Speed", Mathf.Abs(rigid.velocity.x) + Mathf.Abs(rigid.velocity.y));


        if (!dead)
        {
            //Revisar en que estado se encuentra el enemigo, Tiene tres estados posible

            //Idle state: regresar a la posición original
            if (idleState)
            {
                IdleState();
                return;
            }

            //Chasing State: perseguir al player y atacar si puede
            //NO SACAR DE UPDATE, usa mucho el return y sería dificil adaptar a una función
            else if (chasingState)
            {
                if (notEnterAgain) notEnterAgain = false;

                //Comprobar si aún ve al player

                direction = player.transform.position - transform.position;
                direction.Normalize();

                CalculateRaycastOrigin();
                Debug.DrawRay(transform.position + offset, direction * rangeOfVision);
                chasing = Physics2D.RaycastAll(transform.position + offset, direction, rangeOfVision);

                ChasingCheck();

                if (!chasingState) return;
                Debug.DrawRay(transform.position, new Vector2(rangeOfAttack, 0));

                //si es un enemigo a rango ataca desde donde esté
                if (ranged)
                {
                    attacking = true;   
                    StartCoroutine(RangedAttack());
                    return;
                }

                //acercarse si no está lo suficientemente cerca

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
                else //si está suficientemente cerca atacar
                {
                    if(!flying) rigid.velocity = new Vector2(0, rigid.velocity.y);
                    else rigid.velocity = new Vector2(0,0);
                    attacking = true;
                    StartCoroutine(MeleeAttack());
                    
                }

                

            }


            //Suspicious State: el enemigo busca al Player cuando lo pierde de vista
            //solo se entra una vez porque empieza una corutina 
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
        //Si es un enemigo a rango no ha de entrar en suspicious state
        if (ranged)
        {
            suspiciousState = false;
            idleState = true;
            StartCoroutine(MovementRanged());
            return;
        }
        if (lastPosition != null) //mirar si tenemos una última posición
        {
            distanceToLP = Mathf.Abs(lastPosition.x - transform.position.x);
            if (distanceToLP < 1f) //mirar si está lo suficientemente cerca de esa posición
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
        if (distanceToLP > 1f) //mirar si está lejos de su posición inicial
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
   
    //Funcion para que el enemigo vaya alternando entre buscar a la izquierda y a la derecha cuando está en suspicious state
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

    //Funcion para calcular el origen de los raycasts de detección para los enemigos que no detectan con radio
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

    //Comprueba si en el array que se ha hecho en el Chasing State se encuentra el player y si no hay una pared antes
    //Resumen: si aún lo ve
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

    //Manejo del Raycast para la detección del player
    private void DetectingCheck()
    {
        if (detecting.Length > 0)
        {
            //mientras no encuentre una pared ni al player no sale sel bucle hasta revisar todo con lo que ha chocado el rayo
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

    public void UpdateRoom(Room r)
    {
        if (r != currentRoom)
        {
            currentRoom = r;
        }
    }

    //Funcion que comprueba si una explosión o un ataque le da realmente al objetivo
    //El collider de una explosión puede atravesar una pared, pero en ese caso el enemigo no ha de morir
    //Esta funcion sirve para saber si la explosión acierta realmente al enemigo o hay una pared en medio
    private bool ItHits(Collider2D collision)
    {
        Vector2 aux1 = coll.ClosestPoint(collision.gameObject.transform.position);
        Vector2 dir = new(-aux1.x + collision.gameObject.transform.position.x, -aux1.y + collision.gameObject.transform.position.y);
        float dist = Mathf.Sqrt(dir.x * dir.x + dir.y * dir.y);

        attackHits = Physics2D.Raycast(aux1, dir.normalized, dist, groundLayer);
        return attackHits.collider == null;
    }

    //Funcion para que los enemigos a rango hagan un pequeño movimiento bailarín
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
