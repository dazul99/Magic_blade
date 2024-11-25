using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraMovement : MonoBehaviour
{
    private PlayerController player;

    private Collider2D coll;

    private Bounds bound;
    private Vector2 xBounds;
    private Vector2 yBounds;

    private float xMovement;
    private float yMovement;
    private float playerX;
    private float playerY;

    private void Awake()
    {
        coll = GetComponent<Collider2D>();

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
        playerX = player.transform.position.x;
        playerY = player.transform.position.y;
        xMovement = playerX - transform.position.x;
        yMovement = playerY - transform.position.y;
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

}
