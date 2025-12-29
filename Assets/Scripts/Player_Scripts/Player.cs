using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement & Jump")]
    public float speed = 7f;
    public float jumpHeight = 7f;
    private float movement;
    private Rigidbody2D rb;
    public Animator animator;

    [Header("Detection")]
    private bool isGround;
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.2f;
    public LayerMask whatIsGround;
    public int jump_Count = 2;
    private int totall_Jumps;
    private bool facing_Right = true;

    [Header("Attack Settings")]
    public Transform attackPoint;      // Điểm tâm của đòn đánh
    public float attackRadius = 0.5f; // Khoảng cách đánh
    public LayerMask enemyLayers;     // Layer của kẻ địch

    void Start()
    {
        totall_Jumps = jump_Count;
        rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    void Update()
    {
        movement = Input.GetAxis("Horizontal");

        // Animation Chạy
        if (animator != null) {
            animator.SetFloat("Run", Mathf.Abs(movement));
        }

        // Nhảy
        if (Input.GetKeyDown(KeyCode.Space)) {
            Jump();
        }

        // Tấn công khi nhấn Chuột trái (0)
        if (Input.GetMouseButtonDown(0)) {
            AttackAnimation();
        }

        // Kiểm tra mặt đất
        Collider2D collInfo = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, whatIsGround);
        isGround = (collInfo != null);

        Flip();
    }

    // 1. Kích hoạt Animation Tấn công
    void AttackAnimation() {
        if (animator != null) {
            // Sử dụng Trigger để bắt đầu animation tấn công ngay lập tức
            animator.SetTrigger("Attack_1"); 
        }
    }

    // 2. Hàm này sẽ được gọi bởi Animation Event (Sự kiện trong clip Animation)
    // Để thực sự gây sát thương lên kẻ địch
    public void PerformAttackDamage() {
        // Tìm tất cả kẻ địch trong vùng đánh
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayers);

        foreach (Collider2D enemy in hitEnemies) {
            Debug.Log("Đã đánh trúng: " + enemy.name);
            // Gọi hàm nhận sát thương của kẻ địch ở đây
            // enemy.GetComponent<Enemy>().TakeDamage(1);
        }
    }

    void Jump() {
        if (totall_Jumps > 0) {
            if (animator != null) animator.SetBool("Jump", true);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpHeight);
            totall_Jumps--;
            isGround = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Ground")) {
            totall_Jumps = jump_Count;
            if (animator != null) animator.SetBool("Jump", false);
        }
    }

    private void FixedUpdate() {
        rb.linearVelocity = new Vector2(movement * speed, rb.linearVelocity.y);
    }

    void Flip() {
        if (movement < 0f && facing_Right || movement > 0f && !facing_Right) {
            facing_Right = !facing_Right;
            transform.Rotate(0, 180, 0);
        }
    }

    private void OnDrawGizmosSelected() {
        if (groundCheckPoint != null) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
        if (attackPoint != null) {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}