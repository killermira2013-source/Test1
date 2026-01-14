using UnityEngine;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("Текстовые поля")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI ammoText;

    public static GameUI Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void UpdateHealth(float currentHp)
    {
        if (healthText)
            healthText.text = $"HP: {currentHp:0}";
    }

    public void UpdateAmmo(int current, int max)
    {
        if (ammoText)
            ammoText.text = $"{current} / {max}";
    }
}