using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Enemy : MonoBehaviour
{
    /* =========================
     * STATS
     * ========================= */
    [Header("Enemy Stats")]
    public int maxHealth = 3;
    public float deathTime = 2f;
    public float patrol_Speed = 2.2f;
    public float chaseSpeed = 4f;

    private int currentHealth;
    private bool isEnemyDied;

    /* =========================
     * INVINCIBLE
     * ========================= */
    [Header("Invincible")]
    public float invincibleTime = 0.15f;
    private bool isInvincible;

    /* =========================
     * VISUAL EFFECTS
     * ========================= */
    [Header("Visual Effects")]
    public GameObject damageTextPrefab; // Prefab để hiển thị số damage
    public Transform damageTextSpawnPoint; // Vị trí spawn damage text (trên đầu enemy)

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    /* =========================
     * HEALTH BAR UI
     * ========================= */
    [Header("Health Bar UI")]
    public GameObject healthBarRoot;
    public Image healthFill;
    public float hideHealthBarDelay = 2f;
    private Coroutine hideHealthCoroutine;

    /* =========================
     * DETECTION
     * ========================= */
    [Header("Detection")]
    public Transform detectPoint;
    public float distance = .3f;
    public LayerMask whatIsGround;

    public Vector2 size;
    public Vector3 offset;
    private bool isPlayerDetected;

    /* =========================
     * COMBAT
     * ========================= */
    [Header("Combat")]
    public float retrieveDistance = 2.5f;
    public Transform attackPoint;
    public float attackRadius = 1f;
    public LayerMask whatIsPlayer;

    /* =========================
     * INTERNAL
     * ========================= */
    private Transform player;
    private Animator animator;
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider2D;
    private bool facingLeft = true;

    /* =========================
     * START
     * ========================= */
    void Start()
    {
        currentHealth = maxHealth;
        isEnemyDied = false;
        isInvincible = false;

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

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

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        CheckPlayerDetection();

        if (isPlayerDetected && player != null)
        {
            FacePlayer();

            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            if (distanceToPlayer > retrieveDistance)
            {
                animator.SetBool("Attack", false);
                Vector2 targetPos = new Vector2(player.position.x, transform.position.y);
                transform.position = Vector2.MoveTowards(transform.position, targetPos, chaseSpeed * Time.deltaTime);
            }
            else
            {
                animator.SetBool("Attack", true);
            }
        }
        else
        {
            animator.SetBool("Attack", false);
            PatrolEnemy();
            PatrolFlip();
        }
    }

    /* =========================
     * PATROL
     * ========================= */
    void PatrolEnemy()
    {
        transform.Translate(Vector2.left * Time.deltaTime * patrol_Speed);
    }

    void PatrolFlip()
    {
        RaycastHit2D hit = Physics2D.Raycast(detectPoint.position, Vector2.down, distance, whatIsGround);
        if (!hit)
        {
            Flip();
        }
    }

    void FacePlayer()
    {
        if (player.position.x > transform.position.x && facingLeft) Flip();
        else if (player.position.x < transform.position.x && !facingLeft) Flip();
    }

    void Flip()
    {
        facingLeft = !facingLeft;
        transform.eulerAngles = new Vector3(0f, facingLeft ? 0f : 180f, 0f);
    }

    void CheckPlayerDetection()
    {
        Collider2D col = Physics2D.OverlapBox(transform.position + offset, size, 0f, whatIsPlayer);
        isPlayerDetected = (col != null);
    }

    /* =========================
     * ATTACK (Animation Event)
     * ========================= */
    public void Attack()
    {
        Collider2D col = Physics2D.OverlapCircle(attackPoint.position, attackRadius, whatIsPlayer);
        if (col != null)
        {
            Player p = col.GetComponent<Player>();
            if (p != null)
                p.TakeDamage(1);
        }
    }

    /* =========================
     * TAKE DAMAGE
     * ========================= */
    public void TakeDamage(int damageAmount)
    {
        Debug.Log($"Enemy nhận {damageAmount} dame!");

        if (currentHealth <= 0 || isInvincible) return;

        currentHealth -= damageAmount;
        animator.SetTrigger("Hurt");

        ShowHealthBar();
        UpdateHealthBar();

        // ✨ Hiệu ứng nhấp nháy
        StartCoroutine(FlashEffect());

        // 🔢 Hiển thị damage text
        ShowDamageText(damageAmount);

        StartCoroutine(InvincibleCoroutine());

        if (currentHealth <= 0)
            Die();
    }

    /* =========================
     * FLASH EFFECT (Nhấp nháy)
     * ========================= */
    IEnumerator FlashEffect()
    {
        if (spriteRenderer == null) yield break;

        float flashDuration = 0.1f;
        int flashCount = Mathf.FloorToInt(invincibleTime / (flashDuration * 2));

        for (int i = 0; i < flashCount; i++)
        {
            // Đổi sang màu đỏ nhạt
            spriteRenderer.color = new Color(1f, 0.3f, 0.3f, 1f);
            yield return new WaitForSeconds(flashDuration);

            // Về màu gốc
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flashDuration);
        }

        // Đảm bảo về màu gốc
        spriteRenderer.color = originalColor;
    }

    /* =========================
     * DAMAGE TEXT (Số damage bay lên)
     * ========================= */
    void ShowDamageText(int damage)
    {
        if (damageTextPrefab == null)
        {
            Debug.LogWarning("⚠️ Chưa gán Damage Text Prefab!");
            return;
        }

        // Vị trí spawn (trên đầu enemy)
        Vector3 spawnPos = damageTextSpawnPoint != null
            ? damageTextSpawnPoint.position
            : transform.position + Vector3.up * 1.5f;

        // Tạo damage text
        GameObject damageTextObj = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);

        // Set text
        DamageText damageTextScript = damageTextObj.GetComponent<DamageText>();
        if (damageTextScript != null)
        {
            damageTextScript.SetDamage(damage);
        }
    }

    IEnumerator InvincibleCoroutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibleTime);
        isInvincible = false;
    }

    /* =========================
     * HEALTH BAR
     * ========================= */
    void ShowHealthBar()
    {
        if (healthBarRoot == null) return;

        healthBarRoot.SetActive(true);

        if (hideHealthCoroutine != null)
            StopCoroutine(hideHealthCoroutine);

        hideHealthCoroutine = StartCoroutine(HideHealthBarAfterDelay());
    }

    IEnumerator HideHealthBarAfterDelay()
    {
        yield return new WaitForSeconds(hideHealthBarDelay);
        healthBarRoot.SetActive(false);
    }

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

    /* =========================
     * DIE
     * ========================= */
    void Die()
    {
        if (isEnemyDied) return;

        isEnemyDied = true;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        boxCollider2D.enabled = false;

        animator.SetBool("Death", true);
        Destroy(gameObject, deathTime);
    }

    /* =========================
     * GIZMOS
     * ========================= */
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + offset, size);

        Gizmos.color = Color.red;
        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}