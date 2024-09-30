using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    private Rigidbody2D rigid;
    private BoxCollider2D coll;
    private GameManager gameManager;

    [SerializeField] private GameObject attackObject;
    [SerializeField] private GameObject attackHitObject;
    private Collider2D attackColl;
    private bool canAttack = true;
    private bool attacking = false;
    private float attackCD = 1f;
    private float attackTime = 0.3f;
    [SerializeField] private float attackMove = 1f;

    [SerializeField] private float speed = 10f;
    [SerializeField] private float jump = 10f;
    [SerializeField] private float dash = 10f;
    [SerializeField] private float ddelta;

    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private float height =(float) 1.03;
    private bool canDash = true;
    private bool dashing = false;
    private float dashCD = 1f;
    private float dashTime = 0.2f;
    private float horiz;
    private float dir = 1;
    private bool lookingLeft = false;

    private float maxJump = 0.2f;

    private bool touchingGround = false;


    private void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        gameManager = FindObjectOfType<GameManager>();
        attackColl = attackHitObject.GetComponent<Collider2D>();
    }
    // Update is called once per frame
    void Update()
    {
        float dist = coll.bounds.extents.y + ddelta;
        Debug.DrawRay(transform.position + new Vector3((float)0.4, 0, 0), Vector3.down * dist * height);

        RaycastHit2D hits = Physics2D.Raycast(transform.position - new Vector3((float)0.4, 0, 0), Vector2.down, dist * height, groundLayer);
        touchingGround = hits.collider != null;
        if (!touchingGround)
        {
            hits = Physics2D.Raycast(transform.position + new Vector3((float)0.4, 0, 0), Vector2.down, dist * height, groundLayer);
            touchingGround = hits.collider != null;
        }
        if (dashing || attacking) return;

        if (Input.GetKey(KeyCode.A) && horiz !=-1)
        {
            horiz = -1;
        }
        else if (Input.GetKey(KeyCode.D) && horiz != 1)
        {
            horiz = 1;
        }

        rigid.velocity = new Vector2(Input.GetAxis("Horizontal") * speed, rigid.velocity.y);
        /*
        if (horiz > 0.01 && lookingLeft)
        {
            dir = 1;
            lookingLeft = false;
            transform.localScale = Vector3.one;
        }
        else if (horiz < -0.01 && !lookingLeft)
        {
            dir = -1;
            lookingLeft = true;
            transform.localScale = new Vector3(-1, 1, 1);
        }
        */
        if (Input.GetKeyDown(KeyCode.Space) && touchingGround)
        {
            StartCoroutine(Jump());
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(Dash());
        }

        if(Input.GetMouseButtonDown(0))
        {
            StartCoroutine(Attack1());
        }

        if (rigid.velocity.y < 0) rigid.gravityScale = 2;
        else rigid.gravityScale = 3;
    }

    private IEnumerator Dash()
    {
        canDash = false;
        dashing = true;
        float aux = rigid.gravityScale;
        rigid.gravityScale = 0;
        rigid.velocity = new Vector2(transform.localScale.x * dash, 0f);
        yield return new WaitForSeconds(dashTime);
        rigid.gravityScale = aux;
        dashing = false;
        yield return new WaitForSeconds(dashCD);
        canDash = true;
    }
    private IEnumerator Jump()
    {
        rigid.velocity = new Vector2(rigid.velocity.x, jump);
        touchingGround = false;
        float aux = 0;
        while (Input.GetKey(KeyCode.Space) && aux < maxJump)
        {
            yield return new WaitForSeconds(0.05f);
            aux += 0.05f;
            rigid.velocity = new Vector2(rigid.velocity.x, jump);
        }
    }

    private IEnumerator Attack1()
    {
        Vector2 cPos = gameManager.CrossPos();
        
        canAttack = false;
        attacking = true;
        float grav = rigid.gravityScale;
        rigid.gravityScale = 0;
        Vector2 pos = new Vector2 (transform.position.x, transform.position.y);
        Vector2 dir = cPos - pos;
        dir.Normalize();
        Debug.Log(dir);
        attackHitObject.transform.right = cPos - pos;
        attackColl.enabled = true;
        attackObject.SetActive(true);
        rigid.AddForce (dir * attackMove);
        yield return new WaitForSeconds(attackTime);
        attackHitObject.transform.right = Vector2.right;
        attackColl.enabled = false;
        attackObject.SetActive(false);
        rigid.gravityScale = grav;
        attacking = false;
        yield return new WaitForSeconds(attackCD);
        canAttack = true;
    }

}
