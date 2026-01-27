using UnityEngine;

public class FireTrap : MonoBehaviour
{
    [Header("Cài đặt sát thương")]
    public int damage = 1; // Lượng máu sẽ trừ (ví dụ 1 tim)

    // Hàm này tự động chạy khi có vật thể đi vào vùng Collider của lửa
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Kiểm tra xem vật va chạm có phải là Player không
        // (Bằng cách tìm xem nó có gắn script "Player" không)
        Player player = collision.GetComponent<Player>();

        if (player != null)
        {
            // 2. Gọi hàm trừ máu bên script Player
            player.TakeDamage(damage);
            
            Debug.Log("Player bị cháy! Á á á!");
        }
    }
}