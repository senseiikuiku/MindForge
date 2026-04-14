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
    // Dictionary lưu trữ dữ liệu các món đồ cùng loại đang có trên khay, để dễ dàng xử lý logic gộp chúng lại với nhau
    private Dictionary<EItemName, ItemMergeData> itemMergeDataDictionary = new Dictionary<EItemName, ItemMergeData>();

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.2f;
    [SerializeField] private LeanTweenType animationEasing;

    [Header("Actions")]
    public static Action<List<Item>> mergeStarted;


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
        // 1. Kiểm tra đang bận xử lý không
        if (isBusy)
        {
            Debug.Log("ItemSpotsManager is busy. Please wait.");
            return;
        }

        // 2. Kiểm tra còn ô trống không
        if (!IsFreeSpotAvailable())
        {
            Debug.Log("No free item spot available.");
            return;
        }

        // 3. Khóa lại và xử lý
        isBusy = true;
        HandleItemClicked(item);
    }

    private void HandleItemClicked(Item item)
    {
        // Nếu loại item này ĐÃ CÓ trên khay
        if (itemMergeDataDictionary.ContainsKey(item.ItemName))
            HandleItemMergeDataFound(item);  // Đặt cạnh item cùng loại
        else
            MoveItemToFirstFreeSpot(item);   // Đặt vào ô trống đầu tiên
    }

    // Hàm xử lý khi đã tìm thấy 'ItemMergeData' cho item này trong Dictionary
    private void HandleItemMergeDataFound(Item item)
    {
        // Tìm xem 'Vị trí lý tưởng' cho item này là ô nào
        ItemSpot targetSpot = GetIdealSpotFor(item);

        // Cập nhật dữ liệu vào Dictionary (thêm item này vào danh sách cùng loại)
        itemMergeDataDictionary[item.ItemName].Add(item);

        // Thử đưa món đồ vào ô đó
        TryMoveItemToIdealSpot(item, targetSpot);
    }

    // Hàm tìm 'Vị trí lý tưởng' cho item này
    private ItemSpot GetIdealSpotFor(Item item)
    {
        // 1. Lấy danh sách items cùng loại đang có trên khay
        List<Item> items = itemMergeDataDictionary[item.ItemName].items;

        // 2. Tạo danh sách các ô mà items đó đang đứng
        List<ItemSpot> itemSpots = new List<ItemSpot>();
        for (int i = 0; i < items.Count; i++)
        {
            itemSpots.Add(items[i].Spot);
        }

        // 3. Sắp xếp các ô theo index (từ phải sang trái)
        if (itemSpots.Count >= 2)
        {
            itemSpots.Sort((a, b) =>
                b.transform.GetSiblingIndex().CompareTo(a.transform.GetSiblingIndex())
            );
        }

        // 4. Lấy ô bên phải của item cuối cùng trong nhóm
        int idealSpotIndex = itemSpots[0].transform.GetSiblingIndex() + 1;

        return spots[idealSpotIndex];
    }

    private void TryMoveItemToIdealSpot(Item item, ItemSpot idealSpot)
    {
        // Nếu ô lý tưởng đã có món đồ khác loại đang đứng
        if (!idealSpot.IsEmpty())
        {
            // Đẩy món đồ đó sang một bên
            HandleIdealSpotFull(item, idealSpot);
            return;
        }

        // Nếu ô đó trống, di chuyển vào ngay
        MoveItemToSpot(item, idealSpot, () => HandleItemReachedSpot(item));
    }

    private void MoveItemToSpot(Item item, ItemSpot targetSpot, Action completeCallback)
    {
        // 1. Đặt item vào ô
        targetSpot.Populate(item);

        // 2. Animation di chuyển vị trí (dùng LeanTween)
        LeanTween.moveLocal(item.gameObject, itemLocalPositionOnSpot, animationDuration)
            .setEase(animationEasing);

        // 3. Animation scale
        LeanTween.scale(item.gameObject, itemLocalScaleOnSpot, animationDuration)
            .setEase(animationEasing);

        // 4. Animation xoay về 0
        LeanTween.rotateLocal(item.gameObject, Vector3.zero, animationDuration)
            .setOnComplete(completeCallback);  // Callback khi hoàn thành

        // 5. Tắt shadows và physics
        item.DisableShadows();
        item.DisablePhysics();
    }

    // Hàm xử lý khi món đồ đã được đặt vào một ô nào đó trên khay, kiểm tra xem có thể gộp với những món đồ cùng loại khác không, nếu có thì gộp, nếu không thì kiểm tra thua cuộc
    private void HandleItemReachedSpot(Item item, bool checkForMerge = true)
    {
        item.Spot.BumpDown(); // Hiệu ứng nhún nhẹ khi món đồ đã vào vị trí

        if (!checkForMerge)
        {
            return;  // Không kiểm tra merge (dùng khi đang sắp xếp lại)
        }

        // Kiểm tra có đủ 3 items cùng loại không
        if (itemMergeDataDictionary[item.ItemName].CanMergeItems())
        {
            MergeItems(itemMergeDataDictionary[item.ItemName]);
        }
        else
        {
            CheckForGameOver();  // Kiểm tra thua cuộc
        }
    }

    // Hàm xử lý logic gộp các món đồ cùng loại lại với nhau, xóa chúng khỏi khay và Dictionary
    private void MergeItems(ItemMergeData itemMergeData)
    {
        List<Item> items = itemMergeData.items;

        // 1. Xóa khỏi Dictionary
        itemMergeDataDictionary.Remove(itemMergeData.itemName);

        // 2. Xóa khỏi khay (chỉ Clear, chưa Destroy)
        for (int i = 0; i < items.Count; i++)
        {
            items[i].Spot.Clear();
        }

        // 3. Bắn event cho MergeManager để làm hiệu ứng
        mergeStarted?.Invoke(items);

        // 4. Đẩy các items còn lại sang trái
        MoveAllItemsToTheLeft(HandleAllItemsMovedToTheLeft);
    }

    private void MoveAllItemsToTheLeft(Action completeCallback)
    {
        // Duyệt từ index 3 đến hết
        for (int i = 3; i < spots.Length; i++)
        {
            ItemSpot spot = spots[i];

            if (spot.IsEmpty()) continue;

            Item item = spot.Item;
            ItemSpot targetSpot = spots[i - 3];  // Đẩy sang trái 3 ô

            // Nếu ô bên trái đã có item khác → Lỗi logic
            if (!targetSpot.IsEmpty())
            {
                Debug.LogWarning($"{targetSpot.name} is Full");
                isBusy = false;
                return;
            }

            // Xóa khỏi ô hiện tại
            spot.Clear();

            // ⚠️ BUG: completeCallback bị gọi nhiều lần!
            completeCallback += () => HandleItemReachedSpot(item, false);

            // Di chuyển item
            MoveItemToSpot(item, targetSpot, completeCallback);
        }
    }

    //private void MoveAllItemsToTheLeft(Action completeCallback)
    //{
    //    int itemsToMove = 0;
    //    int itemsMoved = 0;

    //    // Đếm số items cần di chuyển
    //    for (int i = 3; i < spots.Length; i++)
    //    {
    //        if (!spots[i].IsEmpty()) itemsToMove++;
    //    }

    //    for (int i = 3; i < spots.Length; i++)
    //    {
    //        ItemSpot spot = spots[i];
    //        if (spot.IsEmpty()) continue;

    //        Item item = spot.Item;
    //        ItemSpot targetSpot = spots[i - 3];
    //        spot.Clear();

    //        MoveItemToSpot(item, targetSpot, () =>
    //        {
    //            HandleItemReachedSpot(item, false);
    //            itemsMoved++;

    //            // Chỉ gọi callback khi tất cả items đã di chuyển xong
    //            if (itemsMoved == itemsToMove)
    //            {
    //                completeCallback?.Invoke();
    //            }
    //        });
    //    }

    //    // Nếu không có item nào cần di chuyển
    //    if (itemsToMove == 0)
    //    {
    //        completeCallback?.Invoke();
    //    }
    //}

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
        // Lấy index của ô lý tưởng
        int spotIndex = idealSpot.transform.GetSiblingIndex();

        // Duyệt từ phải sang trái (cuối mảng về vị trí lý tưởng)
        for (int i = spots.Length - 2; i >= spotIndex; i--)
        {
            ItemSpot spot = spots[i];

            if (spot.IsEmpty()) continue;  // Bỏ qua ô trống

            Item item = spot.Item;
            spot.Clear();  // Xóa khỏi ô hiện tại

            // Đẩy sang phải 1 ô
            ItemSpot targetSpot = spots[i + 1];

            if (!targetSpot.IsEmpty())
            {
                Debug.LogError("ERROR: Target spot should be empty!");
                isBusy = false;
                return;
            }

            // Di chuyển item
            MoveItemToSpot(item, targetSpot, () => HandleItemReachedSpot(item, false));
        }

        // Cuối cùng đặt item mới vào vị trí lý tưởng
        MoveItemToSpot(itemToPlace, idealSpot, () => HandleItemReachedSpot(itemToPlace));
    }

    // Di chuyển item đến vị trí trống đầu tiên
    private void MoveItemToFirstFreeSpot(Item item)
    {
        // Tìm ô trống đầu tiên
        ItemSpot targetSpot = GetFreeSpot();

        if (targetSpot == null)
        {
            Debug.Log("No free item spot found.");
            return;
        }

        // Tạo data mới cho loại item này
        CreateItemMergeData(item);

        // Di chuyển item vào ô
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
        // Thêm entry mới vào Dictionary
        itemMergeDataDictionary.Add(item.ItemName, new ItemMergeData(item));
    }

    // Tìm vị trí trống đầu tiên trong mảng spots
    private ItemSpot GetFreeSpot()
    {
        for (int i = 0; i < spots.Length; i++)
        {
            if (spots[i].IsEmpty())
            {
                return spots[i];
            }
        }
        return null;
    }

    private void StoreSpots()
    {
        // Đếm số con của parent
        spots = new ItemSpot[itemSpotsParent.childCount];

        // Lưu component ItemSpot của từng con
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
            if (spots[i].IsEmpty())
            {
                return true;
            }
        }
        return false;
    }
}
