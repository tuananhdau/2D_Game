using UnityEngine;
using UnityEngine.UI;
public class HealthPath : MonoBehaviour
{
    public Image _HealthPath;
    public void UpdateHealthPath(float Health,float MaxHealth)
    {
        _HealthPath.fillAmount=Health/MaxHealth;
    }
}
