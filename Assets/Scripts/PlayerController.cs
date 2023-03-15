using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // movement variables
    [SerializeField] Rigidbody2D rbody;
    private float horizInput;
    private float moveSpeed = 450.0f;  // 4.5 * 100 since we're using Rigidbody physics to move

    // jump variables
    private float jumpHeight = 3.0f;
    private float jumpTime = 0.75f;
    private float initialJumpVelocity;

    // to keep from unlimited jumping
    private bool isGrounded = false;
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private LayerMask groundLayerMask;
    private Animator anim;
    private float groundCheckRadius = 0.3f;
    private int jumpMax = 2; // 1 == single jump
    private int jumpsAvailable = 0; // remaining jumps available to player

    // animation variables
    private bool facingRight = true;

    // Start is called before the first frame update
    void Start()
    {
        // assign animator
        anim = gameObject.GetComponent<Animator>();

        // given a desired jumpHeight and jumpTime, calculate gravity (same formulas as 3D)
        float timeToApex = jumpTime / 2.0f;
        float gravity = (-2 * jumpHeight) / Mathf.Pow(timeToApex, 2);

        // calculate jump velocity (upward motion)
        initialJumpVelocity = Mathf.Sqrt(jumpHeight * -2 * gravity);
        rbody.gravityScale = gravity / Physics2D.gravity.y;
    }

    // Update is called once per frame
    void Update()
    {
        // read (and store) horizontal input
        horizInput = Input.GetAxis("Horizontal");
        anim.SetBool("isRunning", horizInput > 0.01 || horizInput < -0.01);

        // if we're facing the opposite direction of movement, flip the player sprite
        if ((!facingRight && horizInput > 0.01f) ||
            (facingRight && horizInput < -0.01f))
        {
            Flip();
        }

        // we're grounded if ground detection circle collides with ground layer and we're decending
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayerMask) && rbody.velocity.y < 0.01; // collision and heading downwards
        anim.SetBool("isGrounded", isGrounded);

        if (isGrounded)
        {
            jumpsAvailable = jumpMax;
        }
        if (Input.GetButtonDown("Jump") && jumpsAvailable > 0)
        {
            Jump();
        }
    }

    private void FixedUpdate()
    {
        // We're moving via Rigidbody physics (setting velocity directly) so we need to use FixedUpdate
        float xVel = horizInput * moveSpeed * Time.deltaTime;   // determine x velocity
        rbody.velocity = new Vector2(xVel, rbody.velocity.y);   // set new x velocity, maintain existing y velocity
    }

    private void Jump()
    {
        // maintain x velocity, change y to send player upwards
        rbody.velocity = new Vector2(rbody.velocity.x, initialJumpVelocity);
        jumpsAvailable--;
        anim.SetTrigger("jump");
    }

    private void OnDrawGizmos()
    {
        // draw sphere at transform's location
        Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
    }

    void Flip()
    {
        facingRight = !facingRight;
        // rotate 180 degrees on Y axis
        transform.Rotate(Vector3.up * 180);
    }
}
