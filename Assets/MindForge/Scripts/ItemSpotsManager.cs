using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpotsManager : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private Transform itemSpotsParent;
    private ItemSpot[] spots;// Mảng lưu trữ danh sách các ô cụ thể


    [Header("Settings")]
    [SerializeField] private Vector3 itemLocalPositionOnSpot;
    [SerializeField] private Vector3 itemLocalScaleOnSpot;
    public bool isBusy;// Biến khóa, ngăn người chơi click liên tục khi đang xử lý di chuyển

    [Header("Data")]
    // Từ điển lưu trữ dữ liệu các vật phẩm đã được nhặt, phân loại theo Tên (Enum)
    private Dictionary<EItemName, ItemMergeData> itemMergeDataDictionary = new Dictionary<EItemName, ItemMergeData>();

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.2f;
    [SerializeField] private LeanTweenType animaitonEasing;

    private void Awake()
    {
        InputManager.itemCliked += OnItemClicked;
        StoreSpots();
    }

    private void OnDestroy()
    {
        InputManager.itemCliked -= OnItemClicked;
    }

    private void OnItemClicked(Item item)
    {
        if (isBusy)
        {
            Debug.Log("ItemSpotsManager is busy. Please wait.");
            return;
        }

        if (!IsFreeSpotAvailable())
        {
            Debug.Log("No free item spot available.");
            return;
        }

        // we've now busy
        isBusy = true;

        HandleItemClicked(item);
    }

    private void HandleItemClicked(Item item)
    {
        // Nếu loại vật phẩm này ĐÃ CÓ trên khay (đã có trong Dictionary)
        if (itemMergeDataDictionary.ContainsKey(item.ItemName))
            HandleItemMergeDataFound(item); // Xử lý logic chèn vào cạnh nhau
        else
            MoveItemToFirstFreeSpot(item); // Nếu chưa có, cứ bỏ vào ô trống đầu tiên
    }

    // Hàm xử lý khi đã tìm thấy 'ItemMergeData' cho món đồ này trong Dictionary, tức là đã có món đồ cùng loại trên khay
    private void HandleItemMergeDataFound(Item item)
    {
        // Tìm xem 'Vị trí lý tưởng' cho món đồ này là ô nào
        ItemSpot targetSpot = GetIdealSpotFor(item);

        // Cập nhật dữ liệu vào Dictionary (thêm món đồ này vào danh sách cùng loại)
        itemMergeDataDictionary[item.ItemName].Add(item);

        // Thử đưa món đồ vào ô đó
        TryMoveItemToIdealSpot(item, targetSpot);
    }

    // Hàm tìm 'Vị trí lý tưởng' cho món đồ này, tức là ô đứng ngay cạnh món đồ cùng loại cuối cùng trên khay
    private ItemSpot GetIdealSpotFor(Item item)
    {
        // Lấy danh sách các món đồ cùng loại đang có trên khay
        List<Item> items = itemMergeDataDictionary[item.ItemName].items;

        // Tạo danh sách các Ô (Spots) mà những món đồ đó đang đứng
        List<ItemSpot> itemSpots = new List<ItemSpot>();
        for (int i = 0; i < items.Count; i++)
        {
            itemSpots.Add(items[i].Spot);
        }

        // Sắp xếp các ô này theo thứ tự dựa trên Index trong Unity
        if (itemSpots.Count >= 2)
        {
            itemSpots.Sort((a, b) => b.transform.GetSiblingIndex().CompareTo(a.transform.GetSiblingIndex()));
        }

        // Lấy Index của ô cuối cùng trong nhóm đồ cùng loại, rồi cộng thêm 1
        // Mục tiêu: Đứng ngay sát bên phải món đồ cùng loại cuối cùng
        int idealSpotIndex = itemSpots[0].transform.GetSiblingIndex() + 1;

        return spots[idealSpotIndex]; // Trả về ô mục tiêu
    }

    private void TryMoveItemToIdealSpot(Item item, ItemSpot idealSpot)
    {
        // Nếu ô lý tưởng đã có món đồ khác loại đang đứng
        if (!idealSpot.IsEmplty())
        {
            // Đẩy món đồ đó sang một bên
            HandleIdealSpotFull(item, idealSpot);
            return;
        }

        // Nếu ô đó trống, di chuyển vào ngay
        MoveItemToSpot(item, idealSpot, () => HandleItemReachedSpot(item));
    }

    private void MoveItemToSpot(Item item, ItemSpot targetSpot, Action completeCallBack)
    {
        // Đặt item vào vị trí trống
        targetSpot.Populate(item);

        // Reset local position and scale
        LeanTween.moveLocal(item.gameObject, itemLocalPositionOnSpot, animationDuration)
            .setEase(animaitonEasing);

        LeanTween.scale(item.gameObject, itemLocalScaleOnSpot, animationDuration)
            .setEase(animaitonEasing);

        LeanTween.rotateLocal(item.gameObject, Vector3.zero, animationDuration)
            .setOnComplete(completeCallBack);

        // Disable shadows 
        item.DisableShadows();

        // Disable physics
        item.DisablePhysics();

    }

    // Hàm xử lý khi món đồ đã được đặt vào một ô nào đó trên khay, kiểm tra xem có thể gộp với những món đồ cùng loại khác không, nếu có thì gộp, nếu không thì kiểm tra thua cuộc
    private void HandleItemReachedSpot(Item item, bool checkForMerge = true)
    {
        item.Spot.BumpDown(); // Hiệu ứng nhún nhẹ khi món đồ đã vào vị trí

        if (!checkForMerge)
        {
            return;
        }

        if (itemMergeDataDictionary[item.ItemName].CanMergeItems())
        {
            MergeItems(itemMergeDataDictionary[item.ItemName]);
        }
        else
        {
            CheckForGameOver();
        }
    }

    // Hàm xử lý logic gộp các món đồ cùng loại lại với nhau, xóa chúng khỏi khay và Dictionary
    private void MergeItems(ItemMergeData itemMergeData)
    {
        List<Item> items = itemMergeData.items;

        // Xóa các item merge data từ Dictionary
        itemMergeDataDictionary.Remove(itemMergeData.itemName);

        for (int i = 0; i < items.Count; i++)
        {
            // Xóa các item trên khay
            items[i].Spot.Clear();
            Destroy(items[i].gameObject);
        }

        if (itemMergeDataDictionary.Count <= 0)
        {
            isBusy = false;
        }
        else
        {
            // Di chuyển tất cả các món đồ còn lại trên khay sang bên trái hết
            MoveAllItemsToTheLeft(HandleAllItemsMovedToTheLeft);
        }

        //isBusy = false; // Mở khóa để người chơi có thể click tiếp
    }

    private void MoveAllItemsToTheLeft(Action completeCallback)
    {
        bool callbackTriggered = false; // Biến cờ để theo dõi xem callback đã được gọi chưa

        // Duyệt từ trái sang phải, nếu ô nào có món đồ thì đẩy nó sang bên cạnh bên trái, cứ thế tiếp tục đến hết mảng
        for (int i = 3; i < spots.Length; i++)
        {
            // Nếu ô này có món đồ, thì đẩy nó sang bên cạnh bên trái
            ItemSpot spot = spots[i];

            if (spot.IsEmplty())
            {
                continue;
            }

            // Nếu đã đi đến đầu mảng mà vẫn chưa tìm được ô trống, tức là không thể đẩy thêm nữa, thì dừng lại
            Item item = spot.Item;

            // Di chuyển món đồ sang ô bên cạnh
            ItemSpot targetSpot = spots[i - 3];

            // Nếu ô bên cạnh đã có món đồ khác loại đang đứng, thì dừng lại, không đẩy nữa
            if (!targetSpot.IsEmplty())
            {
                Debug.LogWarning($"{targetSpot.name} is Full");
                isBusy = false;
                return;
            }

            // Xóa món đồ khỏi ô hiện tại
            spot.Clear();

            completeCallback += () => HandleItemReachedSpot(item, false);

            // Di chuyển món đồ sang ô bên cạnh
            MoveItemToSpot(item, targetSpot, completeCallback);

            callbackTriggered = true;
        }

        // Nếu sau khi đã cố gắng đẩy tất cả các món đồ sang trái hết mức có thể mà callback vẫn chưa được gọi, thì gọi nó ngay bây giờ
        if (!callbackTriggered)
        {
            completeCallback?.Invoke();
        }
    }

    private void HandleAllItemsMovedToTheLeft()
    {
        isBusy = false; // Mở khóa để người chơi có thể click tiếp
    }

    private void HandleIdealSpotFull(Item item, ItemSpot idealSpot)
    {
        MoveAllItemsToTheRightFrom(idealSpot, item);
    }

    private void MoveAllItemsToTheRightFrom(ItemSpot idealSpot, Item itemToPlace)
    {
        int spotIndex = idealSpot.transform.GetSiblingIndex();

        for (int i = spots.Length - 2; i >= spotIndex; i--)
        {
            ItemSpot spot = spots[i];

            if (spot.IsEmplty())
            {
                continue;
            }

            // Nếu đã đi đến cuối mảng mà vẫn chưa tìm được ô trống, tức là không thể đẩy thêm nữa, thì dừng lại
            Item item = spot.Item;

            spot.Clear();

            // Đẩy món đồ này sang một bên (sang phải)
            ItemSpot targetSpot = spots[i + 1];

            if (!targetSpot.IsEmplty())
            {
                Debug.LogError("ERROR, this should not happen, we should have already checked for free spot before trying to move items");
                isBusy = false;
                return;
            }

            // Di chuyển món đồ sang ô bên cạnh
            MoveItemToSpot(item, targetSpot, () => HandleItemReachedSpot(item, false));
        }

        // Cuối cùng, khi đã đẩy tất cả các món đồ sang phải hết mức có thể, thì đặt món đồ mới vào vị trí lý tưởng ban đầu
        MoveItemToSpot(itemToPlace, idealSpot, () => HandleItemReachedSpot(itemToPlace));
    }

    // Di chuyển item đến vị trí trống đầu tiên
    private void MoveItemToFirstFreeSpot(Item item)
    {
        ItemSpot targetSpot = GetFreeSpot();

        if (targetSpot == null)
        {
            Debug.Log("No free item spot found.");
            return;
        }

        CreateItemMergeData(item);

        MoveItemToSpot(item, targetSpot, () => HandleFirstItemReachedSpot(item));
    }

    // Hàm xử lý khi món đồ đã được đặt vào một ô nào đó trên khay, kiểm tra xem có thua cuộc không
    private void HandleFirstItemReachedSpot(Item item)
    {
        item.Spot.BumpDown(); // Hiệu ứng nhún nhẹ khi món đồ đã vào vị trí
        CheckForGameOver();
    }

    private void CheckForGameOver()
    {
        // Nếu không còn ô nào trống -> Thua cuộc
        if (GetFreeSpot() == null)
        {
            Debug.Log("Game Over!");
        }
        else
        {
            // Nếu vẫn còn chỗ, mở khóa 'isBusy' để người chơi click món đồ tiếp theo
            isBusy = false;
        }
    }

    private void CreateItemMergeData(Item item)
    {
        itemMergeDataDictionary.Add(item.ItemName, new ItemMergeData(item));
    }

    // Tìm vị trí trống đầu tiên trong mảng spots
    private ItemSpot GetFreeSpot()
    {
        for (int i = 0; i < spots.Length; i++)
        {
            if (spots[i].IsEmplty())
            {
                return spots[i];
            }
        }
        return null;
    }

    private void StoreSpots()
    {
        // Đếm số lượng con của 'itemSpotsParent' để tạo mảng
        spots = new ItemSpot[itemSpotsParent.childCount];
        // Lấy Component ItemSpot của từng ô con và lưu lại
        for (int i = 0; i < itemSpotsParent.childCount; i++)
        {
            spots[i] = itemSpotsParent.GetChild(i).GetComponent<ItemSpot>();
        }
    }

    private bool IsFreeSpotAvailable()
    {
        // Duyệt qua mảng spots, nếu có một ô nào đó trống, trả về true
        for (int i = 0; i < spots.Length; i++)
        {
            if (spots[i].IsEmplty())
            {
                return true;
            }
        }
        return false;
    }
}
