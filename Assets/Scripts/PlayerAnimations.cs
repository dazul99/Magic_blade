using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private Animator slashAnimator;
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
        slashAnimator.SetBool("Attacked", false);

    }
}
