using System;
using UnityEngine;
using static UnityEditor.Progress;

public class InputManager : MonoBehaviour
{
    public static Action<Item> itemCliked;
    public static Action<Powerup> powerupClicked;

    [Header("Settings")]
    [SerializeField] private Material outlineMaterial;
    [SerializeField] private LayerMask powerupLayer;
    private Item currentItem;


    private void Update()
    {
        if (GameManager.Instance.IsGame())
        {
            HandleControl();
        }
    }

    private void HandleControl()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseDown();
        }

        // Kiểm tra nếu người dùng đang giữ chuột trái
        else if (Input.GetMouseButton(0))
        {
            HandleDrag();
        }
        // Kiểm tra nếu người dùng vừa thả chuột trái
        else if (Input.GetMouseButtonUp(0))
        {
            HandleMouseUp();
        }
    }

    private void HandleMouseDown()
    {
        // Nếu không có item nào đang được chọn, kiểm tra xem có click vào powerup không
        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),
                            out RaycastHit hit, 100, powerupLayer))
            return;

        if (hit.collider == null)
            return;

        // Kiểm tra xem collider có component Powerup không
        Powerup powerup = hit.collider.GetComponent<Powerup>();

        if (powerup == null)
        {
            Debug.LogWarning($"⚠️ Object '{hit.collider.name}' có layer Powerup nhưng không có Powerup component!");
            return;
        }

        powerupClicked?.Invoke(powerup); // Bắn event cho PowerupManager
    }

    private void HandleDrag()
    {
        // Bắn tia từ camera qua vị trí chuột
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),
                            out RaycastHit hit, 100))
        {
            // Kiểm tra null
            if (hit.collider == null)
            {
                DeselectCurrentItem();
                return;
            }

            // Kiểm tra có parent không (vì Item component ở parent)
            if (hit.collider.transform.parent == null)
            {
                DeselectCurrentItem();
                return;
            }

            // Kiểm tra parent có component Item không
            if (!hit.collider.transform.parent.TryGetComponent(out Item item))
            {
                DeselectCurrentItem();
                return;
            }

            // Lưu item và highlight
            DeselectCurrentItem();// Bỏ chọn item cũ
            currentItem = item;// Lưu item mới
            currentItem.Select(outlineMaterial);// Thêm outline
        }
    }

    private void DeselectCurrentItem()
    {
        if (currentItem != null)
        {
            currentItem.Deselect(); // Nếu có một item đang được chọn, bỏ chọn nó
        }

        currentItem = null;
    }

    private void HandleMouseUp()
    {
        if (currentItem == null) return;

        currentItem.Deselect();           // Xóa outline
        itemCliked?.Invoke(currentItem); // Bắn event cho ItemSpotsManager
        currentItem = null;               // Reset
    }
}
