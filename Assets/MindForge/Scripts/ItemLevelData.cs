using UnityEngine;
using NaughtyAttributes;

[System.Serializable] // Dùng để cho phép hiển thị trong Inspector của Unity
public struct ItemLevelData
{
    public Item itemPrefab;
    public bool isGoal;
    [ValidateInput("ValidateAmount", "Amount must be a multiple of 3")]
    [AllowNesting]
    [Range(0, 100)] // Giới hạn giá trị của amount từ 0 đến 100
    public int amount;

    // Hàm kiểm tra nếu amount là bội số của 3
    private bool ValidateAmount()
    {
        return amount % 3 == 0; // Kiểm tra nếu amount là bội số của 3
    }
}
