using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour
{

    private GameManager gameManager;

    private Rigidbody2D rigid;
    [SerializeField] private GameObject collGO;
    private Collider2D coll;
    private bool dead = false;


    [SerializeField] private float rangeOfDetection = 6f;

    [SerializeField] private LayerMask playerLayer;
    private Collider2D detecting;

    private bool idleState = true;

    private PlayerController player;

    [SerializeField] private LayerMask groundLayer;


    [SerializeField] private LayerMask projectileLayer;

    [SerializeField] private bool vertical = false;

    private AudioManager audioManager;

    [SerializeField] private float attackCD = 1.5f;
    [SerializeField] private GameObject projectile;

    private int lives = 3;
    private bool attacking = false;
    private bool brokenPoise = false;
    private bool stunned = false;

    [SerializeField] private float stunnedTime = 3f;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        gameManager = FindObjectOfType<GameManager>();
        coll = collGO.GetComponent<Collider2D>();
        rigid = GetComponent<Rigidbody2D>();
        player = FindObjectOfType<PlayerController>();
        audioManager = FindObjectOfType<AudioManager>();

    }

    private void Update()
    {
        if (dead || attacking || stunned || brokenPoise) return;

        
        rigid.velocity = Vector2.zero;

        if (idleState)
        {
            detecting = Physics2D.OverlapCircle(transform.position, rangeOfDetection, playerLayer);
            if (detecting != null)
            {
                StartCoroutine(WaitForTime());
            }
        }
        else
        {
            
            StartCoroutine(BossAttack());
        }
    }

    private IEnumerator WaitForTime()
    {
        gameManager.BossStarted(transform.position);
        attacking = true;
        yield return new WaitForSeconds(2f);
        attacking = false;
        idleState = false;
    }

    private IEnumerator BossAttack()
    {
        
        attacking = true;
        Vector2 cPos;
        Vector2 pos;
        cPos = new(player.transform.position.x, player.transform.position.y);
        pos = new(transform.position.x, transform.position.y);
        audioManager.PlayCastIcicleShot();


        Vector2 dir = cPos - pos;
        dir.Normalize();

        if (dead) yield break;


        Instantiate(projectile, transform.position + new Vector3(dir.x, dir.y, 0), Quaternion.LookRotation(transform.forward, dir));

        animator.SetBool("Attacked", true);
        yield return new WaitForSeconds(attackCD);

        attacking = false;
    }

    public IEnumerator GotShot()
    {
        stunned = true;
        animator.SetBool("Stunned", true);
        yield return new WaitForSeconds(stunnedTime);
        animator.SetBool("Stunned", false);
        stunned = false;
    }

    public bool GetStunned()
    {
        return stunned;
    }

    public void CrystalBreak()
    {
        Debug.Log(lives);
        lives--;
        if (lives == 0)
        {
            animator.SetBool("Poised", true);
            brokenPoise = true;
        }
    }

    public bool GetPoise()
    {
        return brokenPoise;
    }

    public void GotKilled()
    {
        StartCoroutine(Die());
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
        
        yield return new WaitForSeconds(4f);
        gameManager.BossDied();
        Destroy(gameObject);
    }

}
