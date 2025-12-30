using UnityEngine;

public class Player : MonoBehaviour {

    [Header("Movement Settings")]
    public float speed = 7f;
    public float jumpHeight = 8f;
    public int jumpCount = 2; // Số lần nhảy (ví dụ: 2 là double jump)
    private int remainingJumps;
    private float movement;
    private bool facingRight = true;

    [Header("Ground Check")]
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.2f;
    public LayerMask whatIsGround;
    [HideInInspector] public bool isGround;

    private Rigidbody2D rb;
    private Animator animator;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        remainingJumps = jumpCount;
    }

    void Update() {
        // Lấy input di chuyển (A/D hoặc Mũi tên)
        movement = Input.GetAxisRaw("Horizontal");

        // Kiểm tra nhân vật có đang đứng trên mặt đất không
        isGround = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, whatIsGround);

        // Reset số lần nhảy khi chạm đất
        if (isGround && rb.linearVelocityY <= 0.1f) {
            remainingJumps = jumpCount;
            animator.SetBool("Jump", false);
        }

        // Xử lý Nhảy
        if (Input.GetKeyDown(KeyCode.Space)) {
            Jump();
        }

        // Xử lý Animation Tấn công
        if (Input.GetMouseButtonDown(0)) {
            PlayAttackAnimations();
        }

        // Cập nhật Animation Chạy (Dựa trên tốc độ di chuyển)
        animator.SetFloat("Run", Mathf.Abs(movement));

        Flip();
    }

    private void FixedUpdate() {
        // Áp dụng di chuyển vật lý
        rb.linearVelocity = new Vector2(movement * speed, rb.linearVelocity.y);
    }

    void Jump() {
        if (remainingJumps > 0) {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpHeight);
            animator.SetBool("Jump", true);
            remainingJumps--;
        }
    }

    void Flip() {
        // Xoay hướng nhân vật
        if (movement < 0f && facingRight) {
            transform.eulerAngles = new Vector3(0, -180, 0);
            facingRight = false;
        }
        else if (movement > 0f && !facingRight) {
            transform.eulerAngles = new Vector3(0, 0, 0);
            facingRight = true;
        }
    }

    void PlayAttackAnimations() {
        // Random ngẫu nhiên 1 trong 3 animation tấn công
        int attackIndex = Random.Range(0, 3);
        if(attackIndex == 0) {
            animator.SetTrigger("Attack");
        }
        else if(attackIndex == 1) {
            animator.SetTrigger("Attack_2");
        }
        else if(attackIndex == 2) {
            animator.SetTrigger("Attack_3");
        }
    }

    // Vẽ vòng tròn kiểm tra mặt đất trong cửa sổ Scene
    private void OnDrawGizmosSelected() {
        if (groundCheckPoint != null) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}