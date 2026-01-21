using UnityEngine;

public class Enemy_4 : MonoBehaviour
{
    [Header("1. Cài đặt Di chuyển & Vùng hoạt động")]
    public float speed = 2f;
    public float patrolDistance = 3f;

    [Header("2. Cài đặt Phát hiện")]
    public float visionRange = 5f;
    
    [Header("3. Cài đặt Tấn công")]
    public Transform attackPoint;
    public float attackRange = 1f;    
    public int damage = 20;
    public float attackCooldown = 1.5f;
    public LayerMask playerLayer;

    [Header("4. Cài đặt Máu & An toàn")]
    public int maxHealth = 100;
    public Transform detectPoint;
    public float detectRange = 1f;
    public LayerMask groundLayer;

    // --- Biến nội bộ ---
    private Vector3 startPosition;
    private float minX, maxX;
    private bool movingRight = true;
    private float lastAttackTime;
    private int currentHealth;
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        currentHealth = maxHealth;
        startPosition = transform.position;

        // Tính toán giới hạn
        minX = startPosition.x - patrolDistance;
        maxX = startPosition.x + patrolDistance;
    }

    void Update()
    {
        if (currentHealth <= 0) return;

        // 1. Kiểm tra xem có thấy Player không
        Collider2D detectedPlayer = Physics2D.OverlapCircle(transform.position, visionRange, playerLayer);

        // 2. Kiểm tra Player có nằm trong vùng giới hạn không?
        bool isPlayerInZone = false;
        if (detectedPlayer != null)
        {
            float playerX = detectedPlayer.transform.position.x;
            if (playerX >= minX && playerX <= maxX)
            {
                isPlayerInZone = true;
            }
        }

        // 3. AI Logic
        if (isPlayerInZone)
        {
            // --- TRƯỜNG HỢP 1: TẤN CÔNG HOẶC ĐUỔI ---
            float distanceToPlayer = Vector2.Distance(transform.position, detectedPlayer.transform.position);

            if (distanceToPlayer <= attackRange)
            {
                PerformAttack(detectedPlayer);
            }
            else
            {
                Chase(detectedPlayer.transform);
            }
        }
        else
        {
            // --- TRƯỜNG HỢP 2: KHÔNG CÓ PLAYER ---
            // Kiểm tra xem Enemy có đang bị đi quá xa vùng tuần tra không?
            if (transform.position.x > maxX || transform.position.x < minX)
            {
                // Nếu đang ở ngoài vùng -> Chạy về vùng
                ReturnToPatrolArea();
            }
            else
            {
                // Nếu đang ở trong vùng -> Đi tuần bình thường
                BackToPatrol();
            }
        }
    }

    // --- HÀM MỚI: QUAY VỀ VÙNG TUẦN TRA ---
    void ReturnToPatrolArea()
    {
        if (anim != null) anim.SetBool("IsRun", true);

        // Mục tiêu là quay về điểm xuất phát (startPosition)
        // Nhưng chỉ cần quan tâm trục X
        Vector2 targetPos = new Vector2(startPosition.x, transform.position.y);
        
        // Di chuyển về nhà
        transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        // Quay mặt về phía nhà
        if (transform.position.x > startPosition.x && movingRight) 
        {
            // Đang ở bên phải nhà -> Phải quay mặt sang trái
            Flip(); 
        }
        else if (transform.position.x < startPosition.x && !movingRight) 
        {
            // Đang ở bên trái nhà -> Phải quay mặt sang phải
            Flip();
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

        // Chỉ Flip khi chạm biên
        if (transform.position.x >= maxX && movingRight)
        {
            Flip();
        }
        else if (transform.position.x <= minX && !movingRight)
        {
            Flip();
        }

        // Check vực thẳm
        if (detectPoint != null)
        {
            RaycastHit2D groundInfo = Physics2D.Raycast(detectPoint.position, Vector2.down, detectRange, groundLayer);
            if (!groundInfo.collider) Flip();
        }
    }

    void Chase(Transform target)
    {
        if (anim != null) anim.SetBool("IsRun", true);

        Vector2 targetPos = new Vector2(target.position.x, transform.position.y);
        transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        if (target.position.x > transform.position.x && !movingRight) Flip();
        else if (target.position.x < transform.position.x && movingRight) Flip();
    }

    void PerformAttack(Collider2D player)
    {
        if (anim != null) anim.SetBool("IsRun", false);

        if (player.transform.position.x > transform.position.x && !movingRight) Flip();
        else if (player.transform.position.x < transform.position.x && movingRight) Flip();

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
        }
    }

    void Attack()
    {
        if (anim != null) anim.SetTrigger("Attack");
        
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);
        foreach(Collider2D playerCollider in hitPlayers)
        {
            Player playerScript = playerCollider.GetComponent<Player>();
            if(playerScript != null)
            {
                playerScript.TakeDamage(damage);
            }
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
        Gizmos.color = Color.red;
        Vector3 startPos = Application.isPlaying ? startPosition : transform.position;
        Vector3 leftLimit = new Vector3(startPos.x - patrolDistance, startPos.y, startPos.z);
        Vector3 rightLimit = new Vector3(startPos.x + patrolDistance, startPos.y, startPos.z);
        
        Gizmos.DrawLine(leftLimit, rightLimit);
        Gizmos.DrawWireSphere(leftLimit, 0.2f);
        Gizmos.DrawWireSphere(rightLimit, 0.2f);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        if (attackPoint != null) {
            Gizmos.color = Color.yellow; 
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}