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
    private RaycastHit2D[] detecting;

    private bool idleState = true;

    private PlayerController player;

    private RaycastHit2D attackHits;
    [SerializeField] private LayerMask groundLayer;


    private bool canDeflect = true;
    [SerializeField] private LayerMask projectileLayer;

    [SerializeField] private bool vertical = false;

    private AudioManager audioManager;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        coll = collGO.GetComponent<Collider2D>();
        rigid = GetComponent<Rigidbody2D>();
        player = FindObjectOfType<PlayerController>();
        audioManager = FindObjectOfType<AudioManager>();

    }
}
