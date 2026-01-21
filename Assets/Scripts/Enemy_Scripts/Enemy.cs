using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int maxHealth = 3;
    public float deathTime = 2f;
    public float patrol_Speed = 2.2f;
    public float chaseSpeed = 4f;

    [Header("Detection (Mắt của Enemy)")]
    public Transform detectPoint;   // Điểm check vực thẳm
    public float distance = .3f;    // Khoảng cách check vực
    public LayerMask whatIsGround;

    // Vùng phát hiện người chơi (Hình chữ nhật)
    public Vector2 size;            // Cài đặt trong Inspector (VD: X=6, Y=3)
    public Vector3 offset;          // Cài đặt trong Inspector (VD: Y=1)
    private bool isPlayerDetected;  // Biến kiểm tra xem có thấy Player không

    [Header("Combat (Tấn công)")]
    // Khoảng cách dừng lại để đánh. Phải LỚN HƠN khoảng cách va chạm vật lý.
    // Ví dụ: Nếu Collider to 1 đơn vị, thì RetrieveDistance nên là 2 hoặc 2.5
    public float retrieveDistance = 2.5f; 
    
    public Transform attackPoint;   // Điểm gây sát thương (Mũi kiếm)
    public float attackRadius = 1f; // Bán kính vòng tròn sát thương
    public LayerMask whatIsPlayer;  // Layer của Player

    // Các biến nội bộ
    private Transform player;
    private Animator animator;
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider2D;
    private bool facingLeft;
    private bool isEnemyDied;

    void Start()
    {
        // Khởi tạo các giá trị ban đầu
        facingLeft = true;
        isEnemyDied = false;
        isPlayerDetected = false;
        
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        
        // Tìm Player trong Scene để tránh lỗi null
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        // 1. Kiểm tra nếu đã chết thì không làm gì cả
        if (maxHealth <= 0)
        {
            if (!isEnemyDied)
            {
                Die();
            }
            return;
        }

        // 2. Cập nhật vị trí Player (phòng trường hợp Player chết/respawn)
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        // 3. Kiểm tra xem có thấy Player không
        CheckPlayerDetection();

        // 4. Máy trạng thái (AI Logic)
        if (isPlayerDetected && player != null)
        {
            // --- TRẠNG THÁI: PHÁT HIỆN PLAYER ---
            
            // A. Quay mặt về phía Player
            FacePlayer();

            // B. Tính khoảng cách thực tế
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            if (distanceToPlayer > retrieveDistance)
            {
                // C. Nếu xa -> Đuổi theo (Chase)
                animator.SetBool("Attack", false);
                Vector2 targetPos = new Vector2(player.position.x, transform.position.y);
                transform.position = Vector2.MoveTowards(transform.position, targetPos, chaseSpeed * Time.deltaTime);
            }
            else
            {
                // D. Nếu gần -> Đứng lại và Tấn công (Attack)
                // Quan trọng: Set Attack = true để Animator chuyển state
                if (!animator.GetBool("Attack"))
                {
                    animator.SetBool("Attack", true);
                }
            }
        }
        else
        {
            // --- TRẠNG THÁI: KHÔNG THẤY PLAYER -> ĐI TUẦN ---
            animator.SetBool("Attack", false);
            PatrolEnemy();
            PatrolFlip(); // Chỉ tự động quay đầu khi đi tuần gặp vực
        }
    }

    // --- CÁC HÀM CHỨC NĂNG ---

    void PatrolEnemy()
    {
        // Di chuyển sang trái (do logic Flip sẽ xoay trục tọa độ nên luôn dùng Vector2.left)
        transform.Translate(Vector2.left * Time.deltaTime * patrol_Speed);
    }

    // Hàm quay đầu khi đi tuần (gặp vực hoặc tường)
    void PatrolFlip()
    {
        RaycastHit2D hitInfo = Physics2D.Raycast(detectPoint.position, Vector2.down, distance, whatIsGround);

        // Nếu không thấy đất -> Quay đầu
        if (hitInfo == false)
        {
            if (facingLeft)
            {
                transform.eulerAngles = new Vector3(0f, 180f, 0f);
                facingLeft = false;
            }
            else
            {
                transform.eulerAngles = new Vector3(0f, 0f, 0f);
                facingLeft = true;
            }
        }
    }

    // Hàm quay mặt về phía Player khi đang đuổi theo
    void FacePlayer()
    {
        if (player.position.x > transform.position.x && facingLeft)
        {
            transform.eulerAngles = new Vector3(0f, -180f, 0f); // Quay phải
            facingLeft = false;
        }
        else if (player.position.x < transform.position.x && !facingLeft)
        {
            transform.eulerAngles = new Vector3(0f, 0f, 0f); // Quay trái
            facingLeft = true;
        }
    }

    // Hàm kiểm tra vùng nhìn (OverlapBox)
    void CheckPlayerDetection()
    {
        // Tạo một hộp ảo để quét xem có Player đứng trong đó không
        Collider2D collInfo = Physics2D.OverlapBox(transform.position + offset, size, 0f, whatIsPlayer);
        isPlayerDetected = (collInfo != null);
    }

    // --- SỰ KIỆN TẤN CÔNG (Gắn vào Animation Event) ---
    public void Attack()
    {
        // Tạo vòng tròn tại mũi kiếm để check trúng đòn
        Collider2D collInfo = Physics2D.OverlapCircle(attackPoint.position, attackRadius, whatIsPlayer);

        if (collInfo != null)
        {
            Player playerScript = collInfo.gameObject.GetComponent<Player>();
            if (playerScript != null)
            {
                // Trừ máu Player
                playerScript.TakeDamage(1);
            }
        }
    }

    // Hàm nhận sát thương của chính Enemy
    public void TakeDamage(int damageAmount)
    {
        if (maxHealth <= 0) return;

        maxHealth -= damageAmount;
        animator.SetTrigger("Hurt");
        
        if (maxHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isEnemyDied) return;

        isEnemyDied = true;
        Debug.Log("Enemy Died!");
        
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        boxCollider2D.enabled = false; // Tắt va chạm để xác rơi xuống hoặc Player đi qua
        
        animator.SetBool("Death", true);
        Destroy(this.gameObject, deathTime);
    }

    // Vẽ hình hỗ trợ debug (Gizmos)
    private void OnDrawGizmosSelected()
    {
        // 1. Vẽ tia dò đất (Màu vàng)
        if (detectPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(detectPoint.position, Vector2.down * distance);
        }

        // 2. Vẽ tầm đánh (Màu đỏ)
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }

        // 3. Vẽ vùng phát hiện Player (Màu xanh lá)
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + offset, size);

        // 4. Vẽ giới hạn khoảng cách dừng lại (Màu xanh dương)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, retrieveDistance);
    }
}