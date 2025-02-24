using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class CameraMovement : MonoBehaviour
{
    private PlayerController player;

   [SerializeField] private Collider2D coll;

    private Bounds bound;
    private Vector2 xBounds;
    private Vector2 yBounds;

    private float xMovement;
    private float yMovement;
    private float playerX;
    private float playerY;

    private bool moving;

    private float bossSize = 13f;
    private Vector3 bossPos = new Vector3(8.7f, 2.89f, -10);


    private void Awake()
    {

        bound = coll.bounds;
        xBounds.x = bound.center.x + bound.extents.x;
        xBounds.y = bound.center.x - bound.extents.x;
        yBounds.x = bound.center.y + bound.extents.y;
        yBounds.y = bound.center.y - bound.extents.y;
    }
    private void Start()
    {
        player = FindObjectOfType<PlayerController>();
        
    }

    private void Update()
    {
        if (moving) return;
        playerX = player.transform.position.x;
        playerY = player.transform.position.y;
        xMovement = playerX - transform.position.x;
        yMovement = playerY - transform.position.y;

        //La camara no puede salir de los bounds del collider del gameobject padre 
        if(playerX > transform.position.x)
        {
            if (transform.position.x >= xBounds.x) xMovement = 0;

        }
        else if(playerX < transform.position.x)
        {
            if (transform.position.x <= xBounds.y) xMovement = 0;
        }

        if (playerY > transform.position.y)
        {
            if (transform.position.y >= yBounds.x) yMovement = 0;

        }
        else if (playerY < transform.position.y)
        {
            if (transform.position.y <= yBounds.y) yMovement = 0;
        }

        transform.Translate(new Vector3(xMovement * 3f , yMovement * 3, 0) * Time.deltaTime);
    }

    //La camera se centra en la arena del boss y se aleja para verlo todo
    public void LockIn(Vector2 pos)
    {
        gameObject.GetComponent<Camera>().orthographicSize = bossSize;
        moving = true;
        transform.localPosition = bossPos;
        xBounds = new Vector2(pos.x + 10, pos.x - 10);
        yBounds = new Vector2(pos.y + 10, pos.y);

    }

}
