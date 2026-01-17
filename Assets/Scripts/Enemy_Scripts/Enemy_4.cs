using UnityEngine;

public class Enemy_4 : MonoBehaviour
{
    [Header("1. Cài đặt Di chuyển & Đi tuần")]
    public float speed = 2f;
    public float patrolDistance = 3f;

    [Header("2. Cài đặt Phát hiện & Đuổi bắt")]
    public float visionRange = 5f;
    public float maxChaseDistance = 10f;
    
    [Header("3. Cài đặt Tấn công")]
    public Transform attackPoint;
    public float attackRange = 2.5f;     // Tăng tầm đánh lên 2.5 (vòng tròn vàng)
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
        startPosition = transform.position;
    }

    void Update()
    {
        if (currentHealth <= 0) return;

        // Kiểm tra thấy Player
        Collider2D detectedPlayer = Physics2D.OverlapCircle(transform.position, visionRange, playerLayer);

        // Tính khoảng cách từ Enemy về nhà
        float distanceFromHome = Vector2.Distance(transform.position, startPosition);

        // Đuổi theo khi: (Thấy Player) VÀ (Chưa đi quá xa nhà)
        if (detectedPlayer != null && distanceFromHome < maxChaseDistance)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, detectedPlayer.transform.position);

            if (distanceToPlayer <= attackRange)
            {
                // Đủ gần thì ĐÁNH
                if (anim != null) anim.SetBool("IsRun", false);
                
                // Đảm bảo quay mặt về phía Player trước khi tấn công
                if (detectedPlayer.transform.position.x > transform.position.x && !movingRight)
                {
                    Flip();
                }
                else if (detectedPlayer.transform.position.x < transform.position.x && movingRight)
                {
                    Flip();
                }
                
                Debug.Log($"Player trong tầm đánh! Khoảng cách: {distanceToPlayer}");
                
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    Attack();
                }
            }
            else
            {
                // Chưa đủ gần thì ĐUỔI
                if (anim != null) anim.ResetTrigger("Attack"); 
                
                Chase(detectedPlayer.transform);
            }
        }
        else
        {
            // Không thấy Player -> VỀ ĐI TUẦN
            BackToPatrol();
        }
    }

    void BackToPatrol()
    {
        if (anim != null) anim.ResetTrigger("Attack");
        Patrol();
    }

    void Patrol()
    {
        if (anim != null) anim.SetBool("IsRun", true);

        transform.Translate(Vector2.right * speed * Time.deltaTime);

        float distanceFromStart = Vector2.Distance(transform.position, startPosition);
        
        if (distanceFromStart >= patrolDistance)
        {
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

        transform.position = Vector2.MoveTowards(transform.position, 
            new Vector2(target.position.x, transform.position.y), speed * Time.deltaTime);

        // Quay mặt về phía Player
        if (target.position.x > transform.position.x && !movingRight) Flip();
        else if (target.position.x < transform.position.x && movingRight) Flip();
    }

    void Attack()
    {
        // Sau khi đã quay mặt đúng hướng, thực hiện tấn công
        if (anim != null) anim.SetTrigger("Attack");
        
        Debug.Log("=== BẮT ĐẦU ATTACK ===");
        
        // Tìm tất cả Player trong tầm đánh
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);
        
        Debug.Log($"Số Player tìm thấy: {hitPlayers.Length}");
        
        foreach(Collider2D playerCollider in hitPlayers)
        {
            Debug.Log($"Tìm thấy: {playerCollider.gameObject.name}, Layer: {LayerMask.LayerToName(playerCollider.gameObject.layer)}");
            
            // Lấy component Player từ đối tượng bị đánh trúng
            Player playerScript = playerCollider.GetComponent<Player>();
            
            if(playerScript != null)
            {
                // Gây sát thương cho Player
                playerScript.TakeDamage(damage);
                Debug.Log($"Enemy tấn công Player gây {damage} sát thương!");
            }
            else
            {
                Debug.LogWarning("Không tìm thấy Player Script trên GameObject!");
            }
        }
        
        lastAttackTime = Time.time;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if(anim != null) anim.SetTrigger("Hurt");
        
        Debug.Log($"Enemy bị mất {damage} máu! Máu còn lại: {currentHealth}");
        
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
        // Tầm nhìn (Xanh lá)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        // Tầm đánh (Vàng)
        if (attackPoint != null) {
            Gizmos.color = Color.yellow; 
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
        
        // Giới hạn đuổi (Đỏ mờ)
        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Vector3 home = Application.isPlaying ? startPosition : transform.position;
        Gizmos.DrawWireSphere(home, maxChaseDistance);
    }
}