using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    [Header("1. Movement Settings")]
    public float speed = 8.5f;
    public float jumpHeight = 13f;
    public int jumpCount = 2;
    private int remainingJumps;
    private float movement;
    private bool facingRight = true;

    [Header("2. Ground Check")]
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.2f;
    public LayerMask whatIsGround;
    [HideInInspector] public bool isGround;

    [Header("3. Health Settings")]
    public HealthPath HealthPath;
    public float maxHealth = 100f;
    public float currentHealth; // Đổi tên cho rõ ràng

    [Header("4. Combat Settings")]
    public float invincibilityTime = 1f;
    private float lastDamageTime = -999f;
    private bool isDead = false;
    private bool isInvincible = false; // Biến trạng thái bất tử

    [Header("5. Attack Settings")]
    public Transform attackPoint;
    public float attackRange = 1.5f;
    public int attackDamage = 25;
    public float attackRate = 2f; // Số lần đánh trong 1 giây (chống spam)
    private float nextAttackTime = 0f;
    public LayerMask enemyLayer;

    // Components
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        remainingJumps = jumpCount;
        currentHealth = maxHealth;

        if (HealthPath != null)
        {
            HealthPath.UpdateHealthPath(currentHealth, maxHealth);
        }
    }

    void Update()
    {
        if (isDead) return;

        // --- 1. DI CHUYỂN ---
        movement = Input.GetAxisRaw("Horizontal");

        // Kiểm tra chạm đất
        isGround = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, whatIsGround);

        // Reset nhảy khi chạm đất và đang rơi xuống
        if (isGround && rb.linearVelocity.y <= 0.1f)
        {
            remainingJumps = jumpCount;
            animator.SetBool("Jump", false);
        }

        // --- 2. NHẢY ---
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        // --- 3. TẤN CÔNG (Có giới hạn tốc độ đánh) ---
        if (Time.time >= nextAttackTime)
        {
            if (Input.GetMouseButtonDown(0))
            {
                PlayAttackAnimations();
                nextAttackTime = Time.time + 1f / attackRate; // Cài thời gian cho lần đánh tiếp theo
            }
        }

        // --- 4. ANIMATION & FLIP ---
        animator.SetFloat("Run", Mathf.Abs(movement));
        Flip();
    }

    private void FixedUpdate()
    {
        if (isDead) return;
        
        // Dùng rb.velocity thay cho rb.linearVelocity để tương thích tốt hơn
        rb.linearVelocity = new Vector2(movement * speed, rb.linearVelocity.y);
    }

    void Jump()
    {
        if (remainingJumps > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpHeight);
            animator.SetBool("Jump", true);
            remainingJumps--;
        }
    }

    void Flip()
    {
        if (movement < 0f && facingRight)
        {
            transform.eulerAngles = new Vector3(0, -180, 0);
            facingRight = false;
        }
        else if (movement > 0f && !facingRight)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
            facingRight = true;
        }
    }

    void PlayAttackAnimations()
    {
        // Random đòn đánh 1, 2 hoặc 3
        int attackIndex = Random.Range(1, 4); // Random từ 1 đến 3
        animator.SetTrigger("Attack_" + attackIndex);
    }

    // ====== HÀM NÀY ĐƯỢC GỌI TỪ ANIMATION EVENT ======
    public void Attack()
    {
        if (attackPoint == null) return;

        // Tìm tất cả Enemy trong tầm đánh
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            // --- ƯU TIÊN 1: Enemy thường (Script Enemy) ---
            Enemy enemyScript = enemy.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(1); // Gây 1 damage
                continue; // Xong con này, qua con khác
            }

            // --- ƯU TIÊN 2: Enemy loại 4 (Script Enemy_4) ---
            Enemy_4 enemy4Script = enemy.GetComponent<Enemy_4>();
            if (enemy4Script != null)
            {
                enemy4Script.TakeDamage(attackDamage); // Gây 25 damage
                continue;
            }
        }
    }

    // ====== HỆ THỐNG NHẬN SÁT THƯƠNG ======
    public void TakeDamage(float damage)
    {
        // Nếu đang bất tử hoặc đã chết thì thôi
        if (isInvincible || isDead) return;

        // Trừ máu
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth); // Không để máu âm

        // Cập nhật thanh máu UI
        if (HealthPath != null)
        {
            HealthPath.UpdateHealthPath(currentHealth, maxHealth);
        }

        // Animation bị đau
        if (animator != null)
        {
            animator.SetTrigger("Hurt");
        }

        Debug.Log($"Player bị mất {damage} máu! Còn lại: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Kích hoạt bất tử tạm thời
            StartCoroutine(BecomeInvincible());
        }
    }

    // Coroutine Bất Tử & Nhấp Nháy
    private IEnumerator BecomeInvincible()
    {
        isInvincible = true;
        
        // Vòng lặp nhấp nháy
        float flashDuration = 0.1f;
        int flashCount = Mathf.FloorToInt(invincibilityTime / (flashDuration * 2));

        for (int i = 0; i < flashCount; i++)
        {
            // Mờ đi (Màu đỏ nhạt)
            if (spriteRenderer) spriteRenderer.color = new Color(1f, 0.5f, 0.5f, 0.5f);
            yield return new WaitForSeconds(flashDuration);

            // Hiện lại (Màu trắng gốc)
            if (spriteRenderer) spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(flashDuration);
        }

        isInvincible = false;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (animator != null)
        {
            animator.SetBool("Death", true); // Hoặc SetTrigger("Dead") tùy Animator của bạn
        }

        // Dừng vật lý
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static; // Biến thành tượng để không bị đẩy
        
        // Tắt va chạm
        Collider2D col = GetComponent<Collider2D>();
        if(col != null) col.enabled = false;

        this.enabled = false; // Tắt script điều khiển
        Debug.Log("💀 Player đã chết!");
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth); // Không hồi quá cây máu

        if (HealthPath != null)
        {
            HealthPath.UpdateHealthPath(currentHealth, maxHealth);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }

        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}