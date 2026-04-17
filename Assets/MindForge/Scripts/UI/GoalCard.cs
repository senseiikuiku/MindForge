using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class GoalCard : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private GameObject checkmark;
    [SerializeField] private GameObject backFace;
    [SerializeField] private Animator animator;

    private void Start()
    {
        animator.enabled = false; // Vô hiệu hóa animator để tránh chạy animation khi khởi tạo
    }

    private void Update()
    {
        backFace.SetActive(Vector3.Dot(Vector3.forward, transform.forward) < 0); // Hiển thị mặt sau khi card quay lưng lại với camera)
    }

    public void Configure(int initialAmount, Sprite icon)
    {
        amountText.text = initialAmount.ToString();
        iconImage.sprite = icon;
    }

    public void UpdateAmount(int newAmount)
    {
        amountText.text = newAmount.ToString();

        Bump();
    }

    private void Bump()
    {
        LeanTween.cancel(gameObject); // Hủy tất cả các tween đang chạy trên gameObject này để tránh xung đột

        // Tải lại scale về 1 để đảm bảo hiệu ứng luôn bắt đầu từ kích thước gốc
        transform.localScale = Vector3.one;
        LeanTween.scale(gameObject, Vector3.one * 1.1f, .25f)
            .setLoopPingPong(1);
    }

    public void Complete()
    {
        animator.enabled = true; // Kích hoạt animator để có thể chạy animation

        checkmark.SetActive(true);
        amountText.text = "";

        animator.Play("Complete");
    }
}
