using System;
using UnityEngine;

public class ItemSpotsManager : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private Transform itemSpotsParent;
    private ItemSpot[] spots;


    [Header("Settings")]
    [SerializeField] private Vector3 itemLocalPositionOnSpot;
    [SerializeField] private Vector3 itemLocalScaleOnSpot;

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
        if (!IsFreeSpotAvailable())
        {
            Debug.Log("No free item spot available.");
            return;
        }

        HandleItemClicked(item);
    }

    // Xử lý khi một item được click
    private void HandleItemClicked(Item item)
    {
        MoveItemToFirstFreeSpot(item);

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

        // Đặt item vào vị trí trống
        targetSpot.Populate(item);


        // Reset local position and scale
        item.transform.localPosition = itemLocalPositionOnSpot;
        item.transform.localScale = itemLocalScaleOnSpot;
        item.transform.localRotation = Quaternion.identity;

        // Disable shadows 
        item.DisableShadows();

        // Disable physics
        item.DisablePhysics();
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
        // Lấy tất cả các ItemSpot từ itemSpotsParent và lưu vào mảng spots
        spots = new ItemSpot[itemSpotsParent.childCount];

        for (int i = 0; i < itemSpotsParent.childCount; i++)
        {
            spots[i] = itemSpotsParent.GetChild(i).GetComponent<ItemSpot>();
        }
    }

    // Kiểm tra xem có vị trí trống nào không
    private bool IsFreeSpotAvailable()
    {
        for (int i = 0; i < spots.Length; i++)
        {
            // Nếu có một vị trí trống, trả về true
            if (spots[i].IsEmplty())
            {
                return true;
            }
        }
        return false;
    }
}
