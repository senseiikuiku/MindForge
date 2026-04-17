using UnityEngine;
using TMPro;

public class GoalCard : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private TextMeshProUGUI amountText;

    public void Configure(int initialAmount)
    {
        amountText.text = initialAmount.ToString();
    }

    public void UpdateAmount(int newAmount)
    {
        amountText.text = newAmount.ToString();
    }

    public void Complete()
    {
        gameObject.SetActive(false);
    }
}
