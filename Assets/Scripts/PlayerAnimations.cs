using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer sr;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }
    public void Died()
    {
        animator.SetBool("Dead", true);
    }

    public void Attacked()
    {
        animator.SetBool("Attacked", false);
    }
    /*
    public void CheckFlip()
    {
        if (sr.flipX) transform.position = -transform.position;
    }*/
}
