using UnityEngine;

public class Player : MonoBehaviour {
    [Header("Movement Settings")]
    public float speed = 8.5f;
    public float jumpHeight = 13f;
    public int jumpCount = 2;
    private int remainingJumps;
    private float movement;
    private bool facingRight = true;
    
    [Header("Ground Check")]
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.2f;
    public LayerMask whatIsGround;
    [HideInInspector] public bool isGround;
    
    [Header("Health Settings")]
    public HealthPath HealthPath;
    public float Health;
    public float MaxHealth = 100f;
    
    [Header("Damage Settings")]
    public float invincibilityTime = 1f;
    private float lastDamageTime = -999f;
    private bool isDead = false;
    
    [Header("Attack Settings")]
    public Transform attackPoint;           // Điểm tấn công (tạo Empty Object con)
    public float attackRange = 1.5f;        // Tầm đánh
    public int attackDamage = 25;           // Sát thương
    public LayerMask enemyLayer;            // Layer của Enemy
    
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    
    void Start() {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        remainingJumps = jumpCount;
        Health = MaxHealth;
        
        if(HealthPath != null) {
            HealthPath.UpdateHealthPath(Health, MaxHealth);
        }
    }
    
    void Update() {
        if(isDead) return;
        
        movement = Input.GetAxisRaw("Horizontal");
        isGround = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, whatIsGround);
        
        if (isGround && rb.linearVelocityY <= 0.1f) {
            remainingJumps = jumpCount;
            animator.SetBool("Jump", false);
        }
        
        if (Input.GetKeyDown(KeyCode.Space)) {
            Jump();
        }
        
        if (Input.GetMouseButtonDown(0)) {
            PlayAttackAnimations();
        }
        
        animator.SetFloat("Run", Mathf.Abs(movement));
        Flip();
    }
    
    private void FixedUpdate() {
        if(isDead) return;
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
        int attackIndex = Random.Range(0, 3);
        if(attackIndex == 0) {
            animator.SetTrigger("Attack_1");
        }
        else if(attackIndex == 1) {
            animator.SetTrigger("Attack_2");
        }
        else if(attackIndex == 2) {
            animator.SetTrigger("Attack_3");
        }
    }
    
    // ====== HÀM NÀY ĐƯỢC GỌI TỪ ANIMATION EVENT ======
    public void Attack() {
        if(attackPoint == null) {
            Debug.LogWarning("⚠️ Chưa gán Attack Point cho Player!");
            return;
        }
        
        Debug.Log("🗡️ Player tấn công!");
        
        // Tìm tất cả Enemy trong tầm đánh
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
        
        Debug.Log($"Tìm thấy {hitEnemies.Length} enemy trong tầm đánh");
        
        // Gây sát thương cho từng Enemy
        foreach(Collider2D enemy in hitEnemies) {
            Debug.Log($"Đánh trúng: {enemy.name}");
            
            // Thử tìm script Enemy_4
            Enemy_4 enemy4Script = enemy.GetComponent<Enemy_4>();
            if(enemy4Script != null) {
                enemy4Script.TakeDamage(attackDamage);
                Debug.Log($"✅ Player gây {attackDamage} sát thương cho {enemy.name}!");
            }
            
            // Nếu có Enemy khác (Enemy_1, Enemy_2...) thì thêm vào đây
            // Enemy_1 enemy1Script = enemy.GetComponent<Enemy_1>();
            // if(enemy1Script != null) enemy1Script.TakeDamage(attackDamage);
        }
    }
    
    // ====== HỆ THỐNG NHẬN SÁT THƯƠNG ======
    public void TakeDamage(float damage) {
        if(Time.time < lastDamageTime + invincibilityTime) {
            return;
        }
        
        if(isDead) return;
        
        Health -= damage;
        Health = Mathf.Max(0, Health);
        
        if(HealthPath != null) {
            HealthPath.UpdateHealthPath(Health, MaxHealth);
        }
        
        if(animator != null && !isDead) {
            animator.SetTrigger("Hurt");
        }
        
        lastDamageTime = Time.time;
        StartCoroutine(FlashEffect());
        
        Debug.Log($"Player bị mất {damage} máu! Máu còn lại: {Health}");
        
        if(Health <= 0) {
            Die();
        }
    }
    
    private System.Collections.IEnumerator FlashEffect() {
        if(spriteRenderer == null) yield break;
        
        float flashDuration = 0.1f;
        int flashCount = Mathf.FloorToInt(invincibilityTime / (flashDuration * 2));
        
        for(int i = 0; i < flashCount; i++) {
            spriteRenderer.color = new Color(1f, 0.5f, 0.5f, 0.5f);
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(flashDuration);
        }
    }
    
    void Die() {
        isDead = true;
        Debug.Log("Player đã chết!");
        
        if(animator != null) {
            animator.SetTrigger("Dead");
        }
        
        rb.linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
    }
    
    public void Heal(float amount) {
        if(isDead) return;
        
        Health += amount;
        Health = Mathf.Min(Health, MaxHealth);
        
        if(HealthPath != null) {
            HealthPath.UpdateHealthPath(Health, MaxHealth);
        }
        
        Debug.Log($"Player hồi {amount} máu! Máu hiện tại: {Health}");
    }
    
    private void OnDrawGizmosSelected() {
        // Ground check
        if (groundCheckPoint != null) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
        
        // Attack range
        if (attackPoint != null) {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}