using System;
using UnityEngine;
using static UnityEditor.Progress;

public class InputManager : MonoBehaviour
{
    public static Action<Item> itemCliked;

    [Header("Settings")]
    [SerializeField] private Material outlineMaterial;
    private Item currentItem;


    private void Update()
    {
        // Kiểm tra nếu người dùng đang giữ chuột trái
        if (Input.GetMouseButton(0))
        {
            HandleDrag();
        }
        // Kiểm tra nếu người dùng vừa thả chuột trái
        else if (Input.GetMouseButtonUp(0))
        {
            HandleMouseUp();
        }
    }

    private void HandleDrag()
    {
        // Bắn một tia ray từ camera đến vị trí chuột và kiểm tra xem nó có va chạm với bất kỳ collider nào không
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100))
        {
            if (hit.collider == null)
            {
                DeselectCurrentItem();

                return;
            }

            if (hit.collider.transform.parent == null)
            {
                DeselectCurrentItem();
                Debug.Log("Hit object does not have a parent.");
                return;
            }

            // Kiểm tra nếu collider của đối tượng va chạm có một component Item trong parent của nó
            if (!hit.collider.transform.parent.TryGetComponent(out Item item))
            {
                DeselectCurrentItem();
                Debug.Log("Hit object does not have an Item component.");
                return;
            }

            DeselectCurrentItem();

            currentItem = item;

            currentItem.Select(outlineMaterial);
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
        if (currentItem == null)
            return;

        currentItem.Deselect();

        // Bắn sự kiện itemCliked với currentItem đã được gán trong quá trình kéo
        itemCliked?.Invoke(currentItem);

        // Sau khi đã xử lý xong, đặt currentItem về null để chuẩn bị cho lần click tiếp theo
        currentItem = null;
    }
}
