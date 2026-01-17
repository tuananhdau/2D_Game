using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuFunction : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    Image img;

    Color normalColor = Color.white;
    Color hoverColor = new Color(1f, 0.95f, 0.7f); // vàng nhạt WC3

    void Awake()
    {
        img = GetComponent<Image>();
    }

    // Hover vào → sáng
    public void OnPointerEnter(PointerEventData eventData)
    {
        img.color = hoverColor;
    }

    // Rời chuột → trở lại bình thường
    public void OnPointerExit(PointerEventData eventData)
    {
        img.color = normalColor;
    }

    // Nút Single Player
    public void SinglePlayer()
    {
        SceneManager.LoadScene(1);
    }

    // Nút Exit
    public void Exit()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}
