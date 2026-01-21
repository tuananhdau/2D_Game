using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int maxHealth = 3;
    public float deathTime = 2f;
    public float patrol_Speed = 2.2f;
    public float chaseSpeed = 4f;

    [Header("Detection & Ranges")]
    public Transform detectPoint;
    public float distance = .3f; // Khoảng cách check đất để quay đầu
    public LayerMask whatIsGround;
    
    [Tooltip("Khoảng cách để Enemy dừng lại và đánh. Nếu Enemy cứ chạy mãi, hãy TĂNG số này lên (ví dụ: 2.0)")]
    public float retrieveDistance = 1.5f; 

    // Biến này thực chất là "Vùng phát hiện người chơi" (Aggro Range)
    // Được set bởi OverlapBox (hình chữ nhật màu vàng)
    private bool isPlayerInAttackRange; 
    public Vector2 size; // Kích thước vùng phát hiện
    public Vector3 offset;
    
    [Header("Combat")]
    public Transform attackPoint;
    public float attackRadius = 1f;
    public LayerMask whatIsPlayer;

    private Transform player;
    private Animator animator;
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider2D;
    private bool facingLeft;
    private bool isEnemyDied;

    void Start()
    {
        facingLeft = true;
        isEnemyDied = false;
        isPlayerInAttackRange = false;
        rb = this.gameObject.GetComponent<Rigidbody2D>();
        animator = this.gameObject.GetComponent<Animator>();
        boxCollider2D = this.gameObject.GetComponent<BoxCollider2D>();
        
        // Tìm player ngay lúc đầu (để tránh lỗi null nếu có)
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        // 1. Kiểm tra chết
        if (maxHealth <= 0)
        {
            if (!isEnemyDied)
            {
                rb.gravityScale = 0f;
                rb.linearVelocity = Vector2.zero; // Dừng hẳn physics lại
                isEnemyDied = true;
                Die();
            }
            return;
        }

        // 2. Nếu không tìm thấy Player trong Scene -> Đi tuần tra
        if (player == null)
        {
             // Thử tìm lại player nếu bị mất
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) 
            {
                player = p.transform;
            }
            else
            {
                PatrolEnemy();
                animator.SetBool("Attack", false);
                CheckCollision(); // Vẫn check để vẽ gizmos
                Flip();
                return;
            }
        }

        // 3. Các logic chính
        Flip();
        CheckCollision(); // Cập nhật biến isPlayerInAttackRange (Vùng phát hiện)

        // Nếu Player nằm trong vùng hình hộp chữ nhật (OverlapBox)
        if (isPlayerInAttackRange)
        {
            // --- XỬ LÝ QUAY MẶT ---
            if (player.position.x > transform.position.x && facingLeft)
            {
                transform.eulerAngles = new Vector3(0f, -180f, 0f);
                facingLeft = false;
            }
            else if (player.position.x < transform.position.x && !facingLeft)
            {
                transform.eulerAngles = new Vector3(0f, 0f, 0f);
                facingLeft = true;
            }

            // --- XỬ LÝ KHOẢNG CÁCH (CHASE vs ATTACK) ---
            
            // Tính khoảng cách thực tế giữa Enemy và Player
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            // Nếu khoảng cách LỚN hơn tầm đánh -> CHẠY TỚI
            if (distanceToPlayer > retrieveDistance)
            {
                animator.SetBool("Attack", false);
                Vector2 targetPos = new Vector2(player.position.x, transform.position.y);
                transform.position = Vector2.MoveTowards(transform.position, targetPos, chaseSpeed * Time.deltaTime);
            }
            // Nếu khoảng cách NHỎ hơn hoặc bằng tầm đánh -> TẤN CÔNG
            else
            {
                // Dừng di chuyển để đánh (tránh vừa trượt vừa đánh)
                // Không dùng MoveTowards ở đây
                
                if (!animator.GetBool("Attack"))
                {
                    Debug.Log("Enemy in range -> ATTACK!"); // Kiểm tra Console xem dòng này có hiện không
                    animator.SetBool("Attack", true);
                }
            }
        }
        else
        {
            // Player không nằm trong vùng phát hiện -> Đi tuần
            PatrolEnemy();
            animator.SetBool("Attack", false);
        }
    }

    void PatrolEnemy()
    {
        transform.Translate(Vector2.left * Time.deltaTime * patrol_Speed);
    }

    // Hàm này check vùng phát hiện (Aggro Range)
    void CheckCollision()
    {
        Collider2D collInfo = Physics2D.OverlapBox(transform.position + offset, size, 0f, whatIsPlayer);
        isPlayerInAttackRange = (collInfo != null);
    }

    // Hàm này được gọi bởi Animation Event (khi chém xuống)
    public void Attack()
    {
        Collider2D collInfo = Physics2D.OverlapCircle(attackPoint.position, attackRadius, whatIsPlayer);

        if (collInfo)
        {
            // Kiểm tra null kỹ hơn để tránh lỗi
            var playerScript = collInfo.gameObject.GetComponent<Player>(); // Đảm bảo script Player của bạn tên là "Player"
            if (playerScript != null)
            {
                 // playerScript.TakeDamage(1); 
                 Debug.Log("Hit Player!");
            }
        }
    }

    void Flip()
    {
        // Chỉ Flip khi đi tuần (Patrol) hoặc khi không trong trạng thái Chase
        // (Logic Chase đã có phần Flip riêng ở trên để bám theo Player chính xác hơn)
        if(isPlayerInAttackRange) return; 

        RaycastHit2D hitInfo = Physics2D.Raycast(detectPoint.position, Vector2.down, distance, whatIsGround);

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

    public void TakeDamage(int damageAmount)
    {
        if (maxHealth <= 0) return;
        
        maxHealth -= damageAmount;
        animator.SetTrigger("Hurt");
    }

    private void OnDrawGizmosSelected()
    {
        if (detectPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(detectPoint.position, Vector2.down * distance);
        }

        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }

        // Vẽ vùng phát hiện
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + offset, size);
        
        // Vẽ vùng giới hạn đánh (retrieveDistance) để bạn dễ chỉnh
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, retrieveDistance);
    }

    void Die()
    {
        Debug.Log("Enemy Died!");
        animator.SetBool("Death", true);
        boxCollider2D.enabled = false;
        Destroy(this.gameObject, deathTime);
    }
}