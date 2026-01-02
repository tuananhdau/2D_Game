using UnityEngine;

public class Enemy_4 : MonoBehaviour
{
    [Header("1. Cài đặt Di chuyển & Đi tuần")]
    public float speed = 2f;
    public float patrolDistance = 3f; // Quãng đường đi tuần quanh điểm gốc

    [Header("2. Cài đặt Phát hiện & Đuổi bắt")]
    public float visionRange = 5f;       // Tầm nhìn (Vòng tròn xanh lá)
    public float maxChaseDistance = 10f; // Nếu đi quá xa nhà 10m thì tự bỏ về (dù vẫn thấy Player)
    
    [Header("3. Cài đặt Tấn công")]
    public Transform attackPoint;
    public float attackRange = 1.5f;     // Tầm đánh (Vòng tròn vàng)
    public int damage = 20;
    public float attackCooldown = 1.5f;
    public LayerMask playerLayer;

    [Header("4. Cài đặt Máu & An toàn")]
    public int maxHealth = 100;
    public Transform detectPoint;
    public float detectRange = 1f;

    // --- Biến nội bộ ---
    private Vector3 startPosition;
    private bool movingRight = true;
    private float lastAttackTime;
    private int currentHealth;
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        currentHealth = maxHealth;
        startPosition = transform.position; // Lưu vị trí "nhà"
    }

    void Update()
    {
        if (currentHealth <= 0) return;

        // 1. Kiểm tra xem có thấy Player không (Trong vùng VisionRange)
        Collider2D detectedPlayer = Physics2D.OverlapCircle(transform.position, visionRange, playerLayer);

        // 2. Tính khoảng cách từ Enemy về "Nhà" (Start Position)
        float distanceFromHome = Vector2.Distance(transform.position, startPosition);

        // --- QUYẾT ĐỊNH HÀNH ĐỘNG ---
        // Đuổi theo KHI: (Thấy Player) VÀ (Chưa bị dụ đi quá xa nhà)
        if (detectedPlayer != null && distanceFromHome < maxChaseDistance)
        {
            // -> ĐANG Ở TRẠNG THÁI CHIẾN ĐẤU (Attack / Chase)
            float distanceToPlayer = Vector2.Distance(transform.position, detectedPlayer.transform.position);

            if (distanceToPlayer <= attackRange)
            {
                // A. Đủ gần thì ĐÁNH
                if (anim != null) anim.SetBool("IsRun", false); // Dừng chạy
                
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    Attack();
                }
            }
            else
            {
                // B. Chưa đủ gần thì ĐUỔI
                // (Nếu đang có lệnh đánh dở thì hủy đi để chạy cho nhanh)
                if (anim != null) anim.ResetTrigger("Attack"); 
                
                Chase(detectedPlayer.transform);
            }
        }
        else
        {
            // -> KHÔNG THẤY PLAYER (Hoặc Player đã chạy quá xa) -> VỀ ĐI TUẦN
            BackToPatrol();
        }
    }

    // --- HÀM QUAY VỀ ĐI TUẦN ---
    void BackToPatrol()
    {
        // Hủy ngay lệnh tấn công nếu đang chờ kích hoạt (Quan trọng)
        if (anim != null) anim.ResetTrigger("Attack");

        // Gọi hàm đi tuần
        Patrol();
    }

    void Patrol()
    {
        // Kích hoạt lại animation đi bộ
        if (anim != null) anim.SetBool("IsRun", true);

        // Di chuyển thẳng
        transform.Translate(Vector2.right * speed * Time.deltaTime);

        // Logic quay đầu khi đi hết quãng đường tuần tra
        float distanceFromStart = Vector2.Distance(transform.position, startPosition);
        
        // Nếu đang ở xa điểm xuất phát (do vừa đuổi theo Player về)
        if (distanceFromStart >= patrolDistance)
        {
            // Kiểm tra xem có đang đi hướng ra xa không? Nếu có thì quay đầu lại
            bool isMovingAway = (transform.position.x > startPosition.x && movingRight) || 
                                (transform.position.x < startPosition.x && !movingRight);
            if (isMovingAway) Flip();
        }

        // Logic gặp vực thẳm
        if (detectPoint != null)
        {
            RaycastHit2D groundInfo = Physics2D.Raycast(detectPoint.position, Vector2.down, detectRange);
            if (!groundInfo.collider) Flip();
        }
    }

    void Chase(Transform target)
    {
        if (anim != null) anim.SetBool("IsRun", true);

        // Di chuyển tới vị trí Player
        transform.position = Vector2.MoveTowards(transform.position, 
            new Vector2(target.position.x, transform.position.y), speed * Time.deltaTime);

        // Quay mặt về phía Player
        if (target.position.x > transform.position.x && !movingRight) Flip();
        else if (target.position.x < transform.position.x && movingRight) Flip();
    }

    void Attack()
    {
        if (anim != null) anim.SetTrigger("Attack");
        
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);
        foreach(Collider2D player in hitPlayers)
        {
            // Code trừ máu Player (nếu có)
             Debug.Log("Chém trúng!");
        }
        lastAttackTime = Time.time;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if(anim != null) anim.SetTrigger("Hurt");
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        if(anim != null) anim.SetTrigger("Dead");
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
        Destroy(gameObject, 2f);
    }

    void Flip()
    {
        movingRight = !movingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    void OnDrawGizmosSelected()
    {
        // 1. Tầm nhìn (Xanh lá) - Player ra khỏi vòng này là quái bỏ đi
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        // 2. Tầm đánh (Vàng)
        if (attackPoint != null) {
            Gizmos.color = Color.yellow; 
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
        
        // 3. Giới hạn đuổi (Đỏ mờ)
        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Vector3 home = Application.isPlaying ? startPosition : transform.position;
        Gizmos.DrawWireSphere(home, maxChaseDistance);
    }
}