using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxHealth = 3;
    public float deathTime = 2f;
    public float patrol_Speed = 2.2f;
    public Transform detectPoint;
    public float distance = .3f;
    public LayerMask whatIsGround;

    private Transform player;
    public float chaseSpeed = 4f;
    public float retrieveDistance = 1.5f;

    private Animator animator;
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider2D;
    private bool facingLeft;
    private bool isPlayerInAttackRange;

    public Vector2 size;
    public Vector3 offset;
    public Transform attackPoint;
    public float attackRadius = 1f;
    public LayerMask whatIsPlayer;
    private bool isEnemyDied;

    void Start()
    {
        facingLeft = true;
        isEnemyDied = false;
        isPlayerInAttackRange = false;
        rb = this.gameObject.GetComponent<Rigidbody2D>();
        animator = this.gameObject.GetComponent<Animator>();
        boxCollider2D = this.gameObject.GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        if (maxHealth <= 0 && isEnemyDied == false)
        {
            rb.gravityScale = 0f;
            isEnemyDied = true;
            Die();
            return;
        }

        if (maxHealth <= 0)
        {
            rb.gravityScale = 0f;
            return;
        }

        Flip();
        CheckCollision();

        if (GameObject.Find("Player") == null)
        {
            PatrolEnemy();
            animator.SetBool("Attack", false);
            return;
        }

        player = GameObject.FindGameObjectWithTag("Player").transform;


        if (isPlayerInAttackRange == true)
        {
            if (player.position.x > transform.position.x && facingLeft == true)
            {
                transform.eulerAngles = new Vector3(0f, -180f, 0f);
                facingLeft = false;
            }
            else if (player.position.x < transform.position.x && facingLeft == false)
            {
                transform.eulerAngles = new Vector3(0f, 0f, 0f);
                facingLeft = true;
            }

            Vector2 targetPos = new Vector2(player.position.x, transform.position.y);

            if (Vector2.Distance(transform.position, player.position) > retrieveDistance)
            {
                animator.SetBool("Attack", false);
                transform.position = Vector2.MoveTowards(transform.position, targetPos, chaseSpeed * Time.deltaTime);
            }
            else
            {
                animator.SetBool("Attack", true);
            }
        }
        else
        {
            PatrolEnemy();
            animator.SetBool("Attack", false);
        }
    }

    void PatrolEnemy()
    {
        transform.Translate(Vector2.left * Time.deltaTime * patrol_Speed);
    }

    void CheckCollision()
    {
        Collider2D collInfo = Physics2D.OverlapBox(transform.position + offset, size, 0f, whatIsPlayer);

        if (collInfo)
        {
            isPlayerInAttackRange = true;
        }
        else
        {
            isPlayerInAttackRange = false;
        }
    }

    public void Attack()
    {
        Collider2D collInfo = Physics2D.OverlapCircle(attackPoint.position, attackRadius, whatIsPlayer);

        if (collInfo)
        {
            if (collInfo.gameObject.GetComponent<Player>() != null)
            {
                // Đã comment dòng gây lỗi này lại theo yêu cầu
                // collInfo.gameObject.GetComponent<Player>().TakeDamage(1);
                Debug.Log("Enemy hit Player (TakeDamage temporarily disabled)");
            }
        }
    }

    void Flip()
    {
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
        if (maxHealth <= 0)
        {
            return;
        }
        maxHealth -= damageAmount;
        animator.SetTrigger("Hurt");
        // Đã bỏ CameraShake và Audio
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

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + offset, size);
    }

    void Die()
    {
        Debug.Log(" Enemy Died!");
        isEnemyDied = true;
        rb.gravityScale = 0f;
        animator.SetBool("Death", true);
        boxCollider2D.enabled = false;
        // Đã bỏ CameraShake
        Destroy(this.gameObject, deathTime);
    }
}