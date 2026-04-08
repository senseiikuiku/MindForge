using System;
using UnityEngine;

public class ItemSpot : MonoBehaviour
{
    [Header("Settings")]
    private Item item;

    // Hàm này được gọi khi một item được đặt vào vị trí này
    public void Populate(Item item)
    {
        this.item = item;

        // Đặt item làm con của ItemSpot để nó di chuyển cùng với ItemSpot
        item.transform.SetParent(transform);
    }

    public bool IsEmplty() => item == null;


}
