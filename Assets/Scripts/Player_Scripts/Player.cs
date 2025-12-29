using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 7f;
    public float jumpHeight = 10f; // Tăng lên 10 để nhảy rõ hơn
    private float movement;
    private bool facing_Right = true;

    [Header("Jump Settings")]
    public int jump_Count = 2; // Số lần nhảy cho phép (ví dụ: 2 là nhảy đôi)
    private int totall_Jumps;
    private bool isGround;

    [Header("Detection Settings")]
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.3f; // Tăng nhẹ bán kính để nhạy hơn
    public LayerMask whatIsGround;

    private Rigidbody2D rb;
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        totall_Jumps = jump_Count;
    }

    void Update()
    {
        // 1. Lấy đầu vào di chuyển
        movement = Input.GetAxisRaw("Horizontal");

        // 2. Xử lý Animation Run
        if (animator != null)
        {
            // Sử dụng Mathf.Abs để lấy giá trị tuyệt đối, đảm bảo Run luôn > 0 khi di chuyển
            animator.SetFloat("Run", Mathf.Abs(movement));
        }

        // 3. Kiểm tra mặt đất (Ground Check)
        Collider2D collInfo = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, whatIsGround);
        
        if (collInfo != null)
        {
            if (!isGround) // Khoảnh khắc vừa chạm đất
            {
                isGround = true;
                totall_Jumps = jump_Count; // Hồi lại số lần nhảy
                if (animator != null) animator.SetBool("Jump", false); // Tắt anim Jump
            }
        }
        else
        {
            isGround = false;
        }

        // 4. Xử lý lệnh Nhảy
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        Flip();
    }

    private void FixedUpdate()
    {
        // Di chuyển bằng Velocity để mượt mà hơn với vật lý
        rb.linearVelocity = new Vector2(movement * speed, rb.linearVelocity.y);
    }

    void Jump()
    {
        // Điều kiện: Đang trên đất HOẶC còn lượt nhảy phụ (totall_Jumps > 1)
        if (isGround || totall_Jumps > 1)
        {
            // Áp dụng lực nhảy
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpHeight);
            
            // Kích hoạt Animation Jump
            if (animator != null) animator.SetBool("Jump", true);

            // Trừ số lần nhảy
            if (isGround) {
                totall_Jumps = jump_Count - 1;
            } else {
                totall_Jumps--;
            }

            isGround = false;
        }
    }

    void Flip()
    {
        // Xoay nhân vật 180 độ khi đổi hướng
        if (movement < 0f && facing_Right || movement > 0f && !facing_Right)
        {
            facing_Right = !facing_Right;
            transform.Rotate(0f, 180f, 0f);
        }
    }

    // Vẽ vòng tròn kiểm tra trong Scene để dễ căn chỉnh
    private void OnDrawGizmos()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}