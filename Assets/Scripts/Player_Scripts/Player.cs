using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public float speed = 7f;
    public float jumpHeight = 7f;
    private float movement;
    private Rigidbody2D rb;
    private bool isGround;
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.2f;
    public LayerMask whatIsGround;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        movement = 0f;
        isGround = true;
        rb = this.gameObject.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
       movement = Input.GetAxis("Horizontal");
       if(Input.GetKeyDown(KeyCode.Space))
       {
           Jump();
       }

        Collider2D collInfo = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, whatIsGround);
        if(collInfo == true)
        {
            isGround = true;
        }
    }
    private void FixedUpdate()
    {
        transform.position += new Vector3(movement, 0f, 0f)* Time.fixedDeltaTime * speed;
    }
    void Jump()
    {
        if(isGround == true)
        {
             rb.linearVelocityY= jumpHeight;
             isGround = false;
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        Gizmos.color = Color.yellow;
        {
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        } 
    }
}
