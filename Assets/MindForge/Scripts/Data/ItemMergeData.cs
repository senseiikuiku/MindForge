using System.Collections.Generic;
using UnityEngine;

public struct ItemMergeData
{
    public EItemName itemName;        // Tên loại vật phẩm
    public List<Item> items;          // Danh sách các item cùng loại trên khay

    // Constructor: Tạo data mới khi item đầu tiên được đặt lên khay
    public ItemMergeData(Item firstItem)
    {
        itemName = firstItem.ItemName;
        items = new List<Item>();
        items.Add(firstItem);
    }

    // Thêm item cùng loại vào danh sách
    public void Add(Item item)
    {
        items.Add(item);
        Debug.Log($"full items {items.Count}");
    }

    public void Remove(Item item)
    {
        items.Remove(item);
    }

    // Kiểm tra xem có đủ 3 item để merge không
    public bool CanMergeItems()
    {
        return items.Count >= 3;
    }
}
