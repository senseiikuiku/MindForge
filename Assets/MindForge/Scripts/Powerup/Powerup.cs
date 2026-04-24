using UnityEngine;
using TMPro;

public enum EPowerupType
{
    Vaccum = 0,
    Spring = 1,
    Fan = 2,
    Freeze = 3,
}


public abstract class Powerup : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private EPowerupType type;
    public EPowerupType Type => type;


    [Header("Elements")]
    [SerializeField] private TextMeshPro amountText;
    [SerializeField] private GameObject videoIcon;

    private Collider powerupCollider;

    protected virtual void Awake()
    {
        powerupCollider = GetComponent<Collider>();

        if (powerupCollider == null)
        {
            Debug.LogWarning($"⚠️ {name} không có Collider! Powerup sẽ không thể click được.");
        }
    }

    public void UpdateVisuals(int amount)
    {
        videoIcon.SetActive(amount <= 0);

        amountText.gameObject.SetActive(amount > 0);
        amountText.text = amount.ToString();
    }

    public void SetInteractable(bool interactable)
    {
        if (powerupCollider != null)
        {
            powerupCollider.enabled = interactable;
        }
    }
}
