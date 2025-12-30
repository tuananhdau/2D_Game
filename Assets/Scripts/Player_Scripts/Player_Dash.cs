using UnityEngine;
using System.Collections;
using UnityEditor.Experimental.GraphView;

public class Player_Dash : MonoBehaviour
{
    public float dash_Force = 10f;
    public float dash_Duration = .35f;
    private bool facingRight;
    public Rigidbody2D rb;
    private bool isDashing;
    private float direction;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isDashing = false;
        facingRight = true;
    }

    // Update is called once per frame
    void Update()
    {
        float move = Input.GetAxis("Horizontal");
        if (move > 0f)
        {
            facingRight = true;
        }
            
        else if (move < 0f){
            facingRight = false;
        }
        if(Input.GetMouseButtonDown(1) && isDashing== false)
        {
            StartCoroutine(Dash());
        }
    }
    IEnumerator Dash()
    {
        isDashing = true;
        
        if (facingRight == true)
        {
            direction = 1f;
        }    
        else if (facingRight == false)
        {
            direction = -1f;
        }  
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(dash_Force * direction, 0f);
        yield return new WaitForSeconds(dash_Duration);
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 2f;
        isDashing = false;
    }
}
