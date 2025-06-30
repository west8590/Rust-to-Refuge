using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 4.5f;
    Animator animator;
    Rigidbody2D rb;
    bool diag = false;
    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        animator = gameObject.GetComponent<Animator>();
    }
    private void Update()
    {
        float currentSpeed = walkSpeed;
        if ((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)) &&
            (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)))
        {
            currentSpeed /= Mathf.Sqrt(2);
            diag = true;
        }
        Vector2 move = Vector2.zero;
        if (Input.GetKey(KeyCode.W)) move.y += 1;
        if (Input.GetKey(KeyCode.S)) move.y -= 1;
        if (Input.GetKey(KeyCode.D)) move.x += 1;
        if (Input.GetKey(KeyCode.A)) move.x -= 1;
        rb.linearVelocity = move * currentSpeed;
        transform.rotation = Quaternion.identity;
        animator.SetBool("Moving", move != Vector2.zero);
        if (!diag)
        {
            if (move.y > 0)
            {
                animator.SetInteger("Direction", 2);
            }
            if (move.y < 0)
            {
                animator.SetInteger("Direction", 0);
            }
            if (move.x < 0)
            {
                animator.SetInteger("Direction", 1);
            }
            if (move.x > 0)
            {
                animator.SetInteger("Direction", 3);
            }
        }
        else
        {
            if (move.x < 0)
            {
                animator.SetInteger("Direction", 1);
            }
            if (move.x > 0)
            {
                animator.SetInteger("Direction", 3);
            }
        }
        diag = false;
    }
}