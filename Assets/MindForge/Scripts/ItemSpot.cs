using System;
using UnityEngine;

public class ItemSpot : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform itemParent;


    [Header("Settings")]
    private Item item; // Item hiện tại đang ở vị trí này
    public Item Item => item;


    // Hàm này được gọi khi một item được đặt vào vị trí này
    public void Populate(Item item)
    {
        this.item = item; // Để vị trí biết item nào đang ở đây

        // Đặt item vào vị trí của ItemSpot
        item.transform.SetParent(itemParent);

        item.AssignSpot(this); // Gán ItemSpot cho item để item biết nó đang ở đâu
    }

    // Hàm này được gọi khi một item được lấy ra khỏi vị trí này
    public void Clear()
    {
        item = null; // Xóa tham chiếu đến item
    }

    public void BumpDown()
    {
        animator.Play("Bump", 0, 0);
    }

    // Hàm kiểm tra xem vị trí này trống hay có item nào đó đang ở đây
    public bool IsEmpty() => item == null;


}
