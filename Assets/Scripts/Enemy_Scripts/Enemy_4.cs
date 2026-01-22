using UnityEngine;
using UnityEngine.UI;

public class Enemy_4 : MonoBehaviour
{
    /* =========================
     * 1. Di chuyển & Tuần tra
     * ========================= */
    [Header("1. Cài đặt Di chuyển & Vùng hoạt động")]
    public float speed = 2f;
    public float patrolDistance = 3f;

    /* =========================
     * 2. Phát hiện Player
     * ========================= */
    [Header("2. Cài đặt Phát hiện")]
    public float visionRange = 5f;
    public Transform detectPoint;
    public float detectRange = 1f;
    public LayerMask groundLayer;

    /* =========================
     * 3. Tấn công
     * ========================= */
    [Header("3. Cài đặt Tấn công")]
    public Transform attackPoint;
    public float attackRange = 1f;
    public int damage = 20;
    public float attackCooldown = 1.5f;
    public LayerMask playerLayer;

    /* =========================
     * 4. Máu
     * ========================= */
    [Header("4. Cài đặt Máu")]
    public int maxHealth = 100;

    /* =========================
     * 5. UI Thanh máu
     * ========================= */
    [Header("5. UI Thanh máu")]
    public GameObject healthBarRoot;   // HealthPath-Enemy4
    public Image healthFill;           // Image đỏ (Fill)
    public float healthBarHideDelay = 2f;

    /* =========================
     * 6. Bất tử tạm thời
     * ========================= */
    [Header("6. Bất tử khi nhận damage")]
    public float invincibleTime = 0.15f;

    /* =========================
     * Biến nội bộ
     * ========================= */
    private int currentHealth;
    private Vector3 startPosition;
    private float minX, maxX;
    private bool movingRight = true;
    private float lastAttackTime;
    private Animator anim;

    private bool isInvincible = false;
    private float lastHitTime;

    /* =========================
     * START
     * ========================= */
    void Start()
    {
        anim = GetComponent<Animator>();
        currentHealth = maxHealth;

        startPosition = transform.position;
        minX = startPosition.x - patrolDistance;
        maxX = startPosition.x + patrolDistance;

        if (healthBarRoot != null)
            healthBarRoot.SetActive(false);

        UpdateHealthBar();
    }

    /* =========================
     * UPDATE
     * ========================= */
    void Update()
    {
        if (currentHealth <= 0) return;

        // Ẩn thanh máu sau 1 thời gian không bị đánh
        if (healthBarRoot != null && healthBarRoot.activeSelf)
        {
            if (Time.time > lastHitTime + healthBarHideDelay)
                healthBarRoot.SetActive(false);
        }

        Collider2D detectedPlayer =
            Physics2D.OverlapCircle(transform.position, visionRange, playerLayer);

        bool isPlayerInZone = false;

        if (detectedPlayer != null)
        {
            float px = detectedPlayer.transform.position.x;
            if (px >= minX && px <= maxX)
                isPlayerInZone = true;
        }

        if (isPlayerInZone && detectedPlayer != null)
        {
            float distance =
                Vector2.Distance(transform.position, detectedPlayer.transform.position);

            if (distance <= attackRange)
                PerformAttack(detectedPlayer);
            else
                Chase(detectedPlayer.transform);
        }
        else
        {
            if (transform.position.x > maxX || transform.position.x < minX)
                ReturnToPatrolArea();
            else
                Patrol();
        }
    }

    /* =========================
     * TUẦN TRA
     * ========================= */
    void Patrol()
    {
        if (anim != null) anim.SetBool("IsRun", true);

        transform.Translate(Vector2.right * speed * Time.deltaTime);

        if (transform.position.x >= maxX && movingRight) Flip();
        else if (transform.position.x <= minX && !movingRight) Flip();

        if (detectPoint != null)
        {
            RaycastHit2D groundInfo =
                Physics2D.Raycast(detectPoint.position, Vector2.down, detectRange, groundLayer);

            if (!groundInfo.collider) Flip();
        }
    }

    void ReturnToPatrolArea()
    {
        if (anim != null) anim.SetBool("IsRun", true);

        Vector2 target = new Vector2(startPosition.x, transform.position.y);
        transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (transform.position.x > startPosition.x && movingRight) Flip();
        else if (transform.position.x < startPosition.x && !movingRight) Flip();
    }

    /* =========================
     * ĐUỔI THEO PLAYER
     * ========================= */
    void Chase(Transform target)
    {
        if (anim != null) anim.SetBool("IsRun", true);

        Vector2 targetPos = new Vector2(target.position.x, transform.position.y);
        transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        if (target.position.x > transform.position.x && !movingRight) Flip();
        else if (target.position.x < transform.position.x && movingRight) Flip();
    }

    /* =========================
     * TẤN CÔNG
     * ========================= */
    void PerformAttack(Collider2D player)
    {
        if (anim != null) anim.SetBool("IsRun", false);

        if (player.transform.position.x > transform.position.x && !movingRight) Flip();
        else if (player.transform.position.x < transform.position.x && movingRight) Flip();

        if (Time.time >= lastAttackTime + attackCooldown)
            Attack();
    }

    void Attack()
    {
        if (anim != null) anim.SetTrigger("Attack");

        Collider2D[] hits =
            Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);

        foreach (Collider2D col in hits)
        {
            Player p = col.GetComponent<Player>();
            if (p != null)
                p.TakeDamage(damage);
        }

        lastAttackTime = Time.time;
    }

    /* =========================
     * NHẬN DAMAGE
     * ========================= */
    public void TakeDamage(int dmg)
    {
        if (isInvincible) return;

        currentHealth -= dmg;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        lastHitTime = Time.time;

        if (healthBarRoot != null)
            healthBarRoot.SetActive(true);

        UpdateHealthBar();

        if (anim != null) anim.SetTrigger("Hurt");

        StartCoroutine(InvincibleCoroutine());

        if (currentHealth <= 0)
            Die();
    }

    /* =========================
     * BẤT TỬ TẠM THỜI
     * ========================= */
    System.Collections.IEnumerator InvincibleCoroutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibleTime);
        isInvincible = false;
    }

    /* =========================
     * CHẾT
     * ========================= */
    void Die()
    {
        if (anim != null) anim.SetTrigger("Dead");

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        this.enabled = false;
        Destroy(gameObject, 2f);
    }

    /* =========================
     * UI & HỖ TRỢ
     * ========================= */
    void UpdateHealthBar()
    {
        if (healthFill != null)
            healthFill.fillAmount = (float)currentHealth / maxHealth;
    }

    void LateUpdate()
    {
        if (healthBarRoot != null)
            healthBarRoot.transform.rotation = Quaternion.identity;
    }

    void Flip()
    {
        movingRight = !movingRight;
        transform.Rotate(0f, 180f, 0f);
    }
}
