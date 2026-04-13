using System;
using UnityEngine;

public class ItemSpot : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform itemParent;


    [Header("Settings")]
    private Item item;
    public Item Item => item; // Tham chiếu đến item hiện tại đang ở vị trí này, nếu có


    // Hàm này được gọi khi một item được đặt vào vị trí này
    public void Populate(Item item)
    {
        this.item = item;

        // Đặt item làm con của ItemSpot để nó di chuyển cùng với ItemSpot
        item.transform.SetParent(itemParent);

        item.AssignSpot(this); // Gán ItemSpot cho item để item biết nó đang ở đâu
    }

    public void Clear()
    {
        item = null; // Xóa tham chiếu đến item
    }

    public void BumpDown()
    {
        animator.Play("Bump", 0, 0);
    }

    public bool IsEmplty() => item == null;


}
