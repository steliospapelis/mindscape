using System.Collections;
using UnityEngine;

public class GaleneMovement : MonoBehaviour
{
    // Variables for movement
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    private bool isGrounded;
    private Rigidbody2D rb;
    private Animator anim;
    private bool isFacingRight = true;
    public bool canMove = false;       //changed to false so that she cant move while recovering at the start
    private bool isRecover = false;

    public Transform chart;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        anim.Play("recover", 0, 0);
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("recover"))
        {
            canMove = false; 
            isRecover = true;
        }

        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("recover") && isRecover)
        {
            canMove = true;
            isRecover = false;
        }


        if (canMove){
       
            // Check if the player is grounded
            isGrounded = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(0, -0.6f), 0.1f);

            // Horizontal movement
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            Vector2 movement = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
            rb.velocity = movement;

            // Flip the character if moving in the opposite direction
            if (horizontalInput > 0 && !isFacingRight)
            {
                Flip();
            }
            else if (horizontalInput < 0 && isFacingRight)
            {
                Flip();
            }

            // Jumping
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
            
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);


            }

            //Animations
            anim.SetBool("Run",horizontalInput!=0);
            anim.SetBool("Grounded",isGrounded);

        }
    }

    // Flip character sprite
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
        chart.Rotate(0f, -180f, 0f);
    }

}

